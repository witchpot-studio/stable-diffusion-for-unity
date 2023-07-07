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
    public class Text2Img : StableDiffusionClientBase
    {
#if UNITY_EDITOR

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

            if (BatchCount == 1)
            {
                GenerateSingle().Forget();
            }
            else if (BatchCount > 1)
            {
                GenerateLoop(BatchCount).Forget();
            }
        }

        private async ValueTask GenerateSingle()
        {
            try
            {
                _generating = true;

                await GenerateImage(true);
            }
            finally
            {
                _generating = false;
            }
        }

        private async ValueTask GenerateLoop(int count)
        {
            try
            {
                _generating = true;

                for (int i = 0; i< count; i++)
                {
                    await GenerateImage(false);
                }
            }
            finally
            {
                _generating = false;
            }
        }

        private async ValueTask GenerateImage(bool load = false)
        {
            await SetStableDiffusionModel();

            byte[] generated;

            using (var client = new StableDiffusionWebUIClient.Post.SdApi.V1.Txt2Img(StableDiffusionWebUISettings))
            {
                var body = client.GetRequestBody(this);

                body.prompt = Prompt;
                body.negative_prompt = NegativePrompt;

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
