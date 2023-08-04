using System.Threading.Tasks;
using UnityEngine;

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
            var texture = await _controlType.ImageCapturer.CreatePreProcessedImageAsync(_camera, Width, Height);

            if (BatchCount == 1)
            {
                await GenerateSingle(texture.EncodeToPNG());
            }
            else if (BatchCount > 1)
            {
                await GenerateLoop(texture.EncodeToPNG(), BatchCount);
            }
        }

        public override void RefreshUnityEditor()
        {
            ImagePorter.RefreshUnityEditor();
        }

        [ContextMenu("SavePreProcessedImage")]
        public async ValueTask SavePreProcessedImage()
        {
            var texture = await _controlType.ImageCapturer.CreatePreProcessedImageAsync(_camera, Width, Height);

            ImagePorter.SavePngImage(texture.EncodeToPNG());
            AssetDatabase.Refresh();
        }

        private async ValueTask GenerateSingle(byte[] img)
        {
            try
            {
                _transmitting = true;

                await GenerateImage(img, true);
            }
            finally
            {
                _transmitting = false;
            }
        }

        private async ValueTask GenerateLoop(byte[] img, int count)
        {
            try
            {
                _transmitting = true;

                for (int i = 0; i < count; i++)
                {
                    await GenerateImage(img, false);
                }
            }
            finally
            {
                _transmitting = false;
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
