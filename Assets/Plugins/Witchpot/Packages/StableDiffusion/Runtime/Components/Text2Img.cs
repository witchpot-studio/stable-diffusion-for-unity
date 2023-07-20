﻿using System;
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
            if (BatchCount == 1)
            {
                await GenerateSingle();
            }
            else if (BatchCount > 1)
            {
                await GenerateLoop(BatchCount);
            }
        }

        public override void RefreshUnityEditor()
        {
            ImagePorter.RefreshUnityEditor();
        }

        private async ValueTask GenerateSingle()
        {
            try
            {
                _transmitting = true;

                await GenerateImage(true);
            }
            finally
            {
                _transmitting = false;
            }
        }

        private async ValueTask GenerateLoop(int count)
        {
            try
            {
                _transmitting = true;

                for (int i = 0; i< count; i++)
                {
                    await GenerateImage(false);
                }
            }
            finally
            {
                _transmitting = false;
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
