
using UnityEngine;

namespace NeonDrift.World
{
    public class MountainParallaxController : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform targetCamera;

        [Header("Parallax Layers")]
        [SerializeField] private Transform farMountainLayer;
        [SerializeField] private Transform midMountainLayer;

        [Header("Scroll Factors")]
        public float farParallaxFactor = 0.08f;
        public float midParallaxFactor = 0.22f;

        private Vector3 lastCamPos;

        private void Start()
        {
            if (targetCamera == null)
            {
                var cam = Camera.main;
                if (cam != null) targetCamera = cam.transform;
            }
            if (targetCamera != null) lastCamPos = targetCamera.position;
        }

        private void LateUpdate()
        {
            if (targetCamera == null) return;

            Vector3 delta = targetCamera.position - lastCamPos;

            // Follow camera in general position so layers never drift offscreen
            transform.position = new Vector3(targetCamera.position.x, targetCamera.position.y, 0f);

            // Apply relative parallax displacement to layers
            if (farMountainLayer != null)
            {
                farMountainLayer.localPosition += new Vector3(-delta.x * farParallaxFactor, -delta.y * farParallaxFactor, 0f);
            }

            if (midMountainLayer != null)
            {
                midMountainLayer.localPosition += new Vector3(-delta.x * midParallaxFactor, -delta.y * midParallaxFactor, 0f);
            }

            lastCamPos = targetCamera.position;
        }
    }
}
