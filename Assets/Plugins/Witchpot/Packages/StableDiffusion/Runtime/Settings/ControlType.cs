using UnityEngine;


namespace Witchpot.Runtime.StableDiffusion
{
    [CreateAssetMenu(fileName = "ControlType", menuName = "Witchpot/ControlType")]
    public class ControlType : ScriptableObject
    {
        private static string[] _brankList = new string[0];

        [SerializeField]
        private StableDiffusionWebUISettings _stableDiffusionWebUISettings;

        [SerializeField]
        private ProccessedImageCapturer _imageCapturer;
        public ProccessedImageCapturer ImageCapturer => _imageCapturer;

        [SerializeField][HideInInspector]
        private int _selectedControlNetModelIndexIndex;

        public string SelectedControlNetModel => ControlNetModelList[SelectedControlNetModelIndex];
        public string[] ControlNetModelList => _stableDiffusionWebUISettings ? _stableDiffusionWebUISettings.ControlNetModelNames : _brankList;
        public int SelectedControlNetModelIndex
        {
            get { return _selectedControlNetModelIndexIndex; }
            set { _selectedControlNetModelIndexIndex = value; }
        }
    }
}
