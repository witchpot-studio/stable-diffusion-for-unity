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
            public static GUIContent stop_server = new GUIContent("Stop Server");
            public static GUIContent server_starting = new GUIContent("Server Starting ... ");
            public static GUIContent start_server = new GUIContent("Start Server");

            public static GUIContent internal_sd = new GUIContent("Internal StableDiffusion");
            public static GUIContent internal_path = new GUIContent("Internal Bat Path");
            public static GUIContent install = new GUIContent("(Re)Install");
            public static GUIContent uninstall = new GUIContent("Uninstall");
            public static GUIContent open_dir = new GUIContent("Open Dir");           
            
            public static GUIContent external_sd = new GUIContent("External StableDiffusion");
            public static GUIContent external_path = new GUIContent("External Bat Path");

            public static GUIContent infomation = new GUIContent("Infomation");
            public static GUIContent version = new GUIContent("Version");
            public static GUIContent version_number = new GUIContent("1.3.0"); // HACK: The version is hard corded
            public static GUIContent document = new GUIContent("Open Document");
            public static GUIContent discord_jp = new GUIContent("Discord JP");
            public static GUIContent discord_en = new GUIContent("Discord EN");
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

//#if WITCHPOT_DEVELOPMENT
        [MenuItem("Witchpot/Utility/Open Preferences", priority = 100)]
//#endif
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

        private void Separater(GUIContent label)
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
                if (GUILayout.Button(Styles.stop_server))
                {
                    WebUISingleton.Stop();
                }

            }
            else if (WebUISingleton.Status.ServerStarted)
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    GUILayout.Button(Styles.server_starting);
                }
            }
            else
            {
                if (GUILayout.Button(Styles.start_server))
                {
                    WebUISingleton.Start();
                }
            }

            Separater(Styles.internal_sd);

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.TextField(Styles.internal_path, DependenciesInstaller.instance.DestinationBatPath);
            }

            using (new EditorGUILayout.HorizontalScope(m_Hight))
            {
                if (GUILayout.Button(Styles.install))
                {
                    DependenciesInstaller.Install();
                }

                if (GUILayout.Button(Styles.uninstall))
                {
                    DependenciesInstaller.Uninstall();
                }

                if (GUILayout.Button(Styles.open_dir))
                {
                    DependenciesInstaller.Open();
                }
            }

            Separater(Styles.external_sd);

            EditorGUILayout.PropertyField(m_ExternalPath, Styles.external_path);

            m_CustomSettings.ApplyModifiedPropertiesWithoutUndo();

            Separater(Styles.infomation);

            EditorGUILayout.LabelField(Styles.version, Styles.version_number);

            using (new EditorGUILayout.HorizontalScope(m_Hight))
            {
                if (GUILayout.Button(Styles.document))
                {
                    Application.OpenURL(EditorPaths.WITCHPOT_DOCUMENT_URL);
                }

                if (GUILayout.Button(Styles.discord_jp))
                {
                    Application.OpenURL(EditorPaths.WITCHPOT_DISCORD_JP_URL);
                }

                if (GUILayout.Button(Styles.discord_en))
                {
                    Application.OpenURL(EditorPaths.WITCHPOT_DISCORD_EN_URL);
                }
            }
        }
    }
}
