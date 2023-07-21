using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor.PackageManager;
using UnityEngine;


namespace Witchpot.Runtime.StableDiffusion.Test
{
    public class StableDiffusionClientTester : MonoBehaviour, IStableDiffusionClient
    {
        [SerializeField]
        private List<StableDiffusionClientBase> m_ClientBaseList = new List<StableDiffusionClientBase>();

        private bool _generating = false;
        public bool IsGenerating => _generating;

        [ContextMenu("Test")]
        private void Test()
        {
            TestAsync().Forget();
        }

        public void OnClickServerAccessButton()
        {
            TestAsync().Forget();
        }

        private async ValueTask TestAsync()
        {
            _generating = true;

            try
            {
                Debug.Log($"Start with {m_ClientBaseList.Count} items");

                foreach (var client in m_ClientBaseList)
                {
                    Debug.Log($"{client.name} started");

                    await client.GenerateAsync();

                    Debug.Log($"{client.name} finished");
                }

                // HACK:use knowledge inside of the StableDiffusionClientBase
                ImagePorter.RefreshUnityEditor();

                Debug.Log($"All items finished");
            }
            finally
            {
                _generating = false;
            }
        }
    }
}