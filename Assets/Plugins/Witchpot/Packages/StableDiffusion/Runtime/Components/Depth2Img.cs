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
        [SerializeField] private ImagePorter.ImageType _exportType = ImagePorter.ImageType.PNG;
        [HideInInspector][SerializeField] private int _selectedSampler;
        [HideInInspector][SerializeField] private int _selectedModel;
        [HideInInspector][SerializeField] private int _selectedLoraModel;
        private bool _generating = false;

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

            GenerateImage(texture.EncodeToPNG()).Forget();
        }

        private Texture2D CreateDepthImage()
        {
            var pipeline = ((UniversalRenderPipelineAsset)GraphicsSettings.renderPipelineAsset);

            FieldInfo propertyInfo = pipeline.GetType().GetField("m_RendererDataList", BindingFlags.Instance | BindingFlags.NonPublic);
            var scriptableRendererData = ((ScriptableRendererData[])propertyInfo?.GetValue(pipeline))?[0];
            var rendererFeature = ScriptableRendererFeature.CreateInstance<MainRendererFeature>();
            scriptableRendererData.rendererFeatures.Add(rendererFeature);
            scriptableRendererData.SetDirty();

            var volume = GameObject.FindFirstObjectByType<Volume>();
            Depth depth = null;
            volume.profile.TryGet<Depth>(out depth);

            if (depth is null)
            {
                depth = volume.profile.Add<Depth>();
            }

            depth.active = true;
            depth.weight.overrideState = true;
            depth.weight.value = 1f;
            var size = new Vector2Int(_width, _height);
            var render = new RenderTexture(size.x, size.y, 24, UnityEngine.Experimental.Rendering.GraphicsFormat.R32G32B32A32_SFloat);
            render.antiAliasing = 8;
            var texture = new Texture2D(size.x, size.y, TextureFormat.RGB24, false);
            if (_camera == null) _camera = Camera.main;

            try
            {
                _camera.targetTexture = render;
                _camera.Render();
                RenderTexture.active = render;
                texture.ReadPixels(new Rect(0, 0, size.x, size.y), 0, 0);
                texture.Apply();
            }
            finally
            {
                _camera.targetTexture = null;
                RenderTexture.active = null;
            }

            volume.profile.Remove<Depth>();
            scriptableRendererData.rendererFeatures.Remove(rendererFeature);

            return texture;
        }

        private async ValueTask GenerateImage(byte[] depth)
        {
            if (EditorApplication.isCompiling) return;

            try
            {
                _generating = true;

                using (var client = new StableDiffusionWebUIClient.Post.SdApi.V1.Options(_stableDiffusionWebUISettings))
                {
                    var body = client.GetRequestBody();

                    body.sd_model_checkpoint = ModelsList[_selectedModel];

                    var responses = await client.SendRequestAsync(body);
                }

                using (var client = new StableDiffusionWebUIClient.Post.ControlNet.Txt2Img(_stableDiffusionWebUISettings))
                {
                    var body = client.GetRequestBody(_stableDiffusionWebUISettings);

                    body.controlnet_weight = _weight;
                    body.prompt = _prompt;
                    body.steps = _steps;
                    body.negative_prompt = _negativePrompt;
                    body.seed = _seed;
                    body.cfg_scale = _cfgScale;
                    //body.denoising_strength = _denoisingStrength;
                    body.width = _width;
                    body.height = _height;

                    body.SetImage(depth);

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

                    if (ImagePorter.SaveImage(texture, _exportType))
                    {
                        Debug.Log("Image generating completed.");
                    }
                    else
                    {
                        Debug.LogWarning("Faled to save generated image.");
                    }

                    Image image = GetComponent<Image>();
                    if (image != null)
                    {
                        if (ImagePorter.LoadIntoImage(texture, image))
                        {
                            Debug.Log($"Image loaded in {image.name}.", image);
                        }
                        return;
                    }

                    RawImage rawImage = GetComponent<RawImage>();
                    if (rawImage != null)
                    {
                        if (ImagePorter.LoadIntoImage(texture, rawImage))
                        {
                            Debug.Log($"Image loaded in {rawImage.name}.", rawImage);
                        }
                        return;
                    }

                    Renderer renderer = GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        if (ImagePorter.LoadIntoImage(texture, renderer))
                        {
                            Debug.Log($"Image loaded in {renderer.name}.", renderer);
                            return;
                        }
                    }
                }
            }
            finally
            {
                _generating = false;
            }
        }
#endif
    }
}
