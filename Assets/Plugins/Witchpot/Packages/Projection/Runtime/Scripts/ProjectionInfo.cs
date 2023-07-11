
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Witchpot.Runtime.Projection
{
    [Serializable]
    public struct ProjectorSettings
    {
        [SerializeField]
        private bool orthographic;
        public bool Orthographic => orthographic;

        [SerializeField]
        private float orthographicSize;
        public float OrthographicSize => orthographicSize;

        [SerializeField]
        private float aspect;
        public float Aspect => aspect;

        [SerializeField]
        private float nearClipPlane;
        public float NearClipPlane => nearClipPlane;

        [SerializeField]
        private float farClipPlane;
        public float FarClipPlane => farClipPlane;

        [SerializeField]
        private float fieldOfView;
        public float FieldOfView => fieldOfView;

        public ProjectorSettings(Camera camera)
        {
            orthographic = camera.orthographic;
            orthographicSize = camera.orthographicSize;
            aspect = camera.aspect;
            nearClipPlane = camera.nearClipPlane;
            farClipPlane = camera.farClipPlane;
            fieldOfView = camera.fieldOfView;
        }

        public void DrawGizmo(Transform transform)
        {
            var buffer = Gizmos.color;

            try
            {
                Gizmos.color = Color.gray;

                var gizmosMatrix = Gizmos.matrix;

                Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);

                if (Orthographic)
                {
                    var boxLength = FarClipPlane - NearClipPlane;
                    var boxCenter = Vector3.forward * (boxLength / 2.0f + NearClipPlane);
                    Gizmos.DrawWireCube(boxCenter, new Vector3(OrthographicSize * Aspect * 2.0f, OrthographicSize * 2, boxLength));
                }
                else
                {
                    Gizmos.DrawFrustum(Vector3.zero, FieldOfView, FarClipPlane, NearClipPlane, Aspect);
                }

                Gizmos.matrix = gizmosMatrix;
            }
            finally
            {
                Gizmos.color = buffer;
            }
        }
    }

    public struct ProjectionInfo
    {
        public static readonly Vector3 scaler = new Vector3(+1.0f, +1.0f, -1.0f);

        public enum EProjectionType
        {
            Orthographic,
            Perspective,
        }

        public EProjectionType ProjectionType { get; }

        public Matrix4x4 ViewMatrix { get; }
        public Matrix4x4 ProjectionMatrix { get; }
        public Matrix4x4 WorldToProjectionMatrix { get; }
        public Matrix4x4 GpuProjectionMatrix { get; }

        public Vector4 ProjectorPosition { get; }

        public ProjectionInfo(Transform transform, ProjectorSettings camera)
        {
            if (camera.Orthographic)
            {
                ProjectionType = EProjectionType.Orthographic;
            }
            else
            {
                ProjectionType = EProjectionType.Perspective;
            }

            ViewMatrix = Matrix4x4.Scale(scaler) * transform.worldToLocalMatrix;

            Vector4 pos;

            switch (ProjectionType)
            {
                case EProjectionType.Orthographic:
                default:
                    ProjectionMatrix = Matrix4x4.Ortho(-camera.OrthographicSize * camera.Aspect, +camera.OrthographicSize * camera.Aspect, -camera.OrthographicSize, +camera.OrthographicSize, camera.NearClipPlane, camera.FarClipPlane);
                    pos = (Vector4)transform.forward;
                    pos.w = 0.0f;
                    break;

                case EProjectionType.Perspective:
                    ProjectionMatrix = Matrix4x4.Perspective(camera.FieldOfView, camera.Aspect, camera.NearClipPlane, camera.FarClipPlane);
                    pos = (Vector4)transform.position;
                    pos.w = 1.0f;
                    break;
            }

            WorldToProjectionMatrix = ProjectionMatrix * ViewMatrix;
            GpuProjectionMatrix = GL.GetGPUProjectionMatrix(ProjectionMatrix, true);
            ProjectorPosition = pos;
        }

        public ProjectionInfo(Transform transform, Camera camera) : this(transform, new ProjectorSettings(camera))
        {

        }
    }
}
