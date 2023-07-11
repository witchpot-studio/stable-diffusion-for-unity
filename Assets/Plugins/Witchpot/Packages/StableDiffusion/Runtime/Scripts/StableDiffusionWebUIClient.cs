using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;


namespace Witchpot.Runtime.StableDiffusion
{
    public interface IStableDiffusionClient
    {
        public void OnClickServerAccessButton();
    }

    public class WebRequestException : Exception
    {
        public WebRequestException(string str) : base(str) { }
    }

    public static class StableDiffusionWebUIClient
    {
        // Static
        public static readonly string ServerUrl = "http://127.0.0.1:7860";

        private static readonly string ContentType = "Content-Type";
        private static readonly string ApplicationJson = "application/json";

        private static readonly string LabelPrompt = "Prompt";
        private static readonly string LabelNegativePrompt = "Negative Prompt";

        public static string GetLoraString(string lora, double strength = 1.0f)
        {
            return $"<lora:{lora}:{strength}>";
        }

        public static string GetImageString(byte[] data)
        {
            return Convert.ToBase64String(data);
        }

        public static string[] GetImageStringArray(byte[] data)
        {
            return new string[] { GetImageString(data) };
        }

        public static byte[] GetImageByteArray(string[] images)
        {
            return Convert.FromBase64String(images[0].Split(",")[0]);
        }

        //
        public interface IParameters { }
        public interface IRequestBody { }
        public interface IResponses { }

        public struct RequestHeader
        {
            private string _name;
            public string Name => _name;

            private string _value;
            public string Value => _value;

            public RequestHeader(string name, string value)
            {
                _name = name;
                _value = value;
            }
        }

        public abstract class WebRequestWrapper : IDisposable
        {
            protected static UploadHandlerRaw GetUploadHandler(IRequestBody body)
            {
                var json = JsonUtility.ToJson(body);
                var bytes = Encoding.UTF8.GetBytes(json);

                return new UploadHandlerRaw(bytes);
            }

            protected static TResponses GetResponce<TResponses>(string result) where TResponses : IResponses
            {
                return JsonUtility.FromJson<TResponses>(result);
            }

            protected static IReadOnlyList<TResponses> GetListResponce<TResponses>(string result) where TResponses : IResponses
            {
                return JsonConvert.DeserializeObject<TResponses[]>(result);
            }

            // field
            private UnityWebRequest _request;

            // property
            protected UnityWebRequest WebRequest => _request;
            public bool isDone => _request.isDone;
            public UnityWebRequest.Result Result => _request.result;

            protected WebRequestWrapper(string uri, string method, IReadOnlyList<RequestHeader> list)
            {
                _request = new UnityWebRequest(uri, method);

                foreach (var header in list)
                {
                    _request.SetRequestHeader(header.Name, header.Value);
                }
            }

            protected void CheckDone()
            {
                if (isDone)
                {
                    throw new WebRequestException("WebRequest already done.");
                }
            }

            protected TResult ParseResult<TResult>(Func<string, TResult> parser)
            {
                if (WebRequest.result == UnityWebRequest.Result.Success)
                {
                    if (string.IsNullOrEmpty(WebRequest.downloadHandler.text) || WebRequest.downloadHandler.text == "null")
                    {
                        return default;
                    }
                    else
                    {
                        return parser(WebRequest.downloadHandler.text);
                    }
                }
                else
                {
                    throw new WebRequestException($"{WebRequest.error}");
                }
            }

            protected async ValueTask<TResponses> SendRequestAsync<TResponses>()
                where TResponses : IResponses
            {
                CheckDone();

                WebRequest.downloadHandler = new DownloadHandlerBuffer();

                await WebRequest.SendWebRequest();

                return ParseResult(GetResponce<TResponses>);
            }

            protected async ValueTask<IReadOnlyList<TResponses>> SendRequestAsListResponsesAsync<TResponses>()
                where TResponses : IResponses
            {
                CheckDone();

                WebRequest.downloadHandler = new DownloadHandlerBuffer();

                await WebRequest.SendWebRequest();

                return ParseResult(GetListResponce<TResponses>);
            }

            protected async ValueTask<TResponses> SendRequestAsync<TRequestBody, TResponses>(TRequestBody body)
                where TRequestBody : IRequestBody
                where TResponses : IResponses
            {
                CheckDone();

                WebRequest.uploadHandler = GetUploadHandler(body);
                WebRequest.downloadHandler = new DownloadHandlerBuffer();

                await WebRequest.SendWebRequest();

                return ParseResult(GetResponce<TResponses>);
            }

