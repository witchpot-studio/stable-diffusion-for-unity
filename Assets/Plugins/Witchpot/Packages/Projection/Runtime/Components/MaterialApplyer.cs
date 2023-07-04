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

        private Material[] projectionMaterialGlobalArray;
        private Material[] projectionMaterialArray;

        private void OnValidate()
        {
            if (projector == null)
            {
                projector = GetComponent<ImageProjector>();
            }
        }

        public void ApplayMaterial()
        {
            if (projectionMaterialGlobalArray == null || projectionMaterialGlobalArray.Length != 1 )
            {
                projectionMaterialGlobalArray = new Material[1];
            }

            if (projectionMaterialArray == null || projectionMaterialArray.Length != 1)
            {
                projectionMaterialArray = new Material[1];
            }

            projectionMaterialGlobalArray[0] = projectionMaterialGlobal;
            projectionMaterialArray[0] = projectionMaterial;

            if (buffers.Count > 0)
            {
                Debug.LogWarning("Materials stored in buffer. Please do restore or clear first.");
                return;
            }

            var renderers = projector.GetRenderersInProjectionFrustum();

            foreach (var renderer in renderers) 
            {
                if (renderer == null) { continue; }

                buffers.Add(new MaterialBuffer(renderer));

                switch (projector.ProjectionType)
                {
                    case ImageProjector.EProjectionType.Global:
                    default:
                        renderer.sharedMaterials = projectionMaterialGlobalArray;
                        break;

                    case ImageProjector.EProjectionType.TargetRenderers:
                        renderer.sharedMaterials = projectionMaterialArray;
                        break;
                }
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
