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
    public abstract class StableDiffusionClientBase : MonoBehaviour, IStableDiffusionClient,
        StableDiffusionWebUIClient.Post.SdApi.V1.Txt2Img.RequestBody.IDefault,
        StableDiffusionWebUIClient.Post.SdApi.V1.Img2Img.RequestBody.IDefault
    {
#if UNITY_EDITOR

        [SerializeField] private StableDiffusionWebUISettings _stableDiffusionWebUISettings;
        protected StableDiffusionWebUISettings StableDiffusionWebUISettings => _stableDiffusionWebUISettings;

        [SerializeField, TextArea] private string _prompt;
        public string Prompt
        {
            get { return _prompt; }
            set { _prompt = value; }
        }

        [SerializeField, TextArea] private string _negativePrompt;
        public string NegativePrompt
        {
            get { return _negativePrompt; }
            set { _negativePrompt = value; }
        }

        [SerializeField] private Vector2Int _size;
        public int Width => _size.x;
        public int Height => _size.y;

        [SerializeField] private int _steps;
        public int Steps => _steps;

        [SerializeField] private float _cfgScale;
        public float CfgScale => _cfgScale;

        [SerializeField] private long _seed;
        public long Seed => _seed;

        [SerializeField, Range(1, 100)] private int _batchCount = 1;
        protected int BatchCount => _batchCount;

        [HideInInspector][SerializeField] private int _selectedSamplerIndex;
        [HideInInspector][SerializeField] private int _selectedModelIndex;

        public string Sampler => SamplersList[SelectedSamplerIndex];
        public string[] SamplersList => StableDiffusionWebUISettings.Samplers;
        public int SelectedSamplerIndex
        {
            get { return _selectedSamplerIndex; }
            set { _selectedSamplerIndex = value; }
        }

        public string SelectedModel => ModelsList[SelectedModelIndex];
        public string[] ModelsList=> StableDiffusionWebUISettings.ModelNames;
        public int SelectedModelIndex
        {
            get { return _selectedModelIndex; }
            set { _selectedModelIndex = value; }
        }

        public string[] LoraModelsList => StableDiffusionWebUISettings.ModelNamesForLora;

        private void Reset()
        {
            ResetDefautValue();
        }

        private void ResetDefautValue()
        {
            if (_stableDiffusionWebUISettings == null) { return; }

            _size = new Vector2Int(_stableDiffusionWebUISettings.Width, _stableDiffusionWebUISettings.Height);
            _steps = _stableDiffusionWebUISettings.Steps;
            _cfgScale = _stableDiffusionWebUISettings.CfgScale;
            _seed = _stableDiffusionWebUISettings.Seed;
        }

        protected async ValueTask SetStableDiffusionModel()
        {
            using (var client = new StableDiffusionWebUIClient.Post.SdApi.V1.Options(StableDiffusionWebUISettings))
            {
                var body = client.GetRequestBody(SelectedModel);

                var responses = await client.SendRequestAsync(body);
            }
        }

        protected async ValueTask LogSeedValue(byte[] image)
        {
            using (var client = new StableDiffusionWebUIClient.Post.SdApi.V1.PngInfo(StableDiffusionWebUISettings))
            {
                var body = client.GetRequestBody(image);

                var responses = await client.SendRequestAsync(body);

                var dic = responses.Parse();

                Debug.Log($"Seed:{dic.GetValueOrDefault("Seed")}");
            }
        }

        public abstract void OnClickServerAccessButton();
#endif
    }
}
