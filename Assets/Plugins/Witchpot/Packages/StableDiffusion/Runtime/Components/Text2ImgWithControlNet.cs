using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

using ControlNetUnitRequest = Witchpot.Runtime.StableDiffusion.StableDiffusionWebUIClient.Post.SdApi.V1.Extension.ControlNet.UnitRequest;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Witchpot.Runtime.StableDiffusion
{
    public class Text2ImgWithControlNet : StableDiffusionClientBase
    {
#if UNITY_EDITOR
        [SerializeField] private RenderPipelineAsset _pipelineAsset;
        [SerializeField] private MainRendererFeature _mainRendererFeature;
        [SerializeField] private Camera _camera;

        [Serializable]
        class ControlNetSettings : ControlNetUnitRequest.IDefault
        {
            [SerializeField][Range(0.0f, 2.0f)] private float _weight = 1.0f;
            [SerializeField] private ControlNetUnitRequest.ResizeMode _resizeMode = ControlNetUnitRequest.ResizeMode.JustResize;
            [SerializeField] private bool _lowVram = false;
            [SerializeField] private int _processorRes = 64;
            [SerializeField][Range(0.0f, 1.0f)] private float _guidanceStart = 0.0f;
            [SerializeField][Range(0.0f, 1.0f)] private float _guidanceEnd = 1.0f;
            [SerializeField] private ControlNetUnitRequest.ControlMode _controlMode = ControlNetUnitRequest.ControlMode.Balanced;
            [SerializeField] private bool _pixcelPerfect = false;

            public float Weight => _weight;
            public ControlNetUnitRequest.ResizeMode ResizeMode => _resizeMode;
            public bool LowVram => _lowVram;
            public int ProcessorRes => _processorRes;
            public float GuidanceStart => _guidanceStart;
            public float GuidanceEnd => _guidanceEnd;
            public ControlNetUnitRequest.ControlMode ControlMode => _controlMode;
            public bool PixcelPerfect => _pixcelPerfect;
        }
        [SerializeField] private ControlNetSettings _controlNetSettings = new();

        [HideInInspector][SerializeField] private int _selectedControlNetModelIndexIndex;

        public string SelectedControlNetModel => ControlNetModelList[SelectedControlNetModelIndex];
        public string[] ControlNetModelList => StableDiffusionWebUISettings.ControlNetModelNames;
        public int SelectedControlNetModelIndex
        {
            get { return _selectedControlNetModelIndexIndex; }
            set { _selectedControlNetModelIndexIndex = value; }
        }

        private bool _generating = false;

        private PipelineAssetLoader _assetLoader = new PipelineAssetLoader();

        public override void OnClickServerAccessButton()
        {
            if (_generating)
            {
                Debug.LogWarning("Generate already working.");
                return;
            }

            if (string.IsNullOrEmpty(Prompt))
            {
                Debug.LogWarning("Prompt is empty");
                return;
            }

            if (EditorApplication.isCompiling)
            {
                Debug.LogWarning("Compile is running");
                return;
            }

            Debug.Log("Image generating started.");

            GenerateAsync().Forget();
        }

        public async ValueTask GenerateAsync()
        {
            var texture = await CreatePreProcessedImageAsync();

            if (BatchCount == 1)
            {
                GenerateSingle(texture.EncodeToPNG()).Forget();
            }
            else if (BatchCount > 1)
            {
                GenerateLoop(texture.EncodeToPNG(), BatchCount).Forget();
            }
        }

        [ContextMenu("SavePreProcessedImage")]
        public async ValueTask SavePreProcessedImage()
        {
            var texture = await CreatePreProcessedImageAsync();

            ImagePorter.SavePngImage(texture.EncodeToPNG());
        }

        private async ValueTask<Texture2D> CreatePreProcessedImageAsync()
        {
            try
            {
                Texture2D texture;

                if (_assetLoader.SetPipeline(_pipelineAsset))
                {
                    EditorApplication.ExecuteMenuItem("Window/General/Game");

                    await Task.Delay(1000);

                    texture = RenderPreProcessedImage();
                }
                else
                {
                    EditorApplication.ExecuteMenuItem("Window/General/Game");

                    await Task.Delay(1000);

                    var pipeline = ((UniversalRenderPipelineAsset)GraphicsSettings.renderPipelineAsset);

                    FieldInfo propertyInfo = pipeline.GetType().GetField("m_RendererDataList", BindingFlags.Instance | BindingFlags.NonPublic);
                    var scriptableRendererData = ((ScriptableRendererData[])propertyInfo?.GetValue(pipeline))?[0];
                    var rendererFeature = ScriptableRendererFeature.CreateInstance<MainRendererFeature>();
                    scriptableRendererData.rendererFeatures.Add(rendererFeature);
                    scriptableRendererData.SetDirty();

                    texture = RenderPreProcessedImage();

                    scriptableRendererData.rendererFeatures.Remove(rendererFeature);
                }

                return texture;
            }
            finally
            {
                _assetLoader.ResetPipeline();
            }
        }

        private Texture2D RenderPreProcessedImage()
        {
            var render = new RenderTexture(Width, Height, 24, UnityEngine.Experimental.Rendering.GraphicsFormat.R32G32B32A32_SFloat); 
            render.antiAliasing = 8;

            var texture = new Texture2D(Width, Height, TextureFormat.RGB24, false);

            if (_camera == null) _camera = Camera.main;

            var bufferTT = _camera.targetTexture;
            var bufferRT = RenderTexture.active;

            try
            {
                _mainRendererFeature.SetRenderToTexture();

                _camera.targetTexture = render;
                _camera.Render();

                RenderTexture.active = render;
                texture.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
                texture.Apply();
    
                return texture;
            }
            finally
            {
                _camera.targetTexture = bufferTT;
                RenderTexture.active = bufferRT;

                RenderTexture.DestroyImmediate(render);

                _mainRendererFeature.SetRenderToScreen();
            }
        }

        private async ValueTask GenerateSingle(byte[] img)
        {
            try
            {
                _generating = true;

                await GenerateImage(img, true);
            }
            finally
            {
                _generating = false;
            }
        }

        private async ValueTask GenerateLoop(byte[] img, int count)
        {
            try
            {
                _generating = true;

                for (int i = 0; i < count; i++)
                {
                    await GenerateImage(img, false);
                }
            }
            finally
            {
                _generating = false;
            }
        }

        private async ValueTask GenerateImage(byte[] image, bool load = false)
        {
            await SetStableDiffusionModel();

            byte[] generated;

            using (var client = new StableDiffusionWebUIClient.Post.SdApi.V1.Extension.Txt2ImgWithControlNet(StableDiffusionWebUISettings))
            {
                var body = client.GetRequestBody(this);

                body.prompt = Prompt;
                body.negative_prompt = NegativePrompt;

                body.SetAlwaysonScripts(_controlNetSettings, SelectedControlNetModel, image);

                var responses = await client.SendRequestAsync(body);

                generated = responses.GetImage();
            }

            await LogSeedValue(generated);

            if (ImagePorter.SavePngImage(generated))
            {
                Debug.Log("Image generating completed.");
            }
            else
            {
                Debug.LogWarning("Faled to save generated image.");
            }

            if (load)
            {
                var texture = ImagePorter.GenerateTexture(generated);
                ImagePorter.LoadIntoImage(texture, this);
            }
        }
#endif
    }
}
