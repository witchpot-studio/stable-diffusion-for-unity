using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Witchpot.Runtime.StableDiffusion
{
    [Serializable]
    public class PipelineAssetLoader
    {
        [SerializeField]
        private RenderPipelineAsset m_PipelineAsset;

        private RenderPipelineAsset m_PreviousPipelineAsset;
        private bool m_overrodeQualitySettings;

        public bool SetPipeline()
        {
            if (m_PipelineAsset == null) { return false; }
            if (m_PreviousPipelineAsset != null) { ResetPipeline(); } 

            if (QualitySettings.renderPipeline != null && QualitySettings.renderPipeline != m_PipelineAsset)
            {
                m_PreviousPipelineAsset = QualitySettings.renderPipeline;
                QualitySettings.renderPipeline = m_PipelineAsset;
                m_overrodeQualitySettings = true;
            }
            else if (GraphicsSettings.renderPipelineAsset != m_PipelineAsset)
            {
                m_PreviousPipelineAsset = GraphicsSettings.renderPipelineAsset;
                GraphicsSettings.renderPipelineAsset = m_PipelineAsset;
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
