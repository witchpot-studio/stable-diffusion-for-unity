using UnityEditor;

namespace Witchpot.Editor.StableDiffusion
{
    [CustomEditor(typeof(StableDiffusionWebUISettings))]
    public class StableDiffusionWebUISettingsEditor : StableDiffusionClientEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            StableDiffusionWebUISettings component = (StableDiffusionWebUISettings)target;

            LayoutServerAccessButton(component, "List Models");
        }
    }
}
