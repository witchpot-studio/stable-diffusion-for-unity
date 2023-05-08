using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Witchpot.Editor.StableDiffusion
{
    [FilePath(FilePath, FilePathAttribute.Location.PreferencesFolder)]
    public class DependenciesInstalled : ScriptableSingleton<DependenciesInstalled>
    {
        public const string FilePath = "Witchpot\\DependenciesInstalled.asset";

        private static void Save()
        {
            instance.Save(true);
        }

        public static void SetDoneInitialInstall()
        {
            instance._dependenciesInstalled = true;
            Save();
        }

        public static void ResetDoneInitialInstall()
        {
            instance._dependenciesInstalled = false;
            Save();
        }

#if WITCHPOT_DEVELOPMENT
        [MenuItem("Witchpot/Develop/Dependencies Installed Flag/Check (state of editor opened)", priority = 1)]
#endif
        public static void CheckInstalled()
        {
            Debug.Log($"DependenciesInstalled.Flag : {instance.Flag}");
        }

        [SerializeField]
        private bool _dependenciesInstalled = false;
        public bool Flag { get => _dependenciesInstalled; set => _dependenciesInstalled = value; }
    }
}
