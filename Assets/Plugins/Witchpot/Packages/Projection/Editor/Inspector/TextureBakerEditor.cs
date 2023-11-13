using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Witchpot.Runtime.Projection;


namespace Witchpot.Editor.Projection
{
    [CustomEditor(typeof(TextureBaker))]
    public class TextureBakerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            TextureBaker baker = (TextureBaker)target;

            base.OnInspectorGUI();

            if (GUILayout.Button("Bake"))
            {
                baker.Bake();
            }
        }
    }
}