using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Witchpot.Runtime.StableDiffusion
{
    public class VolumeChanger : MonoBehaviour
    {
        private struct VolumeAndStatus
        {
            public Volume volume;
            public bool enabled;

            public VolumeAndStatus(Volume v)
            {
                volume = v;
                enabled = volume.enabled;
            }

            public void Restore()
            {
                volume.enabled = enabled;
            }
        }

        [SerializeField]
        private Volume _volume;

        private bool _volumeChanged = false;
        private List<VolumeAndStatus> _volumeInScene = new List<VolumeAndStatus>();

        public void SetVolumeStatus()
        {
            if (_volumeChanged) { ResetVolumeStatus(); }

            var array = GameObject.FindObjectsByType<Volume>(FindObjectsSortMode.None);

            foreach (var volume in array)
            {
                if (volume == _volume) { continue; }

                _volumeInScene.Add(new VolumeAndStatus(volume));

                volume.enabled = false;
            }

            _volumeChanged = true;
        }

        public void ResetVolumeStatus()
        {
            if (!_volumeChanged) { return; }

            if (_volumeInScene.Count > 0)
            {
                foreach (var volume in _volumeInScene)
                {
                    volume.Restore();
                }

                _volumeInScene.Clear();
            }

            _volumeChanged = false;
        }
    }
}
