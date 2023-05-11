using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Witchpot.Runtime.StableDiffusion
{
    public class Img2Img : MonoBehaviour
    {
#if UNITY_EDITOR
        private enum ImageSource
        {
            Texture,
            Camera,
        }

        [SerializeField] private StableDiffusionWebUISettings _stableDiffusionWebUISettings;
        [SerializeField] private ImageSource _imageSource = ImageSource.Texture;
        [SerializeField] private Camera _camera;
        [SerializeField] private Texture2D _image;
        [SerializeField, TextArea] private string _prompt;
        [SerializeField, TextArea] private string _negativePrompt;
        [SerializeField] private int _width = 960;
        [SerializeField] private int _height = 540;
        [SerializeField] private int _steps = 50;
        [SerializeField] private float _cfgScale = 7;
        [SerializeField][Range(0.0f, 1.0f)] public float _denoisingStrength = 0.75f;
        [SerializeField] private long _seed = -1;
        [SerializeField, Range(1, 100)] private int _batchCount = 1;
        //[SerializeField] private ImagePorter.ImageType _exportType = ImagePorter.ImageType.PNG;
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

        private void OnValidate()
        {
            if (_camera == null)
            {
                _camera = Camera.main;
            }
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

            Texture2D texture;

            switch (_imageSource)
            {
                case ImageSource.Texture:
                    {
                        if (_image == null)
                        {
                            Debug.LogError("Img2Img : Input Image isn't set.", this);
                            return;
                        }

                        if (!_image.isReadable)
                        {
                            Debug.LogError($"Img2Img : Input Image {_image.name} isn't readable. Go to texture import settings and tick the Read/Write box", this);
                            return;
                        }

                        texture = _image;
                    }
                    break;

                case ImageSource.Camera:
                    {
                        if (_camera == null)
                        {
                            Debug.LogError("Img2Img : Camera isn't set.", this);
                            return;
                        }

                        texture = CreateCameraViewImage();
                    }
                    break;

                default:
                    {
                        Debug.LogError("Img2Img : Unknown image source selected.", this);
                    }
                    return;
            }

            if (_batchCount == 1)
            {
                GenerateSingle(texture.EncodeToPNG()).Forget();
            }
            else if (_batchCount > 1)
            {
                GenerateLoop(texture.EncodeToPNG(), _batchCount).Forget();
            }
        }

        private Texture2D CreateCameraViewImage()
        {
            var size = new Vector2Int((int)Handles.GetMainGameViewSize().x, (int)Handles.GetMainGameViewSize().y);
            var render = new RenderTexture(size.x, size.y, 24, UnityEngine.Experimental.Rendering.GraphicsFormat.R32G32B32A32_SFloat);
            render.antiAliasing = 8;
            var texture = new Texture2D(size.x, size.y, TextureFormat.RGB24, false);

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

            return texture;
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

        private async ValueTask GenerateImage(byte[] img, bool load = false)
        {
            using (var client = new StableDiffusionWebUIClient.Post.SdApi.V1.Options(_stableDiffusionWebUISettings))
            {
                var body = client.GetRequestBody();

                body.sd_model_checkpoint = ModelsList[_selectedModel];

                var responses = await client.SendRequestAsync(body);
            }

            using (var client = new StableDiffusionWebUIClient.Post.SdApi.V1.Img2Img(_stableDiffusionWebUISettings))
            {
                var body = client.GetRequestBody(_stableDiffusionWebUISettings);

                body.prompt = _prompt;
                body.steps = _steps;
                body.negative_prompt = _negativePrompt;
                body.seed = _seed;
                body.cfg_scale = _cfgScale;
                body.denoising_strength = _denoisingStrength;
                body.width = _width;
                body.height = _height;

                body.SetImage(img);

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