            protected async ValueTask<IReadOnlyList<TResponses>> SendRequestAsListResponsesAsync<TRequestBody, TResponses>(TRequestBody body)
                where TRequestBody : IRequestBody
                where TResponses : IResponses
            {
                CheckDone();

                WebRequest.uploadHandler = GetUploadHandler(body);
                WebRequest.downloadHandler = new DownloadHandlerBuffer();

                await WebRequest.SendWebRequest();

                return ParseResult(GetListResponce<TResponses>);
            }

            public void Dispose()
            {
                ((IDisposable)WebRequest).Dispose();
            }
        }

        public static class Get
        {
            public static string Method => UnityWebRequest.kHttpVerbGET;

            public class AppId : WebRequestWrapper
            {
                // static
                public static string Paths => "/app_id";
                public static string Url => $"{ServerUrl}{Paths}";
                public static IReadOnlyList<RequestHeader> RequestHeaderList { get; }

                static AppId()
                {
                    RequestHeaderList = new List<RequestHeader>() { new RequestHeader(ContentType, ApplicationJson), };
                }

                // interface
                public interface IUrl
                {
                    public string Url { get; }
                }

                // internal class
                [Serializable]
                public class Responses : IResponses
                {
                    public string app_id;
                }

                // method
                public AppId() : base(Url, Method, RequestHeaderList) { }
                public AppId(IUrl url) : base(url.Url, Method, RequestHeaderList) { }

                public ValueTask<Responses> SendRequestAsync()
                {
                    return base.SendRequestAsync<Responses>();
                }
            }

            public class Info : WebRequestWrapper
            {
                // static
                public static string Paths => "/info";
                public static string Url => $"{ServerUrl}{Paths}";
                public static IReadOnlyList<RequestHeader> RequestHeaderList { get; }

                static Info()
                {
                    RequestHeaderList = new List<RequestHeader>() { new RequestHeader(ContentType, ApplicationJson), };
                }

                // interface
                public interface IUrl
                {
                    public string Url { get; }
                }

                // internal class
                [Serializable]
                public class Responses : IResponses
                {
                    public string[] named_endpoints;
                    public string[] unnamed_endpoints;
                }

                // method
                public Info() : base(Url, Method, RequestHeaderList) { }
                public Info(IUrl url) : base(url.Url, Method, RequestHeaderList) { }

                public ValueTask<Responses> SendRequestAsync()
                {
                    return base.SendRequestAsync<Responses>();
                }
            }

            public class Config : WebRequestWrapper
            {
                // static
                public static string Paths => "/config";
                public static string Url => $"{ServerUrl}{Paths}";
                public static IReadOnlyList<RequestHeader> RequestHeaderList { get; }

                static Config()
                {
                    RequestHeaderList = new List<RequestHeader>() { new RequestHeader(ContentType, ApplicationJson), };
                }

                // interface
                public interface IUrl
                {
                    public string Url { get; }
                }

                // internal class
                [Serializable]
                public class Responses : IResponses
                {
                    public string version;

                    public string GetVersion()
                    {
                        return version.Trim('\n');
                    }
                }

                // method
                public Config() : base(Url, Method, RequestHeaderList) { }
                public Config(IUrl url) : base(url.Url, Method, RequestHeaderList) { }

                public ValueTask<Responses> SendRequestAsync()
                {
                    return base.SendRequestAsync<Responses>();
                }
            }

            public static class SdApi
            {
                public static class V1
                {
                    public class CmdFlags : WebRequestWrapper
                    {
                        // static
                        public static string Paths => "/sdapi/v1/cmd-flags";
                        public static string Url => $"{ServerUrl}{Paths}";
                        public static IReadOnlyList<RequestHeader> RequestHeaderList { get; }

                        static CmdFlags()
                        {
                            RequestHeaderList = new List<RequestHeader>() { new RequestHeader(ContentType, ApplicationJson), };
                        }

                        // interface
                        public interface IUrl
                        {
                            public string Url { get; }
                        }

                        // internal class
                        [Serializable]
                        public class Responses : IResponses
                        {
                            public bool f;
                            public bool update_all_extensions;
                            public bool skip_python_version_check;
                            public bool skip_torch_cuda_test;
                            public bool reinstall_xformers;
                            public bool reinstall_torch;
                            public bool update_check;
                            public string tests;
                            public bool no_tests;
                            public bool skip_install;
                            public string data_dir;
                            public string config;
                            public string ckpt;
                            public string ckpt_dir;
                            public string vae_dir;
                            public string gfpgan_dir;
                            public string gfpgan_model;
                            public bool no_half;
                            public bool no_half_vae;
                            public bool no_progressbar_hiding;
                            public int max_batch_count;
                            public string embeddings_dir;
                            public string textual_inversion_templates_dir;
                            public string hypernetwork_dir;
                            public string localizations_dir;
                            public bool allow_code;
                            public bool medvram;
                            public bool lowvram;
                            public bool lowram;
                            public bool always_batch_cond_uncond;
                            public bool unload_gfpgan;
                            public string precision;
                            public bool upcast_sampling;
                            public bool share;
                            public string ngrok;
                            public string ngrok_region;
                            public bool enable_insecure_extension_access;
                            public string codeformer_models_path;
                            public string gfpgan_models_path;
                            public string esrgan_models_path;
                            public string bsrgan_models_path;
                            public string realesrgan_models_path;
                            public string clip_models_path;
                            public bool xformers;
                            public bool force_enable_xformers;
                            public bool xformers_flash_attention;
                            public bool deepdanbooru;
                            public bool opt_split_attention;
                            public bool opt_sub_quad_attention;
                            public int sub_quad_q_chunk_size;
                            public string sub_quad_kv_chunk_size;
                            public string sub_quad_chunk_threshold;
                            public bool opt_split_attention_invokeai;
                            public bool opt_split_attention_v1;
                            public bool opt_sdp_attention;
                            public bool opt_sdp_no_mem_attention;
                            public bool disable_opt_split_attention;
                            public bool disable_nan_check;
                            // public string[] use_cpu;
                            public bool listen;
                            public string port;
                            public bool show_negative_prompt;
                            public string ui_config_file;
                            public bool hide_ui_dir_config;
                            public bool freeze_settings;
                            public string ui_settings_file;
                            public bool gradio_debug;
                            public string gradio_auth;
                            public string gradio_auth_path;
                            public string gradio_img2img_tool;
                            public string gradio_inpaint_tool;
                            public bool opt_channelslast;
                            public string styles_file;
                            public bool autolaunch;
                            public string theme;
                            public bool use_textbox_seed;
                            public bool disable_console_progressbars;
                            public bool enable_console_prompts;
                            public bool vae_path;
                            public bool disable_safe_unpickle;
                            public bool api;
                            public string api_auth;
                            public bool api_log;
                            public bool nowebui;
                            public bool ui_debug_mode;
                            public string device_id;
                            public bool administrator;
                            public string cors_allow_origins;
                            public string cors_allow_origins_regex;
                            public string tls_keyfile;
                            public string tls_certfile;
                            public string server_name;
                            public bool gradio_queue;
                            public bool no_gradio_queue;
                            public bool skip_version_check;
                            public bool no_hashing;
                            public bool no_download_sd_model;
                            public string controlnet_dir;
                            public string controlnet_annotator_models_path;
                            public string no_half_controlnet;
                            public string ldsr_models_path;
                            public string lora_dir;
                            public string scunet_models_path;
                            public string swinir_models_path;
                        }

                        // method
                        public CmdFlags() : base(Url, Method, RequestHeaderList) { }
                        public CmdFlags(IUrl url) : base(url.Url, Method, RequestHeaderList) { }

                        public ValueTask<Responses> SendRequestAsync()
                        {
                            return base.SendRequestAsync<Responses>();
                        }
                    }

                    public class SdModels : WebRequestWrapper
                    {
                        // static
                        public static string Paths => "/sdapi/v1/sd-models";
                        public static string Url => $"{ServerUrl}{Paths}";
                        public static IReadOnlyList<RequestHeader> RequestHeaderList { get; }

                        static SdModels()
                        {
                            RequestHeaderList = new List<RequestHeader>() { new RequestHeader(ContentType, ApplicationJson), };
                        }

                        // interface
                        public interface IUrl
                        {
                            public string Url { get; }
                        }

                        // internal class
                        [Serializable]
                        public class Responses : IResponses
                        {
                            public string title;
                            public string model_name;
                            public string hash;
                            public string sha256;
                            public string filename;
                            public string config;
                        }

                        // method
                        public SdModels() : base(Url, Method, RequestHeaderList) { }
                        public SdModels(IUrl url) : base(url.Url, Method, RequestHeaderList) { }

                        public ValueTask<IReadOnlyList<Responses>> SendRequestAsync()
                        {
                            return base.SendRequestAsListResponsesAsync<Responses>();
                        }
                    }

                    public class Progress : WebRequestWrapper
                    {
                        // static
                        public static string Paths => "/sdapi/v1/progress";
                        public static string Url => $"{ServerUrl}{Paths}";
                        public static IReadOnlyList<RequestHeader> RequestHeaderList { get; }

                        static Progress()
                        {
                            RequestHeaderList = new List<RequestHeader>() { new RequestHeader(ContentType, ApplicationJson), };
                        }

                        // interface
                        public interface IUrl
                        {
                            public string Url { get; }
                        }

                        // internal class
                        [Serializable]
                        public class Parameters : IParameters
                        {
                            public bool skip_current_image;
                        }

                        [Serializable]
                        public class Responses : IResponses
                        {
                            public double progress;
                            public double eta_relative;
                            //public XXX state = { };
                            public string current_image;
                            public string textinfo;
                        }

                        // method
                        public Progress() : base(Url, Method, RequestHeaderList) { }
                        public Progress(IUrl url) : base(url.Url, Method, RequestHeaderList) { }

                        public ValueTask<Responses> SendRequestAsync()
                        {
                            return base.SendRequestAsync<Responses>();
                        }
                    }
                }
            }

            public static class ControlNet
            {
                public class Version : WebRequestWrapper
                {
                    // static
                    public static string Paths => "/controlnet/version";
                    public static string Url => $"{ServerUrl}{Paths}";
                    public static IReadOnlyList<RequestHeader> RequestHeaderList { get; }

                    static Version()
                    {
                        RequestHeaderList = new List<RequestHeader>() { new RequestHeader(ContentType, ApplicationJson), };
                    }

                    // interface
                    public interface IUrl
                    {
                        public string Url { get; }
                    }

                    [Serializable]
                    public class Responses : IResponses
                    {
                        public int version;
                    }

                    // method
                    public Version() : base(Url, Method, RequestHeaderList) { }
                    public Version(IUrl url) : base(url.Url, Method, RequestHeaderList) { }

                    public ValueTask<Responses> SendRequestAsync()
                    {
                        return base.SendRequestAsync<Responses>();
                    }
                }

                public class ModelList : WebRequestWrapper
                {
                    // static
                    public static string Paths => "/controlnet/model_list";
                    public static string Url => $"{ServerUrl}{Paths}";
                    public static IReadOnlyList<RequestHeader> RequestHeaderList { get; }

                    static ModelList()
                    {
                        RequestHeaderList = new List<RequestHeader>() { new RequestHeader(ContentType, ApplicationJson), };
                    }

                    // interface
                    public interface IUrl
                    {
                        public string Url { get; }
                    }

                    [Serializable]
                    public class Responses : IResponses
                    {
                        public string[] model_list;
                    }

                    // method
                    public ModelList() : base(Url, Method, RequestHeaderList) { }
                    public ModelList(IUrl url) : base(url.Url, Method, RequestHeaderList) { }

                    public ValueTask<Responses> SendRequestAsync()
                    {
                        return base.SendRequestAsync<Responses>();
                    }
                }

                public class ModuleList : WebRequestWrapper
                {
                    // static
                    public static string Paths => "/controlnet/module_list";
                    public static string Url => $"{ServerUrl}{Paths}";
                    public static IReadOnlyList<RequestHeader> RequestHeaderList { get; }

                    static ModuleList()
                    {
                        RequestHeaderList = new List<RequestHeader>() { new RequestHeader(ContentType, ApplicationJson), };
                    }

                    // interface
                    public interface IUrl
                    {
                        public string Url { get; }
                    }

                    [Serializable]
                    public class Responses : IResponses
                    {
                        public string[] module_list;
                    }

                    // method
                    public ModuleList() : base(Url, Method, RequestHeaderList) { }
                    public ModuleList(IUrl url) : base(url.Url, Method, RequestHeaderList) { }

                    public ValueTask<Responses> SendRequestAsync()
                    {
                        return base.SendRequestAsync<Responses>();
                    }
                }
            }
        }

        public static class Post
        {
            public static string Method = UnityWebRequest.kHttpVerbPOST;

            public static class SdApi
            {
                public static class V1
                {
                    public class Options : WebRequestWrapper
                    {
                        // static
                        public static string Paths => "/sdapi/v1/options";
                        public static string Url => $"{ServerUrl}{Paths}";
                        public static IReadOnlyList<RequestHeader> RequestHeaderList { get; }

                        static Options()
                        {
                            RequestHeaderList = new List<RequestHeader>() { new RequestHeader(ContentType, ApplicationJson), };
                        }

