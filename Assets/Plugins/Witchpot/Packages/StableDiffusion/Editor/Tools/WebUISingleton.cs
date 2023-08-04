using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Witchpot.Runtime.StableDiffusion;


namespace Witchpot.Editor.StableDiffusion
{
    [InitializeOnLoad]
    [FilePath(FilePath, FilePathAttribute.Location.PreferencesFolder)]
    public class WebUISingleton : ScriptableSingleton<WebUISingleton>
    {
        public const string FilePath = EditorPaths.WITCHPOT_PREFERENCES_FOLDER + "\\WebUISingleton.asset";

        public enum StableDiffusionMode
        {
            Internal,
            External
        }

        #region Static

        [MenuItem("Witchpot/Start Server")]
        public static void Start()
        {
            instance._Start();
        }

        [MenuItem("Witchpot/Stop Server")]
        public static void Stop()
        {
            instance._Stop();
        }

        // [MenuItem("Witchpot/Server Log")]
        public static void ServerOutputToLog()
        {
            instance._ServerOutputToLog();
        }

        public static IWebUIStatus Status => instance._webUIStatus;

        public static bool IsAvailable => instance != null;

        public static SerializedObject GetSerialized()
        {
            if (instance.hideFlags.HasFlag(HideFlags.NotEditable))
            {
                instance.hideFlags -= HideFlags.NotEditable;
            }

            return new SerializedObject(instance);
        }

        #endregion

        //----------
        // TODO:  To use RedirectStandardOutput function, add "set PYTHONUNBUFFERED=1" in "~\witchpot\StableDiffusion.WebUI@1.2.0\environment.bat"
        // (original includedd in "\Assets\Plugins\Witchpot\Packages\StableDiffusion\Installers\StableDiffusion.WebUI\Package.zip")
        //----------

        private enum ServerType
        {
            Internal,
            InternalBat,
            External,
        }

        private static ServerType _server = ServerType.External;

        private bool _UseShellExecute = true;
        private bool _CreateNoWindow = false;
        private bool _UseRedirectStandardOutput = false;
        private string _ProcessName;
        private string _WorkingDirectory;
        private string _FileName;
        private string _Arguments;
        private string _Verb;

        // HACK: the flag of server started or not
        [SerializeField]
        private List<int> _pidList = new List<int>();

        [SerializeField]
        private StableDiffusionMode _usingStableDiffusion = StableDiffusionMode.Internal;

        [SerializeField]
        private string _externalPath = string.Empty;

        private StringBuilder _Output = new StringBuilder();

        static WebUISingleton()
        {
            // It needs delay because the unity function call is not allowed in the static constructor.
            EditorApplication.delayCall += InitializeOnLoad;
        }

        private static void InitializeOnLoad()
        {
            // UnityEngine.Debug.Log($"WebUISingleton InitializeOnLoad");

            // It invoke OnEnable
            if (instance) { }
        }

        private void Save()
        {
            Save(true);
        }

        private void OnEnable()
        {
            // UnityEngine.Debug.Log($"WebUISingleton OnEnable");

            // foreach (var pid in _PidList)
            // {
            //     UnityEngine.Debug.Log($"listed server (PID:{pid})");
            // }

            UpdateProcessStartInfo();

            RestoreEventsForListedProcess();
        }

        private void OnDisable()
        {
            // UnityEngine.Debug.Log($"WebUISingleton OnDisable");

            Save();
        }

        #region WebUIStatus

        private WebUIStatus _webUIStatus = new WebUIStatus(IPAddress.Any, 50007);

        public interface IWebUIStatus
        {
            public interface IArgs
            {
                bool ServerStarted { get; }
                string ServerAppId { get; }
                bool ServerReady { get; }
            }

            public bool ServerStarted { get; }
            public string ServerAppId { get; }
            public bool ServerReady { get; }

            public event EventHandler<IArgs> Changed;
        }

