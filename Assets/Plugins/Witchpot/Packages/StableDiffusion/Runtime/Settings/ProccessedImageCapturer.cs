using System;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;


namespace Witchpot.Runtime.StableDiffusion
{
    public static class ImageCapturer
    {
        public enum CaptureMode
        {
            ColorImage,
            FunctionalImage,
        }

        public static Texture2D CreateCameraViewImage(Camera camera, int width, int height, CaptureMode mode = CaptureMode.ColorImage)
        {
            RenderTexture render;
            Texture2D texture;

            switch (mode)
            {
                case CaptureMode.ColorImage:
                default:
                    render = new RenderTexture(width, height, 24);
                    render.antiAliasing = 8;

                    texture = new Texture2D(width, height, TextureFormat.RGB24, false);

                    break;

                case CaptureMode.FunctionalImage:
                    render = new RenderTexture(width, height, 24, UnityEngine.Experimental.Rendering.GraphicsFormat.R32G32B32A32_SFloat);
                    render.antiAliasing = 8;

                    texture = new Texture2D(width, height, TextureFormat.RGB24, false);
                    break;
            }

            return CreateCameraViewImage(camera, render, texture);
        }

        public static Texture2D CreateCameraViewImage(Camera camera, RenderTexture render, Texture2D texture)
        {
            var bufferTT = camera.targetTexture;
            var bufferRT = RenderTexture.active;

            try
            {
                camera.targetTexture = render;
                camera.Render();

                RenderTexture.active = render;
                texture.ReadPixels(new Rect(0, 0, render.width, render.height), 0, 0);
                texture.Apply();

                return texture;
            }
            finally
            {
                camera.targetTexture = bufferTT;
                RenderTexture.active = bufferRT;

                RenderTexture.DestroyImmediate(render);
            }
        }
    }

    [CreateAssetMenu(fileName = "PreProccessedImageCapture", menuName = "Witchpot/PreProccessedImageCapture")]
    public class ProccessedImageCapturer : ScriptableObject
    {
        private const string GameMenuItem = "Window/General/Game";

        [SerializeField]
        private RenderPipelineAsset _pipelineAsset;

        [SerializeField]
        private MainRendererFeature _mainRendererFeature;

        private PipelineAssetLoader _assetLoader = new PipelineAssetLoader();

        public async ValueTask<Texture2D> CreatePreProcessedImageAsync(Camera camera, int width, int height)
        {
            try
            {
                if (_assetLoader.SetPipeline(_pipelineAsset))
                {
                    EditorApplication.ExecuteMenuItem(GameMenuItem);

                    await Task.Delay(1000);

                    return RenderPreProcessedImage(camera, width, height);
                }
                else
                {
                    throw new ArgumentException("Failed to set pipeline asset");
                }
            }
            finally
            {
                _assetLoader.ResetPipeline();
            }
        }

        private Texture2D RenderPreProcessedImage(Camera camera, int width, int height)
        {
            try
            {
                _mainRendererFeature.SetRenderToTexture();

                return ImageCapturer.CreateCameraViewImage(camera, width, height, ImageCapturer.CaptureMode.FunctionalImage);
            }
            finally
            {
                _mainRendererFeature.SetRenderToScreen();
            }
        }
    }
}