                        // interface
                        public interface IUrl
                        {
                            public string Url { get; }
                        }

                        // internal class
                        [Serializable]
                        public class RequestBody : IRequestBody
                        {
                            public string sd_model_checkpoint;

                            public RequestBody()
                            {

                            }

                            public RequestBody(string model)
                            {
                                sd_model_checkpoint = model;
                            }
                        }

                        [Serializable]
                        public class Responses : IResponses
                        {
                            
                        }

                        // method
                        public Options() : base(Url, Method, RequestHeaderList) { }
                        public Options(IUrl url) : base(url.Url, Method, RequestHeaderList) { }

                        public RequestBody GetRequestBody()
                        {
                            return new RequestBody();
                        }

                        public RequestBody GetRequestBody(string model)
                        {
                            return new RequestBody(model);
                        }

                        public ValueTask<Responses> SendRequestAsync(RequestBody body)
                        {
                            return base.SendRequestAsync<RequestBody, Responses>(body);
                        }
                    }

                    public class Txt2Img : WebRequestWrapper
                    {
                        // static
                        public static string Paths => "/sdapi/v1/txt2img";
                        public static string Url => $"{ServerUrl}{Paths}";
                        public static IReadOnlyList<RequestHeader> RequestHeaderList { get; }

                        static Txt2Img()
                        {
                            RequestHeaderList = new List<RequestHeader>() { new RequestHeader(ContentType, ApplicationJson), };
                        }

                        // interface
                        public interface IUrl
                        {
                            public string Url { get; }
                        }

                        // internal class
                        [Serializable]
                        public class RequestBody : IRequestBody
                        {
                            public interface IDefault
                            {
                                public string Sampler { get; }
                                public int Width { get; }
                                public int Height { get; }
                                public int Steps { get; }
                                public double CfgScale { get; }
                                public long Seed { get; }
                            }

                            public string sampler_index = "Euler a";
                            public string prompt = "";
                            public string negative_prompt = "";
                            public long seed = -1;
                            public int steps = 20;
                            public double cfg_scale = 7;
                            public int width = 960;
                            public int height = 540;
                            public double denoising_strength = 0.0f;

                            public RequestBody(IDefault def)
                            {
                                sampler_index = def.Sampler;
                                width = def.Width;
                                height = def.Height;
                                seed = def.Seed;
                                steps = def.Steps;
                                cfg_scale = def.CfgScale;
                            }
                        }

                        [Serializable]
                        public class Responses : IResponses
                        {
                            public string[] images;
                            public RequestBody parameters;
                            public string info;

                            [Serializable]
                            public class Info
                            {
                                public string prompt;
                                public string[] all_prompts;
                                public string negative_prompt;
                                public string[] all_negative_prompts;
                                public long seed;
                                public long[] all_seeds;
                                public long subseed;
                                public long[] all_subseeds;
                                public double subseed_strength;
                                public int width;
                                public int height;
                                public string sampler_name;
                                public int steps;
                                public int batch_size;
                                public bool restore_faces;
                                public string face_restoration_model;
                                public string sd_model_hash;
                                public long seed_resize_from_w;
                                public long seed_resize_from_h;
                                public double denoising_strength;
                                public string extra_generation_params;
                                public int index_of_first_image;
                                public string[] infotexts;
                                public string[] styles;
                                public string job_timestamp;
                                public int clip_skip;
                                public bool is_using_inpainting_conditioning;
                            }

                            public byte[] GetImage()
                            {
                                return GetImageByteArray(images);
                            }

                            public Info GetInfo()
                            {
                                return JsonUtility.FromJson<Info>(info);
                            }
                        }

                        // method
                        public Txt2Img() : base(Url, Method, RequestHeaderList) { }
                        public Txt2Img(IUrl url) : base(url.Url, Method, RequestHeaderList) { }

                        public RequestBody GetRequestBody(RequestBody.IDefault def)
                        {
                            return new RequestBody(def);
                        }

                        public ValueTask<Responses> SendRequestAsync(RequestBody body)
                        {
                            return base.SendRequestAsync<RequestBody, Responses>(body);
                        }
                    }

                    public class Img2Img : WebRequestWrapper
                    {
                        // static
                        public static string Paths => "/sdapi/v1/img2img";
                        public static string Url => $"{ServerUrl}{Paths}";
                        public static IReadOnlyList<RequestHeader> RequestHeaderList { get; }

                        static Img2Img()
                        {
                            RequestHeaderList = new List<RequestHeader>() { new RequestHeader(ContentType, ApplicationJson), };
                        }

