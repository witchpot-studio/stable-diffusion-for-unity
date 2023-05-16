using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;

namespace Witchpot.Editor.StableDiffusion
{
    [InitializeOnLoad]
    [FilePath(FilePath, FilePathAttribute.Location.ProjectFolder)]
    public class DependenciesInstaller : ScriptableSingleton<DependenciesInstaller>
    {
        public const string FilePath = "Assets/Plugins/Witchpot/Packages/StableDiffusion/Editor/Tools/DependenciesInstaller.asset";

        public enum RootType
        {
            None,
            UnityProject,
            SystemUserProfile
        }

        public static bool UninstalledFlag => instance._uninstalled_; 

        static DependenciesInstaller()
        {
            // It needs delay because the unity function call is not allowed in the static constructor.
            EditorApplication.delayCall += OnDelayCall;
        }

        private static void OnDelayCall()
        {
            // UnityEngine.Debug.Log($"DependenciesInstaller OnDelayCall");

            // It invoke Awake
            if (instance) { }
        }

        [MenuItem("Witchpot/Utility/(Re)Install Dependencies", priority = 10)]
        public static void Install()
        {
            instance.InstallDependencies();
        }

        [MenuItem("Witchpot/Utility/Uninstall Dependencies", priority = 11)]
        public static void Uninstall()
        {
            instance.UninstallDependencies();
        }

        [MenuItem("Witchpot/Utility/Open Installed Dir", priority = 12)]
        public static void Open()
        {
            instance.OpenInstalledDir();
        }

        private static void Save()
        {
            EditorApplication.delayCall += () => instance.Save(true);
        }

#if WITCHPOT_DEVELOPMENT
        [MenuItem("Witchpot/Develop/Dependencies Installer/Set Uninstalled Flag")]
#endif
        public static void SetUninstalled()
        {
            instance._uninstalled_ = true;
            Save();
        }

#if WITCHPOT_DEVELOPMENT
        [MenuItem("Witchpot/Develop/Dependencies Installer/Reset Uninstalled Flag")]
#endif
        public static void ResetUninstalled()
        {
            instance._uninstalled_ = false;
            Save();
        }

        private static string GetRootPath(RootType type)
        {
            switch (type)
            {
                case RootType.None:
                    return string.Empty;

                default:
                case RootType.UnityProject:
                    return System.IO.Path.GetDirectoryName(Application.dataPath);

                case RootType.SystemUserProfile:
                    return System.Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            }
        }

        public static void DeleteDirectory(string targetDir)
        {
            File.SetAttributes(targetDir, FileAttributes.Normal);

            string[] files = Directory.GetFiles(targetDir);
            string[] dirs = Directory.GetDirectories(targetDir);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }

            Directory.Delete(targetDir, false);
        }

        [SerializeField]
        private RootType _zipRootType; // = RootType.UnityProject;

        [SerializeField]
        private string _zipRelationalPath; // = "Assets\\Plugins\\Witchpot\\Packages\\StableDiffusion\\Installers\\StableDiffusion.WebUI\\Package.zip";

        [SerializeField]
        private RootType _destinationRootType; // = RootType.SystemUserProfile;

        [SerializeField]
        private string _destinationRelationalPath; // = "Witchpot";

        [SerializeField]
        private string _destinationRelationalBatPath = "Witchpot\\StableDiffusion.WebUI@1.2.0\\run.bat";

#pragma warning disable CS0414
        // It doesn't work correctly if the name is _uninstalled
        [SerializeField]
        private bool _uninstalled_; // = false;
#pragma warning restore CS0414

        public string ZipAbsolutePath => Path.Combine(GetRootPath(_zipRootType), _zipRelationalPath);
        public string DestinationAbsolutePath => Path.Combine(GetRootPath(_destinationRootType), _destinationRelationalPath);
        public string DestinationBatPath => Path.Combine(GetRootPath(_destinationRootType), _destinationRelationalBatPath);

        private void OnEnable()
        {
            // UnityEngine.Debug.Log($"DependenciesInstaller OnEnable : {DependenciesInstalled.Flag} {_uninstalled_}");

#if !WITCHPOT_DEVELOPMENT
            if (!DependenciesInstalled.Flag && !_uninstalled_)
            {
                InstallDependencies();
            }
#endif
        }

        private void OnDisable()
        {
            // It is bad, because generating .asset file after package deleted
            // Save();
        }

        private const string _installTitle = "Installing dependencies";
        private const string _installOK = "Delete and Install";
        private const string _installNG = "Cancel";

        private void InstallDependencies()
        {
            try
            {
                Debug.Log($"Installing dependencies");

                // Zip
                var zipAbsolutePath = ZipAbsolutePath;

                if (!File.Exists(zipAbsolutePath))
                {
                    Debug.LogError($"Package zip file not found. Abort installation.\n{zipAbsolutePath}");
                    return;
                }

                // Destination
                var destinationAbsolutePath = DestinationAbsolutePath;
                var destinationBatPath = DestinationBatPath;

                if (Directory.Exists(destinationAbsolutePath))
                {
                    string body = $"Try to install dependencies, but destination dir already exist.\nAre you sure you want to delete it?\n\n{destinationAbsolutePath}";

                    if (EditorUtility.DisplayDialog(_installTitle, body, _installOK, _installNG))
                    {
                        try
                        {
                            DeleteDirectory(destinationAbsolutePath);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"Failed to delete destination directory. Abort installation.\n{e}");
                            return;
                        }
                    }
                    else
                    {
                        Debug.Log("Installation canceled");
                        return;
                    }
                }

                System.IO.Compression.ZipFile.ExtractToDirectory(zipAbsolutePath, destinationAbsolutePath, true);

                Debug.Log($"Installation finished");
                ResetUninstalled();

                Preferences.Open();
            }
            finally
            {
                DependenciesInstalled.SetDoneInitialInstall();
            }
        }

        private const string _uninstallTitle = "Uninstalling dependencies";
        private const string _uninstallOK = "Delete";
        private const string _uninstallNG = "Cancel";

        private void UninstallDependencies()
        {
            // Destination
            var destinationAbsolutePath = DestinationAbsolutePath;

            if (Directory.Exists(destinationAbsolutePath))
            {
                string body = $"Are you sure you want to delete it?\n\n{destinationAbsolutePath}";

                if (EditorUtility.DisplayDialog(_uninstallTitle, body, _uninstallOK, _uninstallNG))
                {
                    try
                    {
                        DeleteDirectory(destinationAbsolutePath);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Failed to delete destination directory. Abort Uninstallation.\n{e}");
                        return;
                    }
                }
                else
                {
                    Debug.Log("Uninstallation canceled");
                    return;
                }

                Debug.Log($"Uninstallation finished");
                SetUninstalled();
                DependenciesInstalled.ResetDoneInitialInstall();
            }
            else
            {
                Debug.LogError($"Destination directory not found. Abort Uninstallation.");
                SetUninstalled();
            }
        }

        private void OpenInstalledDir()
        {
            var destinationAbsolutePath = DestinationAbsolutePath;

            if (Directory.Exists(destinationAbsolutePath))
            {
                Application.OpenURL(destinationAbsolutePath);
            }
            else
            {
                Debug.LogError($"Installed dir not found : {destinationAbsolutePath}");
            }
        }
    }
}
