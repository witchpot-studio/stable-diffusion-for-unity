using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.SearchService;
using UnityEngine;

namespace Witchpot.Runtime.Projection
{
    public class MaterialApplyer : MonoBehaviour
    {
        [Serializable]
        public struct MaterialBuffer
        {
            [SerializeField]
            private string _name;

            [SerializeField]
            private Renderer renderer;

            [SerializeField]
            private Material[] materials;

            public int MaterialCount => materials.Length;

            public MaterialBuffer(Renderer renderer)
            {
                _name = renderer.name;
                this.renderer = renderer;
                materials = renderer.sharedMaterials;
            }

            public void Restore()
            {
                if (renderer == null)
                {
                    Debug.LogWarning($"Renderer ({_name}) is missing. skip it.");
                    return;
                }

                renderer.sharedMaterials = materials;
            }
        }

        [SerializeField]
        private ImageProjector projector;

        [SerializeField]
        private Material projectionMaterialGlobal;

        [SerializeField]
        private Material projectionMaterial;

        [SerializeField]
        private List<MaterialBuffer> buffers = new List<MaterialBuffer>();

        private Dictionary<int, Material[]> projectionMaterialGlobalDic = new Dictionary<int, Material[]>();
        private Dictionary<int, Material[]> projectionMaterialDic = new Dictionary<int, Material[]>();

        private void SetMaterialArrayToDic(Dictionary<int, Material[]> dic, int count, Material material)
        {
            if (dic.ContainsKey(count)) { return; }

            var materials = new Material[count];
            for (int i = 0; i < count; i++)
            {
                materials[i] = material;
            }

            dic.Add(count, materials);
        }

        private void OnValidate()
        {
            if (projector == null)
            {
                projector = GetComponent<ImageProjector>();
            }
        }

        public void ApplayMaterial()
        {
            if (buffers.Count > 0)
            {
                Debug.LogWarning("Materials stored in buffer. Please do restore or clear first.");
                return;
            }

            var renderers = projector.GetRenderersInProjectionFrustum();

            foreach (var renderer in renderers) 
            {
                if (renderer == null) { continue; }

                var buffer = new MaterialBuffer(renderer);

                buffers.Add(buffer);

                Material[] materials;

                switch (projector.ProjectionType)
                {
                    case ImageProjector.EProjectionType.Global:
                    default:
                        SetMaterialArrayToDic(projectionMaterialGlobalDic, buffer.MaterialCount, projectionMaterialGlobal);
                        projectionMaterialGlobalDic.TryGetValue(buffer.MaterialCount, out materials);
                        break;

                    case ImageProjector.EProjectionType.TargetRenderers:
                        SetMaterialArrayToDic(projectionMaterialDic, buffer.MaterialCount, projectionMaterial);
                        projectionMaterialDic.TryGetValue(buffer.MaterialCount, out materials);
                        break;
                }

                renderer.sharedMaterials = materials;
            }
        }

        public void RestoreMaterials()
        {
            foreach (var buffer in buffers)
            {
                buffer.Restore();
            }

            buffers.Clear();
        }

        public void ClearBuffers()
        {
            buffers.Clear();
        }
    }
}