                        // interface
                        public interface IUrl
                        {
                            public string Url { get; }
                        }

                        // internal class
                        [Serializable]
                        public class RequestBody : IRequestBody
                        {
                            public interface IDefault
                            {
                                public string Sampler { get; }
                                public int Width { get; }
                                public int Height { get; }
                                public int Steps { get; }
                                public double CfgScale { get; }
                                public long Seed { get; }
                            }

                            public string[] init_images;
                            public string sampler_index = "Euler a";
                            public string prompt = "";
                            public string negative_prompt = "";
                            public long seed = -1;
                            public int steps = 20;
                            public double cfg_scale = 7;
                            public int width = 960;
                            public int height = 540;
                            public double denoising_strength = 0.75f;

                            public RequestBody(IDefault def)
                            {
                                sampler_index = def.Sampler;
                                width = def.Width;
                                height = def.Height;
                                seed = def.Seed;
                                steps = def.Steps;
                                cfg_scale = def.CfgScale;
                            }

                            public void SetImage(byte[] data)
                            {
                                init_images = GetImageStringArray(data);
                            }
                        }

                        [Serializable]
                        public class Responses : IResponses
                        {
                            public string[] images;
                            public RequestBody parameters;
                            public string info;

                            [Serializable]
                            public class Info
                            {
                                public string prompt;
                                public string[] all_prompts;
                                public string negative_prompt;
                                public string[] all_negative_prompts;
                                public long seed;
                                public long[] all_seeds;
                                public long subseed;
                                public long[] all_subseeds;
                                public double subseed_strength;
                                public int width;
                                public int height;
                                public string sampler_name;
                                public int steps;
                                public int batch_size;
                                public bool restore_faces;
                                public string face_restoration_model;
                                public string sd_model_hash;
                                public long seed_resize_from_w;
                                public long seed_resize_from_h;
                                public double denoising_strength;
                                public string extra_generation_params;
                                public int index_of_first_image;
                                public string[] infotexts;
                                public string[] styles;
                                public string job_timestamp;
                                public int clip_skip;
                                public bool is_using_inpainting_conditioning;
                            }

                            public byte[] GetImage()
                            {
                                return GetImageByteArray(images);
                            }

                            public Info GetInfo()
                            {
                                return JsonUtility.FromJson<Info>(info);
                            }
                        }

                        // method
                        public Img2Img() : base(Url, Method, RequestHeaderList) { }
                        public Img2Img(IUrl url) : base(url.Url, Method, RequestHeaderList) { }

                        public RequestBody GetRequestBody(RequestBody.IDefault def)
                        {
                            return new RequestBody(def);
                        }

                        public ValueTask<Responses> SendRequestAsync(RequestBody body)
                        {
                            return base.SendRequestAsync<RequestBody, Responses>(body);
                        }
                    }

                    public class PngInfo : WebRequestWrapper
                    {
                        // static
                        public static string Paths => "/sdapi/v1/png-info";
                        public static string Url => $"{ServerUrl}{Paths}";
                        public static IReadOnlyList<RequestHeader> RequestHeaderList { get; }

                        static PngInfo()
                        {
                            RequestHeaderList = new List<RequestHeader>() { new RequestHeader(ContentType, ApplicationJson), };
                        }

                        // interface
                        public interface IUrl
                        {
                            public string Url { get; }
                        }

                        // internal class
                        [Serializable]
                        public class RequestBody : IRequestBody
                        {
                            public string image;

                            public RequestBody()
                            {

                            }

                            public RequestBody(byte[] data)
                            {
                                SetImage(data);
                            }

                            public void SetImage(byte[] data)
                            {
                                image = GetImageString(data);
                            }
                        }

                        [Serializable]
                        public class Responses : IResponses
                        {
                            public string info;

                            public IReadOnlyDictionary<string, string> ParseInfo()
                            {
                                var dic = new Dictionary<string, string>();

                                var lines = info.Split('\n');

                                if (lines.Length == 2)
                                {
                                    dic.Add(LabelPrompt, lines[0]);

                                    ParseInfoItems(dic, lines[1]);
                                }
                                else if (lines.Length == 3)
                                {
                                    dic.Add(LabelPrompt, lines[0]);
                                    dic.Add(LabelNegativePrompt, lines[1]);

                                    ParseInfoItems(dic, lines[2]);
                                }

                                return dic;
                            }

