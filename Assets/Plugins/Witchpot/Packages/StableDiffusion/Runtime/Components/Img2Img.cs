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
    public class Img2Img : StableDiffusionClientBase
    {
#if UNITY_EDITOR
        private enum ImageSource
        {
            Texture,
            Camera,
        }

        [SerializeField] private ImageSource _imageSource = ImageSource.Texture;
        [SerializeField] private Camera _camera;
        [SerializeField] private Texture2D _image;

        [SerializeField][Range(0.0f, 1.0f)] public float _denoisingStrength = 0.75f;

        private bool _generating = false;

        private void OnValidate()
        {
            if (_camera == null)
            {
                _camera = Camera.main;
            }
        }

        [ContextMenu("SaveCameraImage")]
        public void SaveCameraImage()
        {
            var texture = CreateCameraViewImage();

            ImagePorter.SavePngImage(texture.EncodeToPNG());
        }

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

            if (BatchCount == 1)
            {
                GenerateSingle(texture.EncodeToPNG()).Forget();
            }
            else if (BatchCount > 1)
            {
                GenerateLoop(texture.EncodeToPNG(),BatchCount).Forget();
            }
        }

        private Texture2D CreateCameraViewImage()
        {
            var view = Handles.GetMainGameViewSize();
            int width = (int)view.x;
            int hight = (int)view.y;

            var render = new RenderTexture(width, hight, 24);
            render.antiAliasing = 8;

            var texture = new Texture2D(width, hight, TextureFormat.RGB24, false);

            try
            {
                _camera.targetTexture = render;
                _camera.Render();
                RenderTexture.active = render;
                texture.ReadPixels(new Rect(0, 0, width, hight), 0, 0);
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

        private async ValueTask GenerateImage(byte[] img, bool load = false)
        {
            await SetStableDiffusionModel();

            byte[] generated;

            using (var client = new StableDiffusionWebUIClient.Post.SdApi.V1.Img2Img(StableDiffusionWebUISettings))
            {
                var body = client.GetRequestBody(this);

                body.prompt = Prompt;
                body.negative_prompt = NegativePrompt;
                body.denoising_strength = _denoisingStrength;

                body.SetImage(img);

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
