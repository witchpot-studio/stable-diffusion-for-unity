using System.Threading;
using UnityEditor;
using UnityEngine;
using Witchpot.Runtime.StableDiffusion;


namespace Witchpot.Editor.StableDiffusion
{
    public abstract class StableDiffusionClientEditor : UnityEditor.Editor
    {
        private static string[] _loraModelListBase = new string[] { "Select to add prompt" };
        protected static string[] LoraModelListBase => _loraModelListBase;

        private static SynchronizationContext _context;
        // protected static SynchronizationContext Context => _context;

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

        protected void LayoutServerAccessButton(IStableDiffusionClient client, string label, bool disabled = false)
        {
            if (WebUISingleton.Status.ServerReady)
            {
                using (new EditorGUI.DisabledScope(disabled))
                {
                    if (GUILayout.Button(label))
                    {
                        client.OnClickServerAccessButton();
                    }
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
                using (new EditorGUI.DisabledScope(disabled))
                {
                    if (GUILayout.Button("Start Server"))
                    {
                        WebUISingleton.Start();
                    }
                }
            }
        }
    }
}
