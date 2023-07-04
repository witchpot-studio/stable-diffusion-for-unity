using UnityEngine;

namespace Witchpot.Runtime.Projection
{
    [RequireComponent(typeof(Camera))]
    public class FreeCamera : MonoBehaviour
    {
        [SerializeField, Range(0.01f, 10.0f)]
        float moveSpeed = 0.1f;

        [SerializeField, Range(0.1f, 1.0f)]
        float rotateSpeed = 0.3f;

        Transform transformCache = default;

        bool preMouseButtonDown = false;

        Vector3 preMousePosition = default;

        void Awake()
        {
            transformCache = this.transform;
        }

        void Update()
        {
            KeyMove();

            MouseRotation();
        }

        void KeyMove()
        {
            var velocity = Vector3.zero;

            if (Input.GetKey(KeyCode.A))
            {
                velocity += Vector3.left * moveSpeed;
            }

            if (Input.GetKey(KeyCode.D))
            {
                velocity += Vector3.right * moveSpeed;
            }

            if (Input.GetKey(KeyCode.W))
            {
                velocity += Vector3.forward * moveSpeed;
            }

            if (Input.GetKey(KeyCode.S))
            {
                velocity += Vector3.back * moveSpeed;
            }

            if (Input.GetKey(KeyCode.E))
            {
                velocity += Vector3.up * moveSpeed;
            }

            if (Input.GetKey(KeyCode.Q))
            {
                velocity += Vector3.down * moveSpeed;
            }

            transformCache.Translate(velocity * Time.deltaTime, Space.Self);
        }

        void MouseRotation()
        {
            if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
            {
                var mousePosition = Input.mousePosition;

                if (preMouseButtonDown == false)
                {
                    preMousePosition = mousePosition;
                }

                preMouseButtonDown = true;

                var diff = mousePosition - preMousePosition;

                var angle = new Vector2(-diff.y, +diff.x) * rotateSpeed;

                transformCache.RotateAround(transformCache.position, transformCache.right, angle.x);
                transformCache.RotateAround(transformCache.position, Vector3.up, angle.y);

                preMousePosition = mousePosition;
            }
            else
            {
                preMouseButtonDown = false;
            }
        }
    }
}
