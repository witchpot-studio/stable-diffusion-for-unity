﻿using System;
using System.Threading.Tasks;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Witchpot.Runtime.StableDiffusion
{
    public class Img2ImgWithControlNet : StableDiffusionClientBase
    {
#if UNITY_EDITOR
        private enum ImageSource
        {
            Texture,
            Camera,
        }

        [SerializeField][Range(0.0f, 1.0f)] public float _denoisingStrength = 0.75f;

        [SerializeField] private ImageSource _imageSource = ImageSource.Texture;
        [SerializeField] private Camera _camera;
        [SerializeField] private Texture2D _image;

        [SerializeField] private ControlType _controlType;

        [SerializeField] private ControlNetSettings _controlNetSettings = new();

        public override void OnClickServerAccessButton()
        {
            if (_transmitting)
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

            GenerateAndRefresh().Forget();
        }

        public override async ValueTask GenerateAsync()
        {
            Texture2D textureImg;

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

                        textureImg = _image;
                    }
                    break;

                case ImageSource.Camera:
                    {
                        if (_camera == null)
                        {
                            Debug.LogError("Img2Img : Camera isn't set.", this);
                            return;
                        }

                        textureImg = ImageCapturer.CreateCameraViewImage(_camera, Width, Height);
                    }
                    break;

                default:
                    {
                        Debug.LogError("Img2Img : Unknown image source selected.", this);
                    }
                    return;
            }

            var textureControlNet = await _controlType.ImageCapturer.CreatePreProcessedImageAsync(_camera, Width, Height);

            if (BatchCount == 1)
            {
                await GenerateSingle(textureImg.EncodeToPNG(), textureControlNet.EncodeToPNG());
            }
            else if (BatchCount > 1)
            {
                await GenerateLoop(textureImg.EncodeToPNG(), textureControlNet.EncodeToPNG(), BatchCount);
            }
        }

        public override void RefreshUnityEditor()
        {
            ImagePorter.RefreshUnityEditor();
        }

        [ContextMenu("SaveCameraImage")]
        public void SaveCameraImage()
        {
            var texture = ImageCapturer.CreateCameraViewImage(_camera, Width, Height);

            ImagePorter.SavePngImage(texture.EncodeToPNG());
            AssetDatabase.Refresh();
        }

        [ContextMenu("SavePreProcessedImage")]
        public async ValueTask SavePreProcessedImage()
        {
            var texture = await _controlType.ImageCapturer.CreatePreProcessedImageAsync(_camera, Width, Height);

            ImagePorter.SavePngImage(texture.EncodeToPNG());
            AssetDatabase.Refresh();
        }

        private async ValueTask GenerateSingle(byte[] image, byte[] imageControlNet)
        {
            try
            {
                _transmitting = true;

                await GenerateImage(image, imageControlNet, true);
            }
            finally
            {
                _transmitting = false;
            }
        }

        private async ValueTask GenerateLoop(byte[] img, byte[] imgControlNet, int count)
        {
            try
            {
                _transmitting = true;

                for (int i = 0; i < count; i++)
                {
                    await GenerateImage(img, imgControlNet, false);
                }
            }
            finally
            {
                _transmitting = false;
            }
        }

        private async ValueTask GenerateImage(byte[] image, byte[] imageControlNet, bool load = false)
        {
            await SetStableDiffusionModel();

            byte[] generated;

            using (var client = new StableDiffusionWebUIClient.Post.SdApi.V1.Extension.Img2ImgWithControlNet(StableDiffusionWebUISettings))
            {
                var body = client.GetRequestBody(this);

                body.prompt = Prompt;
                body.negative_prompt = NegativePrompt;
                body.denoising_strength = _denoisingStrength;

                body.SetImage(image);

                body.SetAlwaysonScripts(_controlNetSettings, _controlType.SelectedControlNetModel, image);

                var responses = await client.SendRequestAsync(body);

                generated = responses.GetImage();

                var info = responses.GetInfo();
                Debug.Log($"Seed : {info.seed}");
            }

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