        public class WebUIStatus : IWebUIStatus
        {
            public WebUIStatus(IPAddress adress, int port)
            {
                _endPoint = new(adress, port);
            }

            // Event
            public class Args : EventArgs, IWebUIStatus.IArgs
            {
                public bool ServerStarted { get; set; } = false;
                public string ServerAppId { get; set; } = string.Empty;
                public bool ServerReady { get; set; } = false;
            }

            public bool ServerStarted => _args.ServerStarted;
            public string ServerAppId => _args.ServerAppId;
            public bool ServerReady => _args.ServerReady;

            public event EventHandler<IWebUIStatus.IArgs> Changed;

            private Args _args = new();

            private void InvokeWebUIStatusChanged()
            {
                if (Changed != null) { Changed.Invoke(this, _args); }
            }

            public void SetServerStarted(bool started)
            {
                _args.ServerStarted = started;
                InvokeWebUIStatusChanged();
            }

            public void SetServerAppId(string id)
            {
                _args.ServerAppId = id;
                InvokeWebUIStatusChanged();
            }

            public void SetServerReady(bool ready)
            {
                _args.ServerReady = ready;
                InvokeWebUIStatusChanged();
            }

            public void SetServerArgs(bool started, string id, bool ready)
            {
                _args.ServerStarted = started;
                _args.ServerAppId = id;
                _args.ServerReady = ready;
                InvokeWebUIStatusChanged();
            }

            public void ResetServerArgs()
            {
                _args.ServerStarted = false;
                _args.ServerAppId = string.Empty;
                _args.ServerReady = false;
                InvokeWebUIStatusChanged();
            }

            // Get.AppId
            bool _checkEnabled = false;

            public async ValueTask<bool> ValidateConnection()
            {
                try
                {
                    string id;

                    using (var client = new StableDiffusionWebUIClient.Get.AppId())
                    {
                        var result = await client.SendRequestAsync();
                        id = result.app_id;
                    }

                    if (Regex.IsMatch(id, "^[0-9]+$"))
                    {
                        SetServerArgs(ServerStarted, id, true);
                        UnityEngine.Debug.Log($"Server AppId : {ServerAppId}");
                    }
                    else
                    {
                        SetServerArgs(ServerStarted, string.Empty, true);
                    }

                    string versionSD;
                    using (var client = new StableDiffusionWebUIClient.Get.Config())
                    {
                        var result = await client.SendRequestAsync();
                        versionSD = result.GetVersion();
                    }
                    UnityEngine.Debug.Log($"StableDiffusion version : {versionSD}");

                    if (!string.Equals(versionSD, DependenciesInstaller.WebuiExpectedVersion))
                    {
                        UnityEngine.Debug.LogWarning($"StableDiffusion version is different. Reinstallation will be needed.\nCurrent : {versionSD}\nExpected : {DependenciesInstaller.WebuiExpectedVersion}");
                    }

                    int versionCN;
                    using (var client = new StableDiffusionWebUIClient.Get.ControlNet.Version())
                    {
                        var result = await client.SendRequestAsync();
                        versionCN = result.version;
                    }
                    UnityEngine.Debug.Log($"ControlNet version : {versionCN}");

                    if (!int.Equals(versionCN, DependenciesInstaller.WebuiControlNetExpectedVersion))
                    {
                        UnityEngine.Debug.LogWarning($"ControlNet version is different. Reinstallation will be needed.\nCurrent : {versionCN}\nExpected : {DependenciesInstaller.WebuiControlNetExpectedVersion}");
                    }

                    UnityEngine.Debug.Log($"Check done.");
                    return true;
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.Log($"Checking ... {e.Message}");
                    return false;
                }
            }

            public async ValueTask ValidateConnectionContinuously()
            {
                if (_checkEnabled)
                {
                    UnityEngine.Debug.LogWarning($"Check already active.");
                    return;
                }

                _checkEnabled = true;

                try
                {
                    while (_checkEnabled)
                    {
                        if (await ValidateConnection())
                        {
                            Close();
                        }
                        else
                        {
                            await Task.Delay(3000);
                        }
                    }
                }
                finally
                {
                    Close();
                }
            }

