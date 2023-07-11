using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor.PackageManager;
using UnityEngine;
using Witchpot.Runtime.StableDiffusion;

public class StableDiffusionClientTester : MonoBehaviour
{
    [SerializeField]
    private List<StableDiffusionClientBase> m_ClientBaseList = new List<StableDiffusionClientBase>();

    [ContextMenu("Test")]
    private void Test()
    {
        TestAsync().Forget();
    }

    private async ValueTask TestAsync()
    {
        Debug.Log($"Start with {m_ClientBaseList.Count} items");

        foreach (var client in m_ClientBaseList)
        {
            Debug.Log($"{client.name} started");

            await client.GenerateAsync();

            Debug.Log($"{client.name} finished");
        }

        Debug.Log($"Finished");
    }
}
