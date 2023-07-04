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
            }

            if (GUILayout.Button("Restore"))
            {
                applyer.RestoreMaterials();
            }

            if (GUILayout.Button("Clear"))
            {
                applyer.ClearBuffers();
            }
        }
    }
}