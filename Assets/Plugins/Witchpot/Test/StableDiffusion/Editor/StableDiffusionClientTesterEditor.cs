using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Witchpot.Runtime.StableDiffusion.Test;

namespace Witchpot.Editor.StableDiffusion.Test
{
    [CustomEditor(typeof(StableDiffusionClientTester))]
    public class StableDiffusionClientTesterEditor : StableDiffusionClientEditor
    {
        public override void OnInspectorGUI()
        {
            StableDiffusionClientTester component = (StableDiffusionClientTester)target;

            base.OnInspectorGUI();

            LayoutServerAccessButton(component, "Test", component.IsGenerating);
        }
    }
}
