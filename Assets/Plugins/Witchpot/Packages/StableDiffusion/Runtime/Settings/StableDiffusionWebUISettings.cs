using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Witchpot.Runtime.StableDiffusion;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Witchpot
{
    public class StableDiffusionWebUISettings : ScriptableObject, IStableDiffusionClient,
        StableDiffusionWebUIClient.Get.SdApi.V1.SdModels.IUrl,
        StableDiffusionWebUIClient.Get.SdApi.V1.Progress.IUrl,
        StableDiffusionWebUIClient.Get.SdApi.V1.CmdFlags.IUrl,
        StableDiffusionWebUIClient.Post.SdApi.V1.Options.IUrl,
        StableDiffusionWebUIClient.Post.SdApi.V1.PngInfo.IUrl,
        StableDiffusionWebUIClient.Post.SdApi.V1.Txt2Img.IUrl,
        StableDiffusionWebUIClient.Post.SdApi.V1.Txt2Img.RequestBody.IDefault,
        StableDiffusionWebUIClient.Post.SdApi.V1.Img2Img.IUrl,
        StableDiffusionWebUIClient.Post.SdApi.V1.Img2Img.RequestBody.IDefault,
        StableDiffusionWebUIClient.Get.ControlNet.Version.IUrl,
        StableDiffusionWebUIClient.Get.ControlNet.ModelList.IUrl,
        StableDiffusionWebUIClient.Get.ControlNet.ModuleList.IUrl
        //StableDiffusionWebUIClient.Post.ControlNet.Txt2Img.IUrl,
        //StableDiffusionWebUIClient.Post.ControlNet.Txt2Img.RequestBody.IDefault
    {
        [SerializeField] private string _stableDiffusionWebUIServerURL = StableDiffusionWebUIClient.ServerUrl;
        [SerializeField] private string _modelsAPI = StableDiffusionWebUIClient.Get.SdApi.V1.SdModels.Paths;
        [SerializeField] private string _textToImageAPI = StableDiffusionWebUIClient.Post.SdApi.V1.Txt2Img.Paths;
        [SerializeField] private string _imageToImageAPI = StableDiffusionWebUIClient.Post.SdApi.V1.Img2Img.Paths;
        [SerializeField] private string _optionAPI = StableDiffusionWebUIClient.Post.SdApi.V1.Options.Paths;
        [SerializeField] private string _pngInfoAPI = StableDiffusionWebUIClient.Post.SdApi.V1.PngInfo.Paths;
        [SerializeField] private string _progressAPI = StableDiffusionWebUIClient.Get.SdApi.V1.Progress.Paths;
        [SerializeField] private string _cmdFlagsAPI = StableDiffusionWebUIClient.Get.SdApi.V1.CmdFlags.Paths;
        [SerializeField] private string _controlNetVersionAPI = StableDiffusionWebUIClient.Get.ControlNet.Version.Paths;
        [SerializeField] private string _controlNetModelListAPI = StableDiffusionWebUIClient.Get.ControlNet.ModelList.Paths;
        [SerializeField] private string _controlNetModuleListAPI = StableDiffusionWebUIClient.Get.ControlNet.ModuleList.Paths;
        //[SerializeField] private string _controlNetTextToImageAPI = StableDiffusionWebUIClient.Post.ControlNet.Txt2Img.Paths;
        [SerializeField] private string _outputFolder = "/StreamingAssets";
        [SerializeField] private int _defaultSamplerIndex = 0;
        [SerializeField] private int _defaultWidth = 512;
        [SerializeField] private int _defaultHeight = 512;
        [SerializeField] private int _defaultSteps = 40;
        [SerializeField] private double _defaultCfgScale = 7;
        [SerializeField] private long _defaultSeed = -1;
        [SerializeField] private string[] _samplers = new string[]
        {
            "Euler a",
            "Euler",
            "LMS",
            "Heun",
            "DPM2",
            "DPM2 a",
            "DPM++ 2S a",
            "DPM++ 2M",
            "DPM++ SDE",
            "DPM fast",
            "DPM adaptive",
            "LMS Karras",
            "DPM2 Karras",
            "DPM2 a Karras",
            "DPM++ 2S a Karras",
            "DPM++ 2M Karras",
            "DPM++ SDE Karras",
            "DDIM",
            "PLMS"
        };
        [SerializeField] private string[] _modelNames;
        [SerializeField] private string[] _modelNamesForLora;
        [SerializeField] private string[] _controlNetModelNames;
        [SerializeField] private string[] _controlNetModuleNames;

        public string StableDiffusionWebUIServerURL { get => _stableDiffusionWebUIServerURL; }

        public string ModelsAPI => _modelsAPI;
        public string ModelsURL => $"{_stableDiffusionWebUIServerURL}{_modelsAPI}";
        string StableDiffusionWebUIClient.Get.SdApi.V1.SdModels.IUrl.Url => ModelsURL;

        public string TextToImageAPI => _textToImageAPI;
        public string TextToImageURL => $"{_stableDiffusionWebUIServerURL}{_textToImageAPI}";
        string StableDiffusionWebUIClient.Post.SdApi.V1.Txt2Img.IUrl.Url => TextToImageURL;

        public string ImageToImageAPI => _imageToImageAPI;
        public string ImageToImageURL => $"{_stableDiffusionWebUIServerURL}{_imageToImageAPI}";
        string StableDiffusionWebUIClient.Post.SdApi.V1.Img2Img.IUrl.Url => ImageToImageURL;

        public string OptionAPI => _optionAPI;
        public string OptionURL => $"{_stableDiffusionWebUIServerURL}{_optionAPI}";
        string StableDiffusionWebUIClient.Post.SdApi.V1.Options.IUrl.Url => OptionURL;

        public string PngInfoAPI => _pngInfoAPI;
        public string PngInfoURL => $"{_stableDiffusionWebUIServerURL}{_pngInfoAPI}";
        string StableDiffusionWebUIClient.Post.SdApi.V1.PngInfo.IUrl.Url => PngInfoURL;

        public string ProgressAPI => _progressAPI;
        public string ProgressURL => $"{_stableDiffusionWebUIServerURL}{_progressAPI}";
        string StableDiffusionWebUIClient.Get.SdApi.V1.Progress.IUrl.Url => ProgressURL;

        public string CmdFlagsAPI => _cmdFlagsAPI;
        public string CmdFlagsURL => $"{_stableDiffusionWebUIServerURL}{_cmdFlagsAPI}";
        string StableDiffusionWebUIClient.Get.SdApi.V1.CmdFlags.IUrl.Url => CmdFlagsURL;

        public string ControlNetVersionAPI => _controlNetVersionAPI;
        public string ControlNetVersionURL => $"{_stableDiffusionWebUIServerURL}{_controlNetVersionAPI}";
        string StableDiffusionWebUIClient.Get.ControlNet.Version.IUrl.Url => ControlNetVersionURL;

        public string ControlNetModelListAPI => _controlNetModelListAPI;
        public string ControlNetModelListURL => $"{_stableDiffusionWebUIServerURL}{_controlNetModelListAPI}";
        string StableDiffusionWebUIClient.Get.ControlNet.ModelList.IUrl.Url => ControlNetModelListURL;

        public string ControlNetModuleListAPI => _controlNetModuleListAPI;
        public string ControlNetModuleListURL => $"{_stableDiffusionWebUIServerURL}{_controlNetModuleListAPI}";
        string StableDiffusionWebUIClient.Get.ControlNet.ModuleList.IUrl.Url => ControlNetModuleListURL;

        //public string ControlNetTextToImageAPI { get => _controlNetTextToImageAPI; }
        //public string ControlNetTextToImageURL => $"{_stableDiffusionWebUIServerURL}{_controlNetTextToImageAPI}";
        //string StableDiffusionWebUIClient.Post.ControlNet.Txt2Img.IUrl.Url => ControlNetTextToImageURL;

        public string OutputFolder { get => _outputFolder; }

        public string Sampler => Samplers[_defaultSamplerIndex];
        public int Width => _defaultWidth;
        public int Height => _defaultHeight;
        public int Steps => _defaultSteps;
        public double CfgScale => _defaultCfgScale;
        public long Seed => _defaultSeed;

        public string[] Samplers => _samplers;
        public string[] ModelNames => _modelNames;
        public string[] ModelNamesForLora => _modelNamesForLora;
        public string[] ControlNetModelNames => _controlNetModelNames;
        public string[] ControlNetModuleNames => _controlNetModuleNames;

        protected bool _transmitting = false;
        public bool IsTransmitting => _transmitting;

#if UNITY_EDITOR
        public void OnClickServerAccessButton()
        {
            ListModels().Forget();
        }

        public async ValueTask ListModels()
        {
            _transmitting = true;

            try
            {
                using (var client = new StableDiffusionWebUIClient.Get.SdApi.V1.SdModels(this))
                {
                    var responses = await client.SendRequestAsync();

                    _modelNames = responses.Select(x => x.model_name).ToArray();
                }

                using (var client = new StableDiffusionWebUIClient.Get.ControlNet.ModelList(this))
                {
                    var responses = await client.SendRequestAsync();

                    _controlNetModelNames = responses.model_list;
                }

                using (var client = new StableDiffusionWebUIClient.Get.ControlNet.ModuleList(this))
                {
                    var responses = await client.SendRequestAsync();

                    _controlNetModuleNames = responses.module_list;
                }

                using (var client = new StableDiffusionWebUIClient.Get.SdApi.V1.CmdFlags(this))
                {
                    var responses = await client.SendRequestAsync();

                    _modelNamesForLora = Directory.GetFiles(responses.lora_dir, "*", SearchOption.AllDirectories)
                        .Select(x => Path.GetFileNameWithoutExtension(x)).ToArray();
                }

                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();
            }
            finally
            {
                _transmitting = false;
            }
        }
#endif
    }
}
