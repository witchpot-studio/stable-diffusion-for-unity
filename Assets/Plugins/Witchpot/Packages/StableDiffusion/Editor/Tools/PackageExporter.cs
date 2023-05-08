using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Witchpot.Editor.StableDiffusion
{
    public class PackageExporter
    {
        private static string[] asset = new string[]
                {
                    "Assets/Plugins/Witchpot",
                };

#if WITCHPOT_DEVELOPMENT
        [MenuItem("Witchpot/Develop/Export Package", priority = 100)]
#endif
        public static void Export()
        {
            if (DependenciesInstaller.UninstalledFlag)
            {
                DependenciesInstaller.ResetUninstalled();
                ExportPackage();
                DependenciesInstaller.SetUninstalled();
            }
            else
            {
                ExportPackage();
            }
        }

        public static void ExportPackage()
        {
            AssetDatabase.ExportPackage(
                asset,
                "stable-diffusion-for-unity.unitypackage",
                ExportPackageOptions.Recurse
            );
        }
    }
}
