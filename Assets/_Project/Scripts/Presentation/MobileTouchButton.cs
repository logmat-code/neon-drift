using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace NeonDrift.Presentation
{
    /// <summary>
    /// High-reliability mobile touch button designed specifically for arcade controls.
    /// Handles simultaneous multitouch, generous touch boundaries to prevent accidental slip-offs,
    /// and provides immediate visual feedback (scale + tint).
    /// </summary>
    [RequireComponent(typeof(Graphic))]
    public class MobileTouchButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public bool IsPressed { get; private set; }

        public event Action OnPressed;
        public event Action OnReleased;

        [Header("Button ID / Role")]
        public string buttonTag = "Button";

        [Header("Visual Feedback")]
        public bool animateScale = true;
        public float pressedScale = 0.90f;
        public Color pressedTint = new Color(0.85f, 0.95f, 1f, 1f);

        private Graphic graphic;
        private Color originalColor;
        private Vector3 originalScale;
        private int activePointerId = -999;
        private Vector2 downScreenPos;

        private void Awake()
        {
            graphic = GetComponent<Graphic>();
            if (graphic != null) originalColor = graphic.color;
            originalScale = transform.localScale;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!IsPressed)
            {
                activePointerId = eventData.pointerId;
                downScreenPos = eventData.position;
                IsPressed = true;

                ApplyVisual(true);
                OnPressed?.Invoke();
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (IsPressed)
            {
                Release();
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            // Do NOT prematurely cancel on micro-movements!
            // Only release if the touch has moved far away (> 220 screen pixels) from the down position
            if (IsPressed && eventData.pointerId == activePointerId)
            {
                float dist = Vector2.Distance(eventData.position, downScreenPos);
                if (dist > 220f)
                {
                    Release();
                }
            }
        }

        private void OnDisable()
        {
            if (IsPressed)
            {
                Release();
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus && IsPressed)
            {
                Release();
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && IsPressed)
            {
                Release();
            }
        }

        public void Release()
        {
            if (!IsPressed) return;

            IsPressed = false;
            activePointerId = -999;

            ApplyVisual(false);
            OnReleased?.Invoke();
        }

        private void ApplyVisual(bool pressed)
        {
            if (animateScale)
            {
                transform.localScale = pressed ? originalScale * pressedScale : originalScale;
            }

            if (graphic != null)
            {
                graphic.color = pressed ? pressedTint : originalColor;
            }
        }

        // Programmatic simulation hook for unit & manual testing
        public void SimulatePress()
        {
            if (!IsPressed)
            {
                IsPressed = true;
                ApplyVisual(true);
                OnPressed?.Invoke();
            }
        }

        public void SimulateRelease()
        {
            if (IsPressed)
            {
                Release();
            }
        }
    }
}
