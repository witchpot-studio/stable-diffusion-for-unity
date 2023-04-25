using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Witchpot.Runtime.StableDiffusion
{
    [Serializable, VolumeComponentMenu("Witchpot/PostProcess/Depth")]
    public class Depth : VolumeComponent, IPostProcessComponent
    {
        [Tooltip("Depth effect intensity.")]
        public ClampedFloatParameter weight = new ClampedFloatParameter(0f, 0f, 1f);

        public bool IsActive() => weight.value > 0f;

        public bool IsTileCompatible() => false;
    }
}
