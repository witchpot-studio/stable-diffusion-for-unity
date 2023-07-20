using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Witchpot.Runtime.StableDiffusion;

namespace Witchpot.Editor.StableDiffusion
{
    [CustomEditor(typeof(ControlType))]
    public class ControlTypeEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            ControlType component = (ControlType)target;

            serializedObject.Update();

            component.SelectedControlNetModelIndex = EditorGUILayout.Popup("ControlNetModel", component.SelectedControlNetModelIndex, component.ControlNetModelList);

            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
