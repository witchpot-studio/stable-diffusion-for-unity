using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Witchpot.Runtime.StableDiffusion;

namespace Witchpot.Editor.StableDiffusion
{
    public class Preferences : SettingsProvider
    {
        private static string Path = "Preferences/StableDiffusion For Unity";

        private static GUILayoutOption m_Hight = GUILayout.Height(20);
        private static GUILayoutOption m_LineHight = GUILayout.Height(3);

        private static float m_Space = 10.0f;

        class Styles
        {
            public static GUIContent use = new GUIContent("Using StableDiffusion");
            public static GUIContent internal_ = new GUIContent("Internal Bat Path");
            public static GUIContent external = new GUIContent("External Bat Path");
        }

        // Register the SettingsProvider
        [SettingsProvider]
        public static SettingsProvider CreatePreferences()
        {
            if (WebUISingleton.IsAvailable)
            {
                var provider = new Preferences(Path, SettingsScope.User);

                // Automatically extract all keywords from the Styles.
                provider.keywords = GetSearchKeywordsFromGUIContentProperties<Styles>();
                return provider;
            }

            // Settings Asset doesn't exist yet; no need to display anything in the Settings window.
            return null;
        }

#if WITCHPOT_DEVELOPMENT
        [MenuItem("Witchpot/Develop/Open Preferences")]
#endif
        public static void Open()
        {
            SettingsService.OpenUserPreferences(Path);
        }        

        private SerializedObject m_CustomSettings;

        private SerializedProperty m_UsingStableDiffusion;
        private SerializedProperty m_ExternalPath;

        public Preferences(string path, SettingsScope scope = SettingsScope.User)
            : base(path, scope)
        {

        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            // This function is called when the user clicks on the MyCustom element in the Settings window.
        }

        private void Separater(string label)
        {
            GUILayout.Space(m_Space);
            using (new EditorGUILayout.HorizontalScope(m_Hight))
            {
                GUILayout.Space(3);
                GUILayout.Label(label, EditorStyles.boldLabel, GUILayout.Width(160));

                using (new EditorGUILayout.VerticalScope(m_Hight))
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.Box(string.Empty, GUILayout.ExpandWidth(true), m_LineHight); // ----------
                    GUILayout.FlexibleSpace();
                }
            }
        }

        public override void OnGUI(string searchContext)
        {
            m_CustomSettings = WebUISingleton.GetSerialized();

            m_UsingStableDiffusion = m_CustomSettings.FindProperty("_usingStableDiffusion");
            m_ExternalPath = m_CustomSettings.FindProperty("_externalPath");

            //m_CustomSettings.Update();

            // Use IMGUI to display UI:
            EditorGUILayout.PropertyField(m_UsingStableDiffusion, Styles.use);

            if (WebUISingleton.Status.ServerReady)
            {
                if (GUILayout.Button("Stop Server"))
                {
                    WebUISingleton.Stop();
                }

            }
            else if (WebUISingleton.Status.ServerStarted)
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    GUILayout.Button("Server Starting ... ");
                }
            }
            else
            {
                if (GUILayout.Button("Start Server"))
                {
                    WebUISingleton.Start();
                }
            }

            Separater("Internal StableDiffusion");

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.TextField(Styles.internal_, DependenciesInstaller.instance.DestinationBatPath);
            }

            using (new EditorGUILayout.HorizontalScope(m_Hight))
            {
                if (GUILayout.Button("(Re)Install"))
                {
                    DependenciesInstaller.Install();
                }

                if (GUILayout.Button("Uninstall"))
                {
                    DependenciesInstaller.Uninstall();
                }

                if (GUILayout.Button("Open Dir"))
                {
                    DependenciesInstaller.Open();
                }
            }

            Separater("External StableDiffusion");

            EditorGUILayout.PropertyField(m_ExternalPath, Styles.external);

            m_CustomSettings.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
