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
        public const string FilePath = EditorPaths.WITCHPOT_ROOT + "\\Packages\\StableDiffusion\\Editor\\Tools\\DependenciesInstaller.asset";

        public enum RootType
        {
            None,
            UnityProject,
            SystemUserProfile
        }

        public static bool IsAvailable => instance != null;
        public static bool UninstalledFlag => instance._uninstalled_;
        public static bool IsModified => instance.modified;

        public static string ZipAbsolutePath => Path.Combine(GetRootPath(instance._zipRootType), instance._zipRelationalPath);
        public static string DestinationAbsolutePath => Path.Combine(GetRootPath(instance._destinationRootType), instance._destinationRelationalPath);
        public static string DestinationBatPath => Path.Combine(DestinationAbsolutePath, instance._zipedRootFolderName, instance._zipedBatPath);
        public static string DestinationPythonExePath => Path.Combine(DestinationAbsolutePath, instance._zipedRootFolderName, instance._zipedPythonExePath);

        public static string WebuiExpectedVersion => instance._webuiExpectedVersion; // "3.32.0";
        public static int WebuiControlNetExpectedVersion => instance._webuiControlNetExpectedVersion; // 2;

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

        public static SerializedObject GetSerialized()
        {
            if (instance.hideFlags.HasFlag(HideFlags.NotEditable))
            {
                instance.hideFlags -= HideFlags.NotEditable;
            }

            return new SerializedObject(instance);
        }

        public static void SetModified()
        {
            instance.modified = true;
        }

        public static void Save()
        {
            EditorApplication.delayCall += () => instance.Save(true);
            instance.modified = false;
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
        private RootType _zipRootType;

        [SerializeField]
        private string _zipRelationalPath;

        [SerializeField]
        private string _zipedRootFolderName;

        [SerializeField]
        private string _zipedBatPath;

        [SerializeField]
        private string _zipedPythonExePath;

        [SerializeField]
        private string _webuiExpectedVersion;

        [SerializeField]
        private int _webuiControlNetExpectedVersion;

        [SerializeField]
        private RootType _destinationRootType;

        [SerializeField]
        private string _destinationRelationalPath;

#pragma warning disable CS0414
        // It doesn't work correctly if the name is _uninstalled
        [SerializeField]
        private bool _uninstalled_;
#pragma warning restore CS0414

        private bool modified = false;

        private void OnEnable()
        {
            //UnityEngine.Debug.Log($"DependenciesInstaller OnEnable : {DependenciesInstalled.Flag} {_uninstalled_}");

#if !WITCHPOT_DEVELOPMENT
            if (!DependenciesInstalled.Flag && !_uninstalled_)
            {
                InstallDependencies();
            }
#endif
        }

        /*
        private void OnDisable()
        {
            //UnityEngine.Debug.Log($"DependenciesInstaller OnDisable");

            // It is bad, because generating .asset file after package deleted
            // Save();
        }

        private void OnDestroy()
        {
            //UnityEngine.Debug.Log($"DependenciesInstaller OnDestroy");
        }
        */

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
