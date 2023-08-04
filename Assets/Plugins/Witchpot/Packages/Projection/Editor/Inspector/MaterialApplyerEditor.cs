using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Witchpot.Runtime.Projection;


namespace Witchpot.Editor.Projection
{
    [CustomEditor(typeof(MaterialApplyer))]
    public class MaterialApplyerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            MaterialApplyer applyer = (MaterialApplyer)target;

            base.OnInspectorGUI();

            if (GUILayout.Button("Apply"))
            {
                applyer.ApplayMaterial();
                EditorUtility.SetDirty(applyer);
            }

            if (GUILayout.Button("Restore"))
            {
                applyer.RestoreMaterials();
                EditorUtility.SetDirty(applyer);
            }

            if (GUILayout.Button("Clear"))
            {
                applyer.ClearBuffers();
                EditorUtility.SetDirty(applyer);
            }
        }
    }
}