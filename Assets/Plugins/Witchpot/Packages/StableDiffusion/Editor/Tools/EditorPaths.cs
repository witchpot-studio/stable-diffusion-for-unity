using System;

namespace Witchpot.Editor.StableDiffusion
{
    public static class EditorPaths
    {
        public const string WITCHPOT_ROOT = "Assets/Plugins/Witchpot/";
        public const string WEBUI_SCRIPT_PATH = "Assets\\Plugins\\Witchpot\\Packages\\StableDiffusion\\Editor\\Tools\\run.py";
        public const string WEBUI_SCRIPT_BAT_PATH = "~\\witchpot\\StableDiffusion.WebUI@1.2.0\\run.bat";
        public static string PYTHON_EXE_PATH = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\witchpot\\StableDiffusion.WebUI@1.2.0\\system\\python\\python.exe";

        public const string WEBUI_EXPECTED_VERSION = "3.32.0";
        public const int WEBUI_CONTROLNET_EXPECTED_VERSION = 2;

        public const string WITCHPOT_DOCUMENT_URL = "https://docs.witchpot.com/";
        public const string WITCHPOT_DISCORD_JP_URL = "https://t.co/z0Qt556Vnv";
        public const string WITCHPOT_DISCORD_EN_URL = "https://discord.gg/kfkwA8R67t";
    }
}
