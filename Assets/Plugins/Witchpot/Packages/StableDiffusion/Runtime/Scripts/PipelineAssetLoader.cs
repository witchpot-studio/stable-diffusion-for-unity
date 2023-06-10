using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Witchpot.Runtime.StableDiffusion
{
    public class PipelineAssetLoader
    {
        private RenderPipelineAsset m_PreviousPipelineAsset;
        private bool m_overrodeQualitySettings;

        public bool SetPipeline(RenderPipelineAsset asset)
        {
            if (asset == null) { return false; }
            if (m_PreviousPipelineAsset != null) { ResetPipeline(); } 

            if (QualitySettings.renderPipeline != null && QualitySettings.renderPipeline != asset)
            {
                m_PreviousPipelineAsset = QualitySettings.renderPipeline;
                QualitySettings.renderPipeline = asset;
                m_overrodeQualitySettings = true;
            }
            else if (GraphicsSettings.renderPipelineAsset != asset)
            {
                m_PreviousPipelineAsset = GraphicsSettings.renderPipelineAsset;
                GraphicsSettings.renderPipelineAsset = asset;
                m_overrodeQualitySettings = false;
            }

            return true;
        }

        public bool ResetPipeline()
        {
            if (m_PreviousPipelineAsset == null) { return false; }

            if (m_overrodeQualitySettings)
            {
                QualitySettings.renderPipeline = m_PreviousPipelineAsset;
            }
            else
            {
                GraphicsSettings.renderPipelineAsset = m_PreviousPipelineAsset;
            }

            return true;
        }
    }
}
