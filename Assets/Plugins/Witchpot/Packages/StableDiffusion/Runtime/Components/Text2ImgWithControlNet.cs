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
        private enum ImageSource
        {
            Texture,
            Camera,
        }

        [SerializeField] private Camera _camera;
        [SerializeField] private ProccessedImageCapturer _imageCapturer;

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

        public override async ValueTask GenerateAsync()
        {
            var texture = await _imageCapturer.CreatePreProcessedImageAsync(_camera, Width, Height);

            if (BatchCount == 1)
            {
                await GenerateSingle(texture.EncodeToPNG());
            }
            else if (BatchCount > 1)
            {
                await GenerateLoop(texture.EncodeToPNG(), BatchCount);
            }

            AssetDatabase.Refresh();
        }

        [ContextMenu("SavePreProcessedImage")]
        public async ValueTask SavePreProcessedImage()
        {
            var texture = await _imageCapturer.CreatePreProcessedImageAsync(_camera, Width, Height);

            ImagePorter.SavePngImage(texture.EncodeToPNG());
            AssetDatabase.Refresh();
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
