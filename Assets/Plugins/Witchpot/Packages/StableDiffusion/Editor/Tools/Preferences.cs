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

        private class Styles
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
            public static GUIContent version_number = new GUIContent("1.4.0"); // HACK: The version is hard corded
            public static GUIContent document = new GUIContent("Open Document");
            public static GUIContent discord_jp = new GUIContent("Discord JP");
            public static GUIContent discord_en = new GUIContent("Discord EN");
        }

#if WITCHPOT_DEVELOPMENT
        private class StylesDevelopment
        {
            public static GUIContent development = new GUIContent("Development");
            public static GUIContent installerAssetFilePath = new GUIContent("Installer Settings Asset File Path");
            public static GUIContent zipRootType = new GUIContent("Zip Root Type");
            public static GUIContent zipRelationalPath = new GUIContent("Zip Relational Path");
            public static GUIContent zipedRootFolderName = new GUIContent("Ziped Root Folder Name");            
            public static GUIContent zipedBatPath = new GUIContent("Ziped Bat Path");
            public static GUIContent zipedPythonExePath = new GUIContent("Ziped Python Exe Path");
            public static GUIContent webuiExpectedVersion = new GUIContent("WebUI Expected Version");
            public static GUIContent webuiControlNetExpectedVersion = new GUIContent("ControlNet Expected Version");
            public static GUIContent destinationRootType = new GUIContent("Destination Root Type");
            public static GUIContent destinationRelationalPath = new GUIContent("Destination Relational Path");
            public static GUIContent destinationBatPath = new GUIContent("Destination Bat Path");
            public static GUIContent save = new GUIContent("Save into asset");
        }
#endif

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
            var widthBUffer = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 200;

            try
            {
                var webUI = WebUISingleton.GetSerialized();

                var usingStableDiffusion = webUI.FindProperty("_usingStableDiffusion");
                var externalPath = webUI.FindProperty("_externalPath");

                //m_CustomSettings.Update();

                // Use IMGUI to display UI:
                EditorGUILayout.PropertyField(usingStableDiffusion, Styles.use);

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
                    EditorGUILayout.TextField(Styles.internal_path, DependenciesInstaller.DestinationBatPath);
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

                EditorGUILayout.PropertyField(externalPath, Styles.external_path);

                webUI.ApplyModifiedPropertiesWithoutUndo();

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


#if WITCHPOT_DEVELOPMENT
                Separater(StylesDevelopment.development);

                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.TextField(StylesDevelopment.installerAssetFilePath, DependenciesInstaller.FilePath);
                }

                var dependencies = DependenciesInstaller.GetSerialized();

                dependencies.Update();

                var zipRootType = dependencies.FindProperty("_zipRootType");
                EditorGUILayout.PropertyField(zipRootType, StylesDevelopment.zipRootType);

                var zipRelationalPath = dependencies.FindProperty("_zipRelationalPath");                
                EditorGUILayout.PropertyField(zipRelationalPath, StylesDevelopment.zipRelationalPath);

                var zipedRootFolderName = dependencies.FindProperty("_zipedRootFolderName");
                EditorGUILayout.PropertyField(zipedRootFolderName, StylesDevelopment.zipedRootFolderName);

                var zipedBatPath = dependencies.FindProperty("_zipedBatPath");
                EditorGUILayout.PropertyField(zipedBatPath, StylesDevelopment.zipedBatPath);

                var zipedPythonExePath = dependencies.FindProperty("_zipedPythonExePath");
                EditorGUILayout.PropertyField(zipedPythonExePath, StylesDevelopment.zipedPythonExePath);

                var webuiExpectedVersion = dependencies.FindProperty("_webuiExpectedVersion");
                EditorGUILayout.PropertyField(webuiExpectedVersion, StylesDevelopment.webuiExpectedVersion);

                var webuiControlNetExpectedVersion = dependencies.FindProperty("_webuiControlNetExpectedVersion");
                EditorGUILayout.PropertyField(webuiControlNetExpectedVersion, StylesDevelopment.webuiControlNetExpectedVersion);

                var destinationRootType = dependencies.FindProperty("_destinationRootType");
                EditorGUILayout.PropertyField(destinationRootType, StylesDevelopment.destinationRootType);
                
                var destinationRelationalPath = dependencies.FindProperty("_destinationRelationalPath");
                EditorGUILayout.PropertyField(destinationRelationalPath, StylesDevelopment.destinationRelationalPath);

                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.TextField(StylesDevelopment.destinationBatPath, DependenciesInstaller.DestinationBatPath);
                }

                if (dependencies.ApplyModifiedProperties())
                {
                    DependenciesInstaller.SetModified();
                }

                using (new EditorGUI.DisabledScope(!DependenciesInstaller.IsModified))
                {
                    var colorBuffer = GUI.color;

                    if (DependenciesInstaller.IsModified)
                    {
                        GUI.color = Color.red;
                    }

                    if (GUILayout.Button(StylesDevelopment.save))
                    {
                        DependenciesInstaller.Save();
                    }

                    if (DependenciesInstaller.IsModified)
                    {
                        GUI.color = colorBuffer;
                    }
                }
#endif
            }
            finally
            {
                EditorGUIUtility.labelWidth = widthBUffer;
            }
        }
    }
}
