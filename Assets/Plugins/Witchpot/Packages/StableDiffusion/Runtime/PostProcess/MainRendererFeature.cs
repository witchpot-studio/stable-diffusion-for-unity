using UnityEngine.Rendering.Universal;


namespace Witchpot.Runtime.StableDiffusion
{
    public sealed class MainRendererFeature : ScriptableRendererFeature
    {
        private PostProcessPass _postProcessPass;

        public override void Create()
        {
            _postProcessPass = new PostProcessPass(RenderPassEvent.BeforeRenderingPostProcessing);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            _postProcessPass.Setup(renderer.cameraColorTarget);
            renderer.EnqueuePass(_postProcessPass);
        }

        protected override void Dispose(bool disposing)
        {
            _postProcessPass.Cleanup();
            base.Dispose(disposing);
        }
    }
}
