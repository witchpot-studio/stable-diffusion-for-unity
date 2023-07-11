using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Witchpot.Runtime.Projection;


namespace Witchpot.Editor.Projection
{
    [CustomEditor(typeof(CameraImageProjector))]
    public class CameraImageProjectorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            CameraImageProjector component = (CameraImageProjector)target;

            base.OnInspectorGUI();

            using (new EditorGUI.DisabledScope(component.AutoProjection == true))
            {
                if (GUILayout.Button("Project"))
                {
                    component.ApplyProjectionInfo();

                    UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
                }
            }
        }
    }
}