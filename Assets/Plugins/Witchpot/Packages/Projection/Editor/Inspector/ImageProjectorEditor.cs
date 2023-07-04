using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Witchpot.Runtime.Projection;


namespace Witchpot.Editor.Projection
{
    [CustomEditor(typeof(ImageProjector))]
    public class ImageProjectorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            ImageProjector projector = (ImageProjector)target;

            base.OnInspectorGUI();

            if (GUILayout.Button("Project"))
            {
                projector.ApplyProjectionInfo();

                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            }
        }
    }
}