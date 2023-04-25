using System.Linq;
using System.Threading;
using UnityEditor;
using UnityEngine;
using Witchpot.Runtime.StableDiffusion;


namespace Witchpot.Editor.StableDiffusion
{
    [CustomEditor(typeof(Text2Img))]
    public class Text2ImgEditor : UnityEditor.Editor
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

            Text2Img text2img = (Text2Img)target;

            serializedObject.Update();

            text2img.SelectedSampler = EditorGUILayout.Popup("Sampler", text2img.SelectedSampler, text2img.SamplersList);
            text2img.SelectedModel = EditorGUILayout.Popup("Model", text2img.SelectedModel, text2img.ModelsList);

            using (var changeCheck = new EditorGUI.ChangeCheckScope())
            {
                text2img.SelectedLoraModel = EditorGUILayout.Popup("Lora", text2img.SelectedLoraModel, new string[] { "None" }.Concat(text2img.LoraModelsList).ToArray());
                if (changeCheck.changed)
                {
                    if (text2img.SelectedLoraModel > 0)
                    {
                        text2img.Prompt += $"<lora:{text2img.LoraModelsList[text2img.SelectedLoraModel-1]}:1>";
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
                    text2img.OnClickGenerateButton();
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
