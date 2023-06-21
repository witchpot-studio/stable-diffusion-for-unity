using UnityEngine.Rendering;
using UnityEngine;
using UnityEngine.Rendering.Universal;


namespace Witchpot.Runtime.StableDiffusion
{
    public sealed class MainRendererFeature : ScriptableRendererFeature
    {
        private static readonly string Keyword = "TO_TEXTURE";

        public ScriptableRenderPassInput m_RenderPassInput = ScriptableRenderPassInput.Color;

        public Shader m_Shader;
        public int m_ShaderPassIndex = 0;

        private Material m_Material;
        private PostProcessPass m_RenderPass = null;


#if URP_14
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.cameraType != CameraType.Game) { return; }

            renderer.EnqueuePass(m_RenderPass);
        }

        public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
        {
            if (renderingData.cameraData.cameraType != CameraType.Game) { return; }

            m_RenderPass.ConfigureInput(m_RenderPassInput);

            m_RenderPass.SetTarget(renderer.cameraColorTargetHandle);
        }
#else
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.cameraType != CameraType.Game) { return; }

            m_RenderPass.ConfigureInput(m_RenderPassInput);

            m_RenderPass.SetTarget(renderer.cameraColorTarget);
            renderer.EnqueuePass(m_RenderPass);
        }
#endif

        public override void Create()
        {
            if (m_Shader == null)
            {
                //Debug.LogWarning("MainRendererFeature.Create canceled once because not ready");
                return;
            }

            m_Material = CoreUtils.CreateEngineMaterial(m_Shader);
            var index = m_ShaderPassIndex;

            m_RenderPass = new PostProcessPass(name, m_Material, index, RenderPassEvent.BeforeRenderingPostProcessing);

            //Debug.Log("MainRendererFeature.Create done.");
        }

        public void SetRenderToTexture()
        {
            if (m_Material == null) { return; }

            m_Material.EnableKeyword(Keyword);
        }

        public void SetRenderToScreen()
        {
            if (m_Material == null) { return; }

            m_Material.DisableKeyword(Keyword);
        }

        protected override void Dispose(bool disposing)
        {
            CoreUtils.Destroy(m_Material);

            m_Material = null;
        }
    }
}
