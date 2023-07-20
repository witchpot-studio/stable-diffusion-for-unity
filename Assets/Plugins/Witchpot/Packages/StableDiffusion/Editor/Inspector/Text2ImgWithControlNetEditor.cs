using System.Linq;
using UnityEditor;
using UnityEngine;
using Witchpot.Runtime.StableDiffusion;


namespace Witchpot.Editor.StableDiffusion
{
    [CustomEditor(typeof(Text2ImgWithControlNet))]
    public class Text2ImgWithControlNetEditor : StableDiffusionClientEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            Text2ImgWithControlNet component = (Text2ImgWithControlNet)target;

            serializedObject.Update();

            component.SelectedSamplerIndex = EditorGUILayout.Popup("Sampler", component.SelectedSamplerIndex, component.SamplersList);
            component.SelectedModelIndex = EditorGUILayout.Popup("Model", component.SelectedModelIndex, component.ModelsList);
            
            int loraIndex = 0;
            var loraList = LoraModelListBase.Concat(component.LoraModelsList).ToArray();
            loraIndex = EditorGUILayout.Popup("Lora", loraIndex, loraList);

            if (loraIndex > 0)
            {
                component.Prompt += StableDiffusionWebUIClient.GetLoraString(loraList[loraIndex]);
            }

            // component.SelectedControlNetModelIndex = EditorGUILayout.Popup("ControlNetModel", component.SelectedControlNetModelIndex, component.ControlNetModelList);

            LayoutServerAccessButton(component, "Generate", component.IsTransmitting);

            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
