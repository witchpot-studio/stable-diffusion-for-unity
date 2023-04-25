using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


namespace Witchpot.Runtime.StableDiffusion
{
    public sealed class PostProcessPass : ScriptableRenderPass
    {
        private static readonly int keepFrameBuffer = Shader.PropertyToID("KeepFrameBuffer");
        private Material _depthMaterial;
        private RenderTargetIdentifier _target;
        private const string renderPostProcessingTag = "Render PostProcessing Effects";
        private static readonly ProfilingSampler profilingRenderPostProcessing = new ProfilingSampler(renderPostProcessingTag);

        public PostProcessPass(RenderPassEvent evt)
        {
            base.profilingSampler = new ProfilingSampler(nameof(PostProcessPass));
            renderPassEvent = evt;
            var shader = Shader.Find("Witchpot/PostProcess/Depth");
            _depthMaterial = CoreUtils.CreateEngineMaterial(shader);
        }

        public void Setup(RenderTargetIdentifier target)
        {
            _target = target;
            ConfigureInput(ScriptableRenderPassInput.Normal);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (!renderingData.cameraData.postProcessEnabled)
            {
                return;
            }

            var cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, profilingRenderPostProcessing))
            {
                if (TryGetVolume<Depth>(out var depth))
                {
                    RenderDepth(cmd, depth, ref renderingData.cameraData);
                }
            }
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        private bool TryGetVolume<T>(out T @out)
            where T : VolumeComponent, IPostProcessComponent
        {
            var stack = VolumeManager.instance.stack;
            @out = stack.GetComponent<T>();
            if (@out == null)
            {
                return false;
            }

            if (!@out.IsActive())
            {
                return false;
            }

            return true;
        }

        private void RenderDepth(CommandBuffer commandBuffer, Depth depth, ref CameraData cameraData)
        {
            int width = cameraData.cameraTargetDescriptor.width;
            int height = cameraData.cameraTargetDescriptor.height;

            var destination = keepFrameBuffer;
            _depthMaterial.SetFloat("_Weight", depth.weight.value);
            var viewToWorld = cameraData.camera.cameraToWorldMatrix;
            _depthMaterial.SetMatrix("_ViewToWorld", viewToWorld);

            commandBuffer.GetTemporaryRT(destination, width, height,
                0, FilterMode.Point, RenderTextureFormat.Default);
            commandBuffer.SetGlobalTexture("_MainTex", destination);
            commandBuffer.Blit(_target, destination);
            commandBuffer.Blit(destination, _target, _depthMaterial, 0);
            commandBuffer.ReleaseTemporaryRT(destination);
        }

        public void Cleanup()
        {
            CoreUtils.Destroy(_depthMaterial);
        }
    }
}
