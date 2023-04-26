using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Witchpot.Editor.StableDiffusion
{
    [FilePath("Witchpot/DependenciesInstalled.asset", FilePathAttribute.Location.PreferencesFolder)]
    public class DependenciesInstalled : ScriptableSingleton<DependenciesInstalled>
    {
        private static void Save()
        {
            instance.Save(true);
        }

#if WITCHPOT_DEVELOPMENT
        [MenuItem("Witchpot/Develop/Dependencies Installed Flag/Set", priority = 1)]
#endif
        public static void SetInstalled()
        {
            instance._dependenciesInstalled = true;
            Save();
        }

#if WITCHPOT_DEVELOPMENT
        [MenuItem("Witchpot/Develop/Dependencies Installed Flag/Reset", priority = 1)]
#endif
        public static void ResetInstalled()
        {
            instance._dependenciesInstalled = false;
            Save();
        }

#if WITCHPOT_DEVELOPMENT
        [MenuItem("Witchpot/Develop/Dependencies Installed Flag/Check (state of editor opened)", priority = 1)]
#endif
        public static void CheckInstalled()
        {
            Debug.Log($"DependenciesInstalled.Flag : {instance._dependenciesInstalled}");
        }

        [SerializeField]
        private bool _dependenciesInstalled = false;

        public static bool Flag => instance._dependenciesInstalled;
    }
}