            // HACK:UDP Client ver
            private static IPEndPoint _endPoint;
            private UdpClient _client;
            private bool _receiveEnabled = false;

            public async ValueTask ConnectUdp()
            {
                if (_receiveEnabled)
                {
                    UnityEngine.Debug.Log($"UdpClient already receiving.");
                    return;
                }

                try
                {
                    _client = new(_endPoint);
                    _receiveEnabled = true;

                    while (_receiveEnabled)
                    {
                        var result = await _client.ReceiveAsync();

                        var str = Encoding.UTF8.GetString(result.Buffer);

                        if (Regex.IsMatch(str, "^[0-9]+$"))
                        {
                            SetServerAppId(str);
                            UnityEngine.Debug.Log($"Server PID : {ServerAppId}");
                        }
                        else
                        {
                            switch (str)
                            {
                                case "start":
                                    SetServerStarted(true);
                                    UnityEngine.Debug.Log("Server starting.");
                                    break;

                                case "ready":
                                    Close();
                                    SetServerReady(true);
                                    UnityEngine.Debug.Log("Server ready.");
                                    break;
                            }
                        }
                    }
                }
                catch (ObjectDisposedException)
                {
                    UnityEngine.Debug.LogWarning("UdpClient closed.");
                }
                finally
                {
                    Close();
                }
            }

            public void Close()
            {
                _checkEnabled = false;
                _receiveEnabled = false;

                if (_client != null)
                {
                    _client.Close();
                    _client = null;
                }
            }
        }

        #endregion

        #region ProcessEvent

        private bool DoActionForListedProcess(Action<Process> processFound, Action<int> processNotFound)
        {
            bool foundAtLeastOne = false;

            Span<int> buffer = stackalloc int[_pidList.Count];

            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = _pidList[i];
            }

            Process[] Processes = Process.GetProcessesByName(_ProcessName);

            foreach (var id in buffer)
            {
                bool found = false;

                foreach (Process process in Processes)
                {
                    if (process.Id == id)
                    {
                        if (processFound != null) { processFound(process); }

                        found = true;
                        foundAtLeastOne = true;
                        continue;
                    }
                }

                if (!found)
                {
                    if (processNotFound != null) { processNotFound(id); }
                }
            }

            return foundAtLeastOne;
        }

        private void AddProcessEvent(Process process)
        {
            if (_UseRedirectStandardOutput)
            {
                process.OutputDataReceived -= OnOutputDataReceived;
                process.OutputDataReceived += OnOutputDataReceived;
            }

            process.EnableRaisingEvents = true;

            process.Exited -= OnExited;
            process.Exited += OnExited;
        }