                            private static void ParseInfoItems(Dictionary<string, string> dic, string line)
                            {
                                var items = line.Split(", ");

                                foreach (var item in items)
                                {
                                    var split = item.Split(": ");

                                    if (split.Length >= 2)
                                    {
                                        dic.Add(split[0], split[1]);
                                    }
                                }
                            }
                        }

                        // method
                        public PngInfo() : base(Url, Method, RequestHeaderList) { }
                        public PngInfo(IUrl url) : base(url.Url, Method, RequestHeaderList) { }

                        public RequestBody GetRequestBody()
                        {
                            return new RequestBody();
                        }

                        public RequestBody GetRequestBody(byte[] data)
                        {
                            return new RequestBody(data);
                        }

                        public ValueTask<Responses> SendRequestAsync(RequestBody body)
                        {
                            return base.SendRequestAsync<RequestBody, Responses>(body);
                        }
                    }

                    public static class Extension
                    {
                        [Serializable]
                        public class ControlNet
                        {
                            [Serializable]
                            public class UnitRequest
                            {
                                public interface IDefault
                                {
                                    public double Weight { get; }
                                    public ResizeMode ResizeMode { get; }
                                    public bool LowVram { get; }
                                    public int ProcessorRes { get; }
                                    public double GuidanceStart { get; }
                                    public double GuidanceEnd { get; }
                                    public ControlMode ControlMode { get; }
                                    public bool PixcelPerfect { get; }
                                }

                                [Serializable]
                                public enum ResizeMode : int
                                {
                                    JustResize = 0,
                                    ScaleToFit = 1,
                                    Envelope = 2,
                                }

                                [Serializable]
                                public enum ControlMode : int
                                {
                                    Balanced = 0,
                                    MyPromptIsMoreImportant = 1,
                                    ControlNetIsMoreImportant = 2,
                                }

                                public string input_image = null;
                                public string mask = null;
                                public string module = "none";
                                public string model = "None";
                                public double weight = 1.0f;
                                public ResizeMode resize_mode = ResizeMode.ScaleToFit;
                                public bool lowvram = false;
                                public int processor_res = 64;
                                public double threshold_a = 64.0f;
                                public double threshold_b = 64.0f;
                                public double guidance_start = 0.0f;
                                public double guidance_end = 1.0f;
                                public ControlMode control_mode = ControlMode.Balanced;
                                public bool pixel_perfect = false;

                                public UnitRequest(IDefault def)
                                {
                                    weight = def.Weight;
                                    resize_mode = def.ResizeMode;
                                    lowvram = def.LowVram;
                                    processor_res = def.ProcessorRes;
                                    guidance_start = def.GuidanceStart;
                                    guidance_end = def.GuidanceEnd;
                                    control_mode = def.ControlMode;
                                    pixel_perfect = def.PixcelPerfect;
                                }

                                public UnitRequest(IDefault def, string model, byte[] data) : this(def)
                                {
                                    SetModel(model);
                                    SetImage(data);
                                }

                                public void SetModel(string model)
                                {
                                    this.model = model;
                                }

                                public void SetImage(byte[] data)
                                {
                                    input_image = GetImageString(data);
                                }
                            }

                            public UnitRequest[] args;

                            public ControlNet(UnitRequest[] args)
                            {
                                this.args = args;
                            }

                            public ControlNet(UnitRequest arg) : this(new UnitRequest[] { arg })
                            {

                            }

                            public ControlNet(UnitRequest.IDefault def, string model, byte[] image) : this(new UnitRequest(def, model, image))
                            {

                            }
                        }

                        [Serializable]
                        public class AS_ControlNet
                        {
                            public ControlNet controlnet;

                            public AS_ControlNet(ControlNet cn)
                            {
                                controlnet = cn;
                            }
                        }

                        public class Txt2ImgWithControlNet : Txt2Img
                        {
                            [Serializable]
                            public new class RequestBody : Txt2Img.RequestBody
                            {
                                public AS_ControlNet alwayson_scripts;

                                public RequestBody(IDefault def) : base(def)
                                {
                            
                                }

                                public RequestBody(IDefault def, AS_ControlNet alwayson) : base(def)
                                {
                                    alwayson_scripts = alwayson;
                                }

                                public void SetAlwaysonScripts(ControlNet.UnitRequest[] args)
                                {
                                    var controlnet = new ControlNet(args);
                                    alwayson_scripts = new AS_ControlNet(controlnet);
                                }

                                public void SetAlwaysonScripts(ControlNet.UnitRequest arg)
                                {
                                    var controlnet = new ControlNet(arg);
                                    alwayson_scripts = new AS_ControlNet(controlnet);
                                }

                                public void SetAlwaysonScripts(ControlNet.UnitRequest.IDefault args, string model, byte[] image)
                                {
                                    var controlnet = new ControlNet(args, model, image);
                                    alwayson_scripts = new AS_ControlNet(controlnet);
                                }
                            }

