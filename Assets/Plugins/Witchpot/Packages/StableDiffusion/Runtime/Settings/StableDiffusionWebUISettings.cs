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
    public class StableDiffusionWebUISettings : ScriptableObject,
        StableDiffusionWebUIClient.Get.SdApi.V1.SdModels.IUrl,
        StableDiffusionWebUIClient.Get.SdApi.V1.Progress.IUrl,
        StableDiffusionWebUIClient.Get.SdApi.V1.CmdFlags.IUrl,
        StableDiffusionWebUIClient.Post.SdApi.V1.Options.IUrl,
        StableDiffusionWebUIClient.Post.SdApi.V1.PngInfo.IUrl,
        StableDiffusionWebUIClient.Post.SdApi.V1.Txt2Img.IUrl,
        StableDiffusionWebUIClient.Post.SdApi.V1.Txt2Img.RequestBody.IDefault,
        StableDiffusionWebUIClient.Post.SdApi.V1.Img2Img.IUrl,
        StableDiffusionWebUIClient.Post.SdApi.V1.Img2Img.RequestBody.IDefault,
        StableDiffusionWebUIClient.Post.ControlNet.Txt2Img.IUrl,
        StableDiffusionWebUIClient.Post.ControlNet.Txt2Img.RequestBody.IDefault
    {
        [SerializeField] private string _stableDiffusionWebUIServerURL = StableDiffusionWebUIClient.ServerUrl;
        [SerializeField] private string _modelsAPI = StableDiffusionWebUIClient.Get.SdApi.V1.SdModels.Paths;
        [SerializeField] private string _textToImageAPI = StableDiffusionWebUIClient.Post.SdApi.V1.Txt2Img.Paths;
        [SerializeField] private string _imageToImageAPI = StableDiffusionWebUIClient.Post.SdApi.V1.Img2Img.Paths;
        [SerializeField] private string _controlNetTextToImageAPI = StableDiffusionWebUIClient.Post.ControlNet.Txt2Img.Paths;
        [SerializeField] private string _optionAPI = StableDiffusionWebUIClient.Post.SdApi.V1.Options.Paths;
        [SerializeField] private string _pngInfoAPI = StableDiffusionWebUIClient.Post.SdApi.V1.PngInfo.Paths;
        [SerializeField] private string _progressAPI = StableDiffusionWebUIClient.Get.SdApi.V1.Progress.Paths;
        [SerializeField] private string _cmdFlagsAPI = StableDiffusionWebUIClient.Get.SdApi.V1.CmdFlags.Paths;
        [SerializeField] private string _outputFolder = "/streamingAssets";
        [SerializeField] private string _defaultSampler = "Euler a";
        [SerializeField] private int _defaultWidth = 512;
        [SerializeField] private int _defaultHeight = 512;
        [SerializeField] private int _defaultSteps = 40;
        [SerializeField] private float _defaultCfgScale = 7;
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

        public string StableDiffusionWebUIServerURL { get => _stableDiffusionWebUIServerURL; }

        public string ModelsAPI { get => _modelsAPI; }
        public string ModelsURL => $"{_stableDiffusionWebUIServerURL}{_modelsAPI}";
        string StableDiffusionWebUIClient.Get.SdApi.V1.SdModels.IUrl.Url => ModelsURL;

        public string TextToImageAPI { get => _textToImageAPI; }
        public string TextToImageURL => $"{_stableDiffusionWebUIServerURL}{_textToImageAPI}";
        string StableDiffusionWebUIClient.Post.SdApi.V1.Txt2Img.IUrl.Url => TextToImageURL;

        public string ImageToImageAPI { get => _imageToImageAPI; }
        public string ImageToImageURL => $"{_stableDiffusionWebUIServerURL}{_imageToImageAPI}";
        string StableDiffusionWebUIClient.Post.SdApi.V1.Img2Img.IUrl.Url => ImageToImageURL;

        public string ControlNetTextToImageAPI { get => _controlNetTextToImageAPI; }
        public string ControlNetTextToImageURL => $"{_stableDiffusionWebUIServerURL}{_controlNetTextToImageAPI}";
        string StableDiffusionWebUIClient.Post.ControlNet.Txt2Img.IUrl.Url => ControlNetTextToImageURL;

        public string OptionAPI { get => _optionAPI; }
        public string OptionURL => $"{_stableDiffusionWebUIServerURL}{_optionAPI}";
        string StableDiffusionWebUIClient.Post.SdApi.V1.Options.IUrl.Url => OptionURL;

        public string PngInfoAPI { get => _pngInfoAPI; }
        public string PngInfoURL => $"{_stableDiffusionWebUIServerURL}{_pngInfoAPI}";
        string StableDiffusionWebUIClient.Post.SdApi.V1.PngInfo.IUrl.Url => PngInfoURL;

        public string ProgressAPI { get => _progressAPI; }
        public string ProgressURL => $"{_stableDiffusionWebUIServerURL}{_progressAPI}";
        string StableDiffusionWebUIClient.Get.SdApi.V1.Progress.IUrl.Url => ProgressURL;

        public string CmdFlagsAPI { get => _cmdFlagsAPI; }
        public string CmdFlagsURL => $"{_stableDiffusionWebUIServerURL}{_cmdFlagsAPI}";
        string StableDiffusionWebUIClient.Get.SdApi.V1.CmdFlags.IUrl.Url => CmdFlagsURL;

        public string OutputFolder { get => _outputFolder; }

        public string Sampler { get => _defaultSampler; set => _defaultSampler = value; }
        public int Width { get => _defaultWidth; set => _defaultWidth = value; }
        public int Height { get => _defaultHeight; set => _defaultHeight = value; }
        public int Steps { get => _defaultSteps; set => _defaultSteps = value; }
        public float CfgScale { get => _defaultCfgScale; set => _defaultCfgScale = value; }
        public long Seed { get => _defaultSeed; set => _defaultSeed = value; }
        public string[] Samplers { get => _samplers; }
        public string[] ModelNames { get => _modelNames; }
        public string[] ModelNamesForLora { get => _modelNamesForLora; }

#if UNITY_EDITOR
        public async ValueTask ListModels()
        {
            using (var client = new StableDiffusionWebUIClient.Get.SdApi.V1.SdModels(this))
            {
                var responses = await client.SendRequestAsync();

                _modelNames = responses.Select(x => x.model_name).ToArray();
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
#endif
    }
}
