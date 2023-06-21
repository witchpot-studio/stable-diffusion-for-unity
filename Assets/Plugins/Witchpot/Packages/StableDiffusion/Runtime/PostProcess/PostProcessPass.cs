using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


namespace Witchpot.Runtime.StableDiffusion
{
    public sealed class PostProcessPass : ScriptableRenderPass
    {
        private static readonly int Buffer = Shader.PropertyToID("KeepFrameBuffer");

        private ProfilingSampler m_ProfilingSampler;
        private Material m_Material;
        private int m_PassIndex;

#if URP_14
        private RTHandle m_CameraColorTarget;
#else
        private RenderTargetIdentifier m_CameraColorTarget;
#endif

        public PostProcessPass(string name, Material material, int passIndex, RenderPassEvent passEvent)
        {
            m_ProfilingSampler = new ProfilingSampler(name);
            m_Material = material;
            m_PassIndex = passIndex;
            renderPassEvent = passEvent;
        }

#if URP_14
        public void SetTarget(RTHandle colorHandle)
        {
            m_CameraColorTarget = colorHandle;
        }
#else
        public void SetTarget(RenderTargetIdentifier targetID)
        {
            m_CameraColorTarget = targetID;
        }
#endif

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            ConfigureTarget(m_CameraColorTarget);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (m_Material == null) { return; }
            if (renderingData.cameraData.camera.cameraType != CameraType.Game) { return; }

            CommandBuffer cmd = CommandBufferPool.Get();

            using (new ProfilingScope(cmd, m_ProfilingSampler))
            {
                m_Material.SetMatrix("_WorldToView", renderingData.cameraData.camera.cameraToWorldMatrix.inverse);

#if URP_14
                Blitter.BlitCameraTexture(cmd, m_CameraColorTarget, m_CameraColorTarget, m_Material, m_PassIndex);
#else
                int width = renderingData.cameraData.cameraTargetDescriptor.width;
                int height = renderingData.cameraData.cameraTargetDescriptor.height;

                cmd.GetTemporaryRT(Buffer, width, height, 0, FilterMode.Point, RenderTextureFormat.Default);
                cmd.Blit(m_CameraColorTarget, Buffer);
                cmd.Blit(Buffer, m_CameraColorTarget, m_Material, m_PassIndex);
                cmd.ReleaseTemporaryRT(Buffer);
#endif
            }

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            CommandBufferPool.Release(cmd);
        }
    }
}
