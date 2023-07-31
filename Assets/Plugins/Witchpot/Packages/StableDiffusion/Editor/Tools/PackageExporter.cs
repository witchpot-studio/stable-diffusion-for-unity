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
                    EditorPaths.WITCHPOT_ROOT,
                };

#if WITCHPOT_DEVELOPMENT
        [MenuItem("Witchpot/Develop/Export Package", priority = 100)]
#endif
        public static void Export()
        {
            Debug.Log($"Start Package Exporting...");

            if (DependenciesInstaller.UninstalledFlag)
            {
                Debug.Log($"Reset UninstaledFlag to export");
                DependenciesInstaller.ResetUninstalled();

                AssetDatabase.Refresh();

                EditorApplication.delayCall += () =>
                {
                    ExportPackage();
                    DependenciesInstaller.SetUninstalled();
                };
            }
            else
            {
                EditorApplication.delayCall += () =>
                {
                    ExportPackage();
                };
            }
        }

        public static void ExportPackage()
        {
            AssetDatabase.ExportPackage(
                asset,
                "stable-diffusion-for-unity.unitypackage",
                ExportPackageOptions.Recurse
            );

            Debug.Log($"Done Export");
        }
    }
}
