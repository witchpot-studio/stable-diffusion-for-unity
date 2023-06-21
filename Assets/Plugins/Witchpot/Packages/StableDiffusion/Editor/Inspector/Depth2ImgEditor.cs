using System.Linq;
using System.Threading;
using UnityEditor;
using UnityEngine;
using Witchpot.Runtime.StableDiffusion;


namespace Witchpot.Editor.StableDiffusion
{
    [CustomEditor(typeof(Depth2Img))]
    public class Depth2ImgEditor : UnityEditor.Editor
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

            Depth2Img depth2img = (Depth2Img)target;

            serializedObject.Update();

            depth2img.SelectedSampler = EditorGUILayout.Popup("Sampler", depth2img.SelectedSampler, depth2img.SamplersList);
            depth2img.SelectedModel = EditorGUILayout.Popup("Model", depth2img.SelectedModel, depth2img.ModelsList);

            using (var changeCheck = new EditorGUI.ChangeCheckScope())
            {
                depth2img.SelectedLoraModel = EditorGUILayout.Popup("Lora", depth2img.SelectedLoraModel, new string[] { "None" }.Concat(depth2img.LoraModelsList).ToArray());
                if (changeCheck.changed)
                {
                    if (depth2img.SelectedLoraModel > 0)
                    {
                        depth2img.Prompt += $"<lora:{depth2img.LoraModelsList[depth2img.SelectedLoraModel - 1]}:1>";
                    }
                }
            }

            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }

            serializedObject.ApplyModifiedProperties();

            if (WebUISingleton.Status.ServerReady)
            {
                if (GUILayout.Button("Generate"))
                {
                    depth2img.OnClickGenerateButton().Forget();
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