        private void OnOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                _Output.Append($"{e.Data}\n");
            }
        }

        private void OnExited(object sender, EventArgs e)
        {
            var process = (Process)sender;

            if (_pidList.Contains(process.Id))
            {
                _pidList.Remove(process.Id);
            }

            UnityEngine.Debug.Log($"Server stoped (PID:{process.Id})");

            _webUIStatus.Close();
            _webUIStatus.ResetServerArgs();
        }

        private bool RestoreEventsForListedProcess()
        {
            return DoActionForListedProcess(RegisterEvents, x => _pidList.Remove(x));
        }

        private void RegisterEvents(Process process)
        {
            AddProcessEvent(process);

            _webUIStatus.SetServerStarted(true);
            _webUIStatus.ValidateConnectionContinuously().Forget();
        }

        #endregion

        private string GetArgmentFull(string bat) => $"{_Arguments} {bat}";

        private string GetBatPath()
        {
            switch (_usingStableDiffusion)
            {
                default:
                case StableDiffusionMode.Internal:
                    return DependenciesInstaller.DestinationBatPath;

                case StableDiffusionMode.External:
                    return _externalPath;
            }
        }

        private void UpdateProcessStartInfo()
        {
            switch (_server)
            {
                case ServerType.Internal:
                    _UseShellExecute = false;
                    _CreateNoWindow = true;
                    _UseRedirectStandardOutput = true;
                    _ProcessName = "python";
                    _WorkingDirectory = string.Empty;
                    _FileName = DependenciesInstaller.DestinationPythonExePath;
                    _Arguments = $"-u {EditorPaths.WEBUI_SCRIPT_PATH}";
                    _Verb = string.Empty;
                    break;

                case ServerType.InternalBat:
                    _UseShellExecute = false;
                    _CreateNoWindow = true;
                    _UseRedirectStandardOutput = true;
                    _ProcessName = "cmd";
                    _WorkingDirectory = string.Empty;
                    _FileName = "cmd.exe";
                    _Arguments = $"/c";
                    _Verb = "RunAs";
                    break;

                default:
                case ServerType.External:
                    _UseShellExecute = true;
                    _CreateNoWindow = false;
                    _UseRedirectStandardOutput = false;
                    _ProcessName = "python";
                    _WorkingDirectory = string.Empty;
                    _FileName = DependenciesInstaller.DestinationPythonExePath;
                    _Arguments = $"{EditorPaths.WEBUI_SCRIPT_PATH}";
                    _Verb = string.Empty;
                    break;
            }
        }

        private void _Start()
        {
            UpdateProcessStartInfo();

            if (!System.IO.File.Exists(_FileName))
            {
                // TODO: The message should include the next action.
                UnityEngine.Debug.LogError($"{_FileName} not found.");
                return;
            }

            if (RestoreEventsForListedProcess())
            {
                UnityEngine.Debug.LogWarning($"Server alredy started.");
                return;
            }

            //_webUIStatus.Connect().Forget();

            var process = new Process();

            process.StartInfo.FileName = _FileName;
            process.StartInfo.Arguments = GetArgmentFull(GetBatPath());
            process.StartInfo.Verb = _Verb;

            process.StartInfo.UseShellExecute = _UseShellExecute;
            process.StartInfo.CreateNoWindow = _CreateNoWindow;
            process.StartInfo.RedirectStandardOutput = _UseRedirectStandardOutput;

            if (_UseRedirectStandardOutput)
            {
                process.StartInfo.StandardOutputEncoding = System.Text.Encoding.GetEncoding("shift_jis");

                _Output.Clear();
            }

            process.Start();

            if (_UseRedirectStandardOutput)
            {
                process.BeginOutputReadLine();
            }

            AddProcessEvent(process);

            _pidList.Add(process.Id);
            Save();

            UnityEngine.Debug.Log($"Start server. (PID:{process.Id})");

            _webUIStatus.SetServerStarted(true);
            _webUIStatus.ValidateConnectionContinuously().Forget();
        }

        private void Kill(Process process)
        {
            if (process.HasExited)
            {
                // UnityEngine.Debug.LogWarning($"Process has exited. (PID:{pid})");
                return;
            }

            UnityEngine.Debug.Log($"Stop server. (PID:{process.Id})");

            KillProcessTree(process);
            process.WaitForExit();

            _pidList.Remove(process.Id);
        }

        private void KillProcessTree(Process process)
        {
            string taskkill = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "taskkill.exe");

            using (var procKiller = new Process())
            {
                procKiller.StartInfo.FileName = taskkill;
                procKiller.StartInfo.Arguments = $"/PID {process.Id} /T /F";
                procKiller.StartInfo.CreateNoWindow = true;
                procKiller.StartInfo.UseShellExecute = false;
                procKiller.Start();
                procKiller.WaitForExit();
            }
        }

        private bool _Stop()
        {
            return DoActionForListedProcess(Kill, null);
        }

        private void _ServerOutputToLog()
        {
            UnityEngine.Debug.Log($"Server log\n{_Output}");
        }
    }
}
