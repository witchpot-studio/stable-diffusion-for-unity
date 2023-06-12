using System;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Witchpot.Runtime.StableDiffusion
{
    public class Depth2Img : MonoBehaviour
    {
#if UNITY_EDITOR
        [SerializeField] private StableDiffusionWebUISettings _stableDiffusionWebUISettings;
        [SerializeField] private RenderPipelineAsset _pipelineAsset;
        [SerializeField] private VolumeChanger _prefabVolumeChanger;
        [SerializeField] private Camera _camera;
        [SerializeField, TextArea] private string _prompt;
        [SerializeField, TextArea] private string _negativePrompt;
        [SerializeField] private int _width = 960;
        [SerializeField] private int _height = 540;
        [SerializeField] private int _steps = 50;
        [SerializeField] private float _cfgScale = 7;
        //[SerializeField][Range(0.0f, 1.0f)] public float _denoisingStrength = 0.0f;
        [SerializeField] private long _seed = -1;
        [SerializeField][Range(0.0f, 2.0f)] private float _weight = 1.0f;
        [SerializeField, Range(1, 100)] private int _batchCount = 1;
        //[SerializeField] private ImagePorter.ImageType _exportType = ImagePorter.ImageType.PNG;

        [Serializable]
        class ControlNetSettings
        {
            [SerializeField] [Range(0.0f, 2.0f)] public float _weight = 1.0f;
            [SerializeField] public StableDiffusionWebUIClient.ControlNetUnitRequest.ResizeMode _resizeMode = StableDiffusionWebUIClient.ControlNetUnitRequest.ResizeMode.JustResize;
            [SerializeField] public bool _lowVram = false;
            [SerializeField] public int _processorRes = 64;
            [SerializeField] [Range(0.0f, 1.0f)] public float _guidanceStart = 0.0f;
            [SerializeField] [Range(0.0f, 1.0f)] public float _guidanceEnd = 1.0f;
            [SerializeField] public StableDiffusionWebUIClient.ControlNetUnitRequest.ControlMode _controlMode = StableDiffusionWebUIClient.ControlNetUnitRequest.ControlMode.Balanced;
            [SerializeField] public bool _pixcelPerfect = false;
        }
        [SerializeField] private ControlNetSettings _controlNetSettings = new();

        [HideInInspector] [SerializeField] private int _selectedSampler;
        [HideInInspector] [SerializeField] private int _selectedModel;
        [HideInInspector] [SerializeField] private int _selectedLoraModel;

        private bool _generating = false;

        private PipelineAssetLoader _assetLoader = new PipelineAssetLoader();

        public string Prompt
        {
            get { return _prompt; }
            set { _prompt = value; }
        }

        public string[] SamplersList
        {
            get { return _stableDiffusionWebUISettings.Samplers; }
        }

        public string[] ModelsList
        {
            get { return _stableDiffusionWebUISettings.ModelNames; }
        }

        public string[] LoraModelsList
        {
            get { return _stableDiffusionWebUISettings.ModelNamesForLora; }
        }

        public int SelectedSampler
        {
            get { return _selectedSampler; }
            set { _selectedSampler = value; }
        }

        public int SelectedModel
        {
            get { return _selectedModel; }
            set { _selectedModel = value; }
        }

        public int SelectedLoraModel
        {
            get { return _selectedLoraModel; }
            set { _selectedLoraModel = value; }
        }

        public void OnClickGenerateButton()
        {
            if (_generating)
            {
                Debug.LogWarning("Generate already working.");
                return;
            }

            if (string.IsNullOrEmpty(_prompt))
            {
                Debug.LogWarning("Prompt is empty");
                return;
            }

            var texture = CreateDepthImage();

            if (_batchCount == 1)
            {
                GenerateSingle(texture.EncodeToPNG()).Forget();
            }
            else if (_batchCount > 1)
            {
                GenerateLoop(texture.EncodeToPNG(), _batchCount).Forget();
            }
        }

        [ContextMenu("SaveDepthImage")]
        public void SaveDepthImage()
        {
            var texture = CreateDepthImage();

            ImagePorter.SavePngImage(texture.EncodeToPNG());
        }

        private Texture2D CreateDepthImage()
        {
            try
            {
                Texture2D texture;

                if (_assetLoader.SetPipeline(_pipelineAsset))
                {
                    texture = RenderDepthImage();
                }
                else
                {
                    var pipeline = ((UniversalRenderPipelineAsset)GraphicsSettings.renderPipelineAsset);

                    FieldInfo propertyInfo = pipeline.GetType().GetField("m_RendererDataList", BindingFlags.Instance | BindingFlags.NonPublic);
                    var scriptableRendererData = ((ScriptableRendererData[])propertyInfo?.GetValue(pipeline))?[0];
                    var rendererFeature = ScriptableRendererFeature.CreateInstance<MainRendererFeature>();
                    scriptableRendererData.rendererFeatures.Add(rendererFeature);
                    scriptableRendererData.SetDirty();

                    texture = RenderDepthImage();

                    scriptableRendererData.rendererFeatures.Remove(rendererFeature);
                }

                return texture;
            }
            finally
            {
                _assetLoader.ResetPipeline();
            }
        }

        private Texture2D RenderDepthImage()
        {
            var changer = Instantiate(_prefabVolumeChanger);

            try
            {
                changer.SetVolumeStatus();

                var size = new Vector2Int(_width, _height);
                Texture2D texture = new Texture2D(size.x, size.y, TextureFormat.RGB24, false);

                var render = new RenderTexture(size.x, size.y, 24, UnityEngine.Experimental.Rendering.GraphicsFormat.R32G32B32A32_SFloat);
                render.antiAliasing = 8;
                texture = new Texture2D(size.x, size.y, TextureFormat.RGB24, false);
                if (_camera == null) _camera = Camera.main;

                _camera.targetTexture = render;
                _camera.Render();
                RenderTexture.active = render;
                texture.ReadPixels(new Rect(0, 0, size.x, size.y), 0, 0);
                texture.Apply();
    
                return texture;
            }
            finally
            {
                _camera.targetTexture = null;
                RenderTexture.active = null;

                changer.ResetVolumeStatus();

                DestroyImmediate(changer.gameObject);
            }
        }

        private async ValueTask GenerateSingle(byte[] img)
        {
            if (EditorApplication.isCompiling) return;

            Debug.Log("Image generating started.");

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
            if (EditorApplication.isCompiling) return;

            Debug.Log("Image generating started.");

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

        private async ValueTask GenerateImage(byte[] depth, bool load = false)
        {
            using (var client = new StableDiffusionWebUIClient.Post.SdApi.V1.Options(_stableDiffusionWebUISettings))
            {
                var body = client.GetRequestBody();

                body.sd_model_checkpoint = ModelsList[_selectedModel];

                var responses = await client.SendRequestAsync(body);
            }

            using (var client = new StableDiffusionWebUIClient.Post.SdApi.V1.Txt2Img(_stableDiffusionWebUISettings))
            {
                var body = client.GetRequestBody(_stableDiffusionWebUISettings);

                body.prompt = _prompt;
                body.steps = _steps;
                body.negative_prompt = _negativePrompt;
                body.seed = _seed;
                body.cfg_scale = _cfgScale;
                //body.denoising_strength = _denoisingStrength;
                body.width = _width;
                body.height = _height;
                body.sampler_index = SamplersList[SelectedSampler];

                var controlnet_unit = new StableDiffusionWebUIClient.ControlNetUnitRequest()
                {
                    model = "control_v11f1p_sd15_depth_fp16 [4b72d323]",
                    weight = _controlNetSettings._weight,
                    resize_mode = _controlNetSettings._resizeMode,
                    lowvram = _controlNetSettings._lowVram,
                    processor_res = _controlNetSettings._processorRes,
                    guidance_start = _controlNetSettings._guidanceStart,
                    guidance_end = _controlNetSettings._guidanceEnd,
                    control_mode = _controlNetSettings._controlMode,
                    pixel_perfect = _controlNetSettings._pixcelPerfect,
                };
                controlnet_unit.SetImage(depth);

                body.alwayson_scripts = new StableDiffusionWebUIClient.AlwaysonScripts();
                body.alwayson_scripts.controlnet = new StableDiffusionWebUIClient.ControlNet();
                body.alwayson_scripts.controlnet.args = new StableDiffusionWebUIClient.ControlNetUnitRequest[] { controlnet_unit };

                var responses = await client.SendRequestAsync(body);

                using (var clientInfo = new StableDiffusionWebUIClient.Post.SdApi.V1.PngInfo(_stableDiffusionWebUISettings))
                {
                    var bodyInfo = clientInfo.GetRequestBody();
                    bodyInfo.SetImage(responses.GetImage());

                    var responsesInfo = await clientInfo.SendRequestAsync(bodyInfo);

                    var dic = responsesInfo.Parse();

                    Debug.Log($"Seed:{dic.GetValueOrDefault("Seed")}");
                }

                var texture = ImagePorter.GenerateTexture(responses.GetImage());

                if (ImagePorter.SavePngImage(responses.GetImage()))
                //if (ImagePorter.SaveImage(texture, _exportType))
                {
                    Debug.Log("Image generating completed.");
                }
                else
                {
                    Debug.LogWarning("Faled to save generated image.");
                }

                if (load)
                {
                    ImagePorter.LoadIntoImage(texture, this);
                }
            }
        }
#endif
    }
}
