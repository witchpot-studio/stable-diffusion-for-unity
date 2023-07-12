using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor.SearchService;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Witchpot.Runtime.Projection
{
    [ExecuteAlways]
    public class CameraImageProjector : MonoBehaviour
    {
        private readonly static int vpMatrixShaderGlobalNameID = Shader.PropertyToID("_ProjectorMatrixVPGlobal");
        private readonly static int textureShaderGlobalNameID = Shader.PropertyToID("_ProjectionMapGlobal");
        private readonly static int positionShaderGlobalNameID = Shader.PropertyToID("_ProjectorPositionGlobal");

        private readonly static int vpMatrixShaderNameID = Shader.PropertyToID("_ProjectorMatrixVP");
        private readonly static int textureShaderNameID = Shader.PropertyToID("_ProjectionMap");
        private readonly static int positionShaderNameID = Shader.PropertyToID("_ProjectorPosition");

        private readonly static Renderer[] blankRenderers = new Renderer[0];

        public enum EProjectionType
        {
            Global,
            TargetRenderers,
        }

        [SerializeField]
        private Texture textureProjection;

        [SerializeField]
        private Camera cameraProjection;

        [SerializeField]
        private EProjectionType projectionType;
        public EProjectionType ProjectionType => projectionType;

        [SerializeField]
        private List<Renderer> renderers;
        public IReadOnlyList<Renderer> TargetRenderers => renderers;

        [SerializeField]
        private bool drawGizmosProjectionTarget = true;

        [SerializeField]
        private bool autoProjection = false;
        public bool AutoProjection => autoProjection;

        public bool IsValid => textureProjection != null && cameraProjection != null;

        private void Update()
        {
            if (autoProjection)
            {
                ApplyProjectionInfo();
            }
        }

        private ProjectionInfo GetProjectionInfo()
        {
            return new ProjectionInfo(transform, cameraProjection);
        }

        private void ApplyProjectionInfoToGlobal(ProjectionInfo info)
        {
            Shader.SetGlobalTexture(textureShaderGlobalNameID, textureProjection);
            Shader.SetGlobalMatrix(vpMatrixShaderGlobalNameID, info.GpuProjectionMatrix * info.ViewMatrix);
            Shader.SetGlobalVector(positionShaderGlobalNameID, info.ProjectorPosition);
        }

        private void ApplyProjectionInfoToMaterial(Material material, ProjectionInfo info)
        {
            if (material.HasProperty(textureShaderNameID))
            {
                material.SetTexture(textureShaderNameID, textureProjection);
            }

            material.SetMatrix(vpMatrixShaderNameID, info.GpuProjectionMatrix * info.ViewMatrix);

            if (material.HasProperty(positionShaderNameID))
            {
                material.SetVector(positionShaderNameID, info.ProjectorPosition);
            }
        }

        private void ApplyProjectionInfoToRenderers(ProjectionInfo info)
        {
            var materials = renderers
                .Where(renderer => renderer != null)
                .SelectMany(renderer => renderer.sharedMaterials)
                .Distinct();

            foreach (var material in materials)
            {
                ApplyProjectionInfoToMaterial(material, info);
            }
        }

        public void ApplyProjectionInfo()
        {
            if (!IsValid) { return; }

            var info = GetProjectionInfo();

            switch (projectionType)
            {
                case EProjectionType.Global:
                default:
                    ApplyProjectionInfoToGlobal(info);
                    break;

                case EProjectionType.TargetRenderers:
                    ApplyProjectionInfoToRenderers(info);
                    break;
            }
        }

        public void ApplyProjectionInfoToMaterial(Material material)
        {
            if (!IsValid) { return; }

            var info = GetProjectionInfo();

            ApplyProjectionInfoToMaterial(material, info);
        }

        public IEnumerable<Renderer> GetRenderersInProjectionFrustum()
        {
            if (cameraProjection == null) { return blankRenderers; }

            var info = GetProjectionInfo();

            IReadOnlyList<Renderer> renderers;

            switch (ProjectionType)
            {
                case EProjectionType.Global:
                default:
                    renderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);
                    break;

                case EProjectionType.TargetRenderers:
                    renderers = TargetRenderers;
                    break;
            }

            var planes = GeometryUtility.CalculateFrustumPlanes(info.WorldToProjectionMatrix);

            return renderers
                .Where(renderer =>
                {
                    return GeometryUtility.TestPlanesAABB(planes, renderer.bounds);
                });
        }


#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            DrawProjecrtionTarget();
        }

        private void DrawProjecrtionTarget()
        {
            if (!drawGizmosProjectionTarget) { return; }

            var buffer = Gizmos.color;

            try
            {
                Gizmos.color = Color.red;

                var renderers = GetRenderersInProjectionFrustum();

                foreach (var renderer in renderers)
                {
                    Gizmos.DrawLine(transform.position, renderer.bounds.center);
                    Gizmos.DrawWireCube(renderer.bounds.center, renderer.bounds.size);
                }
            }
            finally
            {
                Gizmos.color = buffer;
            }
        }
#endif
    }
}
