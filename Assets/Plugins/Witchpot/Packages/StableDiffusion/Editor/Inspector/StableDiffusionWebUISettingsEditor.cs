using System.Threading;
using UnityEditor;
using UnityEngine;
using Witchpot.Editor.StableDiffusion;
using Witchpot.Runtime.StableDiffusion;

namespace Witchpot
{
    [CustomEditor(typeof(StableDiffusionWebUISettings))]
    public class StableDiffusionWebUISettingsEditor : UnityEditor.Editor
    {
        private SynchronizationContext _context;

        private void Awake()
        {
            _context = SynchronizationContext.Current;

            WebUISingleton.Status.Changed -= OnWebUIStatusChanged;
            WebUISingleton.Status.Changed += OnWebUIStatusChanged;
        }

        private void OnWebUIStatusChanged(object sender, WebUISingleton.IWebUIStatus.IArgs args)
        {
            if (_context != null)
            {
                _context.Post(_ => { Repaint(); }, null);
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            StableDiffusionWebUISettings component = (StableDiffusionWebUISettings)target;

            if (WebUISingleton.Status.ServerReady)
            {
                if (GUILayout.Button("List Models"))
                {
                    component.ListModels().Forget();
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
        }
    }
}
