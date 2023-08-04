﻿using System.Linq;
using UnityEditor;
using UnityEngine;
using Witchpot.Runtime.StableDiffusion;


namespace Witchpot.Editor.StableDiffusion
{
    [CustomEditor(typeof(Text2Img))]
    public class Text2ImgEditor : StableDiffusionClientEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            Text2Img component = (Text2Img)target;

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

            LayoutServerAccessButton(component, "Generate", component.IsTransmitting);

            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