                            public Txt2ImgWithControlNet() : base() { }
                            public Txt2ImgWithControlNet(IUrl url) : base(url) { }

                            public new RequestBody GetRequestBody(RequestBody.IDefault def)
                            {
                                return new RequestBody(def);
                            }
                        }

                        public class Img2ImgWithControlNet : Img2Img
                        {
                            [Serializable]
                            public new class RequestBody : Img2Img.RequestBody
                            {
                                public AS_ControlNet alwayson_scripts;

                                public RequestBody(IDefault def) : base(def)
                                {

                                }

                                public RequestBody(IDefault def, AS_ControlNet alwayson) : base(def)
                                {
                                    alwayson_scripts = alwayson;
                                }

                                public void SetAlwaysonScripts(ControlNet.UnitRequest[] args)
                                {
                                    var controlnet = new ControlNet(args);
                                    alwayson_scripts = new AS_ControlNet(controlnet);
                                }

                                public void SetAlwaysonScripts(ControlNet.UnitRequest arg)
                                {
                                    var controlnet = new ControlNet(arg);
                                    alwayson_scripts = new AS_ControlNet(controlnet);
                                }

                                public void SetAlwaysonScripts(ControlNet.UnitRequest.IDefault args, string model, byte[] image)
                                {
                                    var controlnet = new ControlNet(args, model, image);
                                    alwayson_scripts = new AS_ControlNet(controlnet);
                                }
                            }

                            public Img2ImgWithControlNet() : base() { }
                            public Img2ImgWithControlNet(IUrl url) : base(url) { }

                            public new RequestBody GetRequestBody(RequestBody.IDefault def)
                            {
                                return new RequestBody(def);
                            }
                        }
                    }
                }
            }

            [Obsolete]
            public static class ControlNet
            {
                [Obsolete]
                public class Txt2Img : WebRequestWrapper
                {
                    // static
                    public static string Paths => "/controlnet/txt2img";
                    public static string Url => $"{ServerUrl}{Paths}";
                    public static IReadOnlyList<RequestHeader> RequestHeaderList { get; }

                    static Txt2Img()
                    {
                        RequestHeaderList = new List<RequestHeader>() { new RequestHeader(ContentType, ApplicationJson), };
                    }

                    // interface
                    public interface IUrl
                    {
                        public string Url { get; }
                    }

                    // internal class
                    [Serializable]
                    public class RequestBody : IRequestBody
                    {
                        public interface IDefault
                        {
                            public string Sampler { get; }
                            public int Width { get; }
                            public int Height { get; }
                            public int Steps { get; }
                            public double CfgScale { get; }
                            public long Seed { get; }
                        }

                        public string[] controlnet_input_image;
                        public string controlnet_module = "none";
                        public string controlnet_model = "control_v11f1p_sd15_depth_fp16 [4b72d323]";
                        public string sampler_index = "Euler a";
                        public double controlnet_weight = 1.0f;
                        public string prompt = "";
                        public string negative_prompt = "";
                        public long seed = -1;
                        public int steps = 20;
                        public double cfg_scale = 7;
                        public int width = 960;
                        public int height = 540;
                        public double denoising_strength = 0.0f;

                        public RequestBody(IDefault def)
                        {
                            sampler_index = def.Sampler;
                            width = def.Width;
                            height = def.Height;
                            seed = def.Seed;
                            steps = def.Steps;
                            cfg_scale = def.CfgScale;
                        }

                        public void SetImage(byte[] data)
                        {
                            controlnet_input_image = GetImageStringArray(data);
                        }
                    }

                    [Serializable]
                    public class Responses : IResponses
                    {
                        public string[] images;

                        public byte[] GetImage()
                        {
                            return GetImageByteArray(images);
                        }
                    }

                    // method
                    public Txt2Img() : base(Url, Method, RequestHeaderList) { }
                    public Txt2Img(IUrl url) : base(url.Url, Method, RequestHeaderList) { }

                    public RequestBody GetRequestBody(RequestBody.IDefault def)
                    {
                        return new RequestBody(def);
                    }

                    public ValueTask<Responses> SendRequestAsync(RequestBody body)
                    {
                        return base.SendRequestAsync<RequestBody, Responses>(body);
                    }
                }
            }
        }
    }
}
