using System.Linq;
using System.Threading;
using UnityEditor;
using UnityEngine;
using Witchpot.Runtime.StableDiffusion;


namespace Witchpot.Editor.StableDiffusion
{
    [CustomEditor(typeof(Img2Img))]
    public class Img2ImgEditor : UnityEditor.Editor
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

            Img2Img img2img = (Img2Img)target;

            serializedObject.Update();

            img2img.SelectedSampler = EditorGUILayout.Popup("Sampler", img2img.SelectedSampler, img2img.SamplersList);
            img2img.SelectedModel = EditorGUILayout.Popup("Model", img2img.SelectedModel, img2img.ModelsList);

            using (var changeCheck = new EditorGUI.ChangeCheckScope())
            {
                img2img.SelectedLoraModel = EditorGUILayout.Popup("Lora", img2img.SelectedLoraModel, new string[] { "None" }.Concat(img2img.LoraModelsList).ToArray());
                if (changeCheck.changed)
                {
                    if (img2img.SelectedLoraModel > 0)
                    {
                        img2img.Prompt += $"<lora:{img2img.LoraModelsList[img2img.SelectedLoraModel - 1]}:1>";
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
                    img2img.OnClickGenerateButton();
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
