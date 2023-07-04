using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Witchpot.Runtime.Projection
{
    public class TextureBaker : MonoBehaviour
    {
        [SerializeField]
        private ImageProjector projector;

        [SerializeField]
        private Vector2Int textureSize = new Vector2Int(1024, 1024);

        [SerializeField]
        private string targetDirectory;

        [SerializeField]
        private Material bakeMaterialGlobal;

        [SerializeField]
        private Material bakeMaterial;


        Queue<Renderer> targetRendererQueue = default;

        RenderTexture currentRenderTexture = default;
        Camera bakeCamera = default;
        RenderTexture renderTexture = default;
        Texture2D outputTexture = default;
        List<(Renderer renderer, int originalLayer)> originalLayerList = default;

        Material originalMaterial = default;

        int width = default;
        int height = default;
        string outputDirectory = default;

        const int NON_TARGET_LAYER_NUMBER = 0;
        const int TARGET_LAYER_NUMBER = 31;

        private void OnValidate()
        {
            if (projector == null)
            {
                projector = GetComponent<ImageProjector>();
            }

            if (string.IsNullOrEmpty(targetDirectory))
            {
                var scene = this.gameObject.scene;

                targetDirectory = $"Assets/BakedTexture/{scene.name}/{this.name}/";
            }
        }

        private Material GetBakeMaterial()
        {
            switch (projector.ProjectionType)
            {
                case ImageProjector.EProjectionType.Global:
                default:
                    return bakeMaterialGlobal;

                case ImageProjector.EProjectionType.TargetRenderers:
                    return bakeMaterial;
            }
        }

        public void Bake()
        {
            StartBake(textureSize.x, textureSize.y, targetDirectory);
        }

        private void StartBake(int width, int height, string outputDirectory)
        {
            this.width = width;
            this.height = height;
            
            try
            {
                bakeCamera = gameObject.AddComponent<Camera>();
                bakeCamera.aspect = (float)width / height;
                bakeCamera.clearFlags = CameraClearFlags.SolidColor;
                bakeCamera.backgroundColor = Color.black;
                bakeCamera.cullingMask = 1 << TARGET_LAYER_NUMBER;

                var renderersInFrustum = projector.GetRenderersInProjectionFrustum();

                if (!renderersInFrustum.Any())
                {
                    return;
                }

                this.outputDirectory = Path.GetDirectoryName(outputDirectory);
                if (!Directory.Exists(this.outputDirectory))
                {
                    Directory.CreateDirectory(this.outputDirectory);
                }

                originalLayerList = renderersInFrustum
                    .Select(renderer => (renderer, originalLayer: renderer.gameObject.layer))
                    .ToList();

                foreach (var renderer in renderersInFrustum)
                {
                    renderer.gameObject.layer = NON_TARGET_LAYER_NUMBER;
                }

                renderTexture = RenderTexture.GetTemporary(width, height, 0);
                outputTexture = new Texture2D(width, height);

                var currentRenderTexture = RenderTexture.active;

                targetRendererQueue = new Queue<Renderer>(renderersInFrustum);
                var target = targetRendererQueue.Peek();

                PrepareScreenShot(target);

                RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
            }
            catch
            {
                EndBake();
            }
        }

        private void EndBake()
        {
            RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;

            if (currentRenderTexture != null)
            {
                RenderTexture.active = currentRenderTexture;
            }

            if (renderTexture != null)
            {
                RenderTexture.active = null;
                RenderTexture.ReleaseTemporary(renderTexture);
            }

            if (bakeCamera != null)
            {
                var universalAdditionalCameraData = bakeCamera.GetUniversalAdditionalCameraData();
                if (universalAdditionalCameraData != null)
                {
                    UnityEngine.Object.DestroyImmediate(universalAdditionalCameraData);
                }

                UnityEngine.Object.DestroyImmediate(bakeCamera);
            }

            if (outputTexture != null)
            {
                UnityEngine.Object.DestroyImmediate(outputTexture);
            }

            if (originalLayerList != null)
            {
                foreach (var tuple in originalLayerList)
                {
                    tuple.renderer.gameObject.layer = tuple.originalLayer;
                }
            }
        }

        private void OnEndCameraRendering(ScriptableRenderContext context, Camera camera)
        {
            try
            {
                if (camera != bakeCamera)
                {
                    return;
                }

                var target = targetRendererQueue.Dequeue();

                TakeScreenShot(target);

                if (targetRendererQueue.TryPeek(out var nextTarget))
                {
                    PrepareScreenShot(nextTarget);
                }
                else
                {
                    EndBake();
                }
            }
            catch
            {
                EndBake();
            }
        }

        private void PrepareScreenShot(Renderer target)
        {
            target.gameObject.layer = TARGET_LAYER_NUMBER;
            var material = GetBakeMaterial();

            originalMaterial = target.sharedMaterial;
            target.sharedMaterial = material;

            bakeCamera.targetTexture = renderTexture;
            RenderTexture.active = renderTexture;

            switch (projector.ProjectionType)
            {
                case ImageProjector.EProjectionType.Global:
                default:
                    projector.ApplyProjectionInfo();
                    break;

                case ImageProjector.EProjectionType.TargetRenderers:
                    projector.ApplyProjectionInfoToMaterial(material);
                    break;
            }
        }

        private void TakeScreenShot(Renderer target)
        {
            target.gameObject.layer = NON_TARGET_LAYER_NUMBER;
            target.sharedMaterial = originalMaterial;

            var readRect = new Rect(0, 0, width, height);
            outputTexture.ReadPixels(readRect, 0, 0);
            outputTexture.Apply();

            var outputBytes = outputTexture.EncodeToPNG();

            var outputPath = $"{outputDirectory}/{target.name}-{GetTimestamp()}.png";

            File.WriteAllBytes(outputPath, outputBytes);


#if UNITY_EDITOR
            // TODO: "Assets/"ˆÈ‰º‚Ì‘Š‘ÎƒpƒX‚©‚Ç‚¤‚©‚ðŒ©‚é
            AssetDatabase.ImportAsset(outputPath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.Refresh();

            var asset = AssetDatabase.LoadAssetAtPath<Texture>(outputPath);
            EditorGUIUtility.PingObject(asset);
            Selection.activeObject = asset;

            Debug.Log($"Texture saved in {outputPath}", asset);
#else
            Debug.Log($"Texture saved in {outputPath}");
#endif
        }

        private string GetTimestamp()
        {
            var now = DateTime.Now;

            return now.ToString("yyyyMMddHHmmss");
        }
    }
}
