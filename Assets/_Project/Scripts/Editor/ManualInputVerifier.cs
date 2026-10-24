using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using NeonDrift.Presentation;
using NeonDrift.Vehicle;

namespace NeonDrift.Editor
{
    public static class ManualInputVerifier
    {
        [MenuItem("NeonDrift/Verify Real UI Input")]
        public static void VerifyUI()
        {
            Debug.Log("==================================================");
            Debug.Log(">>> AUDITING REAL UI INPUT PIPELINE IN SCENE <<<");
            Debug.Log("==================================================");

            string sandboxPath = "Assets/_Project/Scenes/HandlingSandbox.unity";
            EditorSceneManager.OpenScene(sandboxPath);

            var canvas = Object.FindFirstObjectByType<Canvas>();
            var raycaster = Object.FindFirstObjectByType<GraphicRaycaster>();
            var eventSystem = Object.FindFirstObjectByType<EventSystem>();
            var sbm = Object.FindFirstObjectByType<HandlingSandboxManager>();
            var vc = Object.FindFirstObjectByType<VehicleController>();

            Debug.Log($"[UI Audit] Canvas: {canvas?.name}, Raycaster: {raycaster != null}, EventSystem: {eventSystem != null}, SBM: {sbm != null}, VC: {vc != null}");

            var touchLeft = GameObject.Find("TouchLeft");
            var touchRight = GameObject.Find("TouchRight");
            var touchDrift = GameObject.Find("TouchDrift");
            var touchBoost = GameObject.Find("TouchBoost");

            Debug.Log($"Buttons found: Left={touchLeft!=null}, Right={touchRight!=null}, Drift={touchDrift!=null}, Boost={touchBoost!=null}");

            void InspectButton(string name, GameObject go)
            {
                if (go == null) { Debug.LogError($"[UI Audit] {name} is NULL!"); return; }
                var rt = go.GetComponent<RectTransform>();
                var img = go.GetComponent<Image>();
                var mtb = go.GetComponent<MobileTouchButton>();
                var et = go.GetComponent<EventTrigger>();
                var btn = go.GetComponent<Button>();

                Vector3[] corners = new Vector3[4];
                rt.GetWorldCorners(corners);
                Vector2 centerScreen = RectTransformUtility.WorldToScreenPoint(null, rt.position);
                Debug.Log($"[Button: {name}] Pos={rt.anchoredPosition}, Size={rt.sizeDelta}, CenterScreen={centerScreen}, BL={corners[0]}, TR={corners[2]}, RaycastTarget={img?.raycastTarget}, MTB={mtb != null}, ET={et != null}, Btn={btn != null}");
            }

            InspectButton("TouchLeft", touchLeft);
            InspectButton("TouchRight", touchRight);
            InspectButton("TouchDrift", touchDrift);
            InspectButton("TouchBoost", touchBoost);

            // Test raycasting against them
            Vector2[] screenPositions = new Vector2[]
            {
                new Vector2(-360 + 540, -740 + 960), // Left: (180, 220)
                new Vector2(-160 + 540, -740 + 960), // Right: (380, 220)
                new Vector2(160 + 540, -740 + 960),  // Drift: (700, 220)
                new Vector2(360 + 540, -740 + 960)   // Boost: (900, 220)
            };
            string[] btnNames = new string[] { "TouchLeft", "TouchRight", "TouchDrift", "TouchBoost" };

            for (int i = 0; i < screenPositions.Length; i++)
            {
                PointerEventData ped = new PointerEventData(eventSystem);
                ped.position = screenPositions[i];
                List<RaycastResult> hits = new List<RaycastResult>();
                raycaster.Raycast(ped, hits);

                Debug.Log($"[Raycast Test {btnNames[i]} at {screenPositions[i]}]: Hit Count = {hits.Count}");
                for (int h = 0; h < hits.Count; h++)
                {
                    Debug.Log($"   -> Hit[{h}]: {hits[h].gameObject.name} (type: {hits[h].gameObject.GetComponent<Graphic>()?.GetType().Name})");
                }
            }

            var sbmStart = typeof(HandlingSandboxManager).GetMethod("Start", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (sbmStart != null) sbmStart.Invoke(sbm, null);

            var vcAwake = typeof(VehicleController).GetMethod("Awake", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (vcAwake != null) vcAwake.Invoke(vc, null);

            // Test dispatching pointer events to TouchLeft
            if (touchLeft != null)
            {
                PointerEventData pedLeft = new PointerEventData(eventSystem) { pointerId = 0 };
                Debug.Log($"[Dispatching PointerDown to TouchLeft] Before: steerInput={vc.steerInput}");
                ExecuteEvents.Execute(touchLeft, pedLeft, ExecuteEvents.pointerDownHandler);
                Debug.Log($"[Dispatched PointerDown to TouchLeft] After: steerInput={vc.steerInput}");
                ExecuteEvents.Execute(touchLeft, pedLeft, ExecuteEvents.pointerUpHandler);
                Debug.Log($"[Dispatched PointerUp to TouchLeft] After: steerInput={vc.steerInput}");
            }

            // Test dispatching pointer events to TouchRight
            if (touchRight != null)
            {
                PointerEventData pedRight = new PointerEventData(eventSystem) { pointerId = 0 };
                Debug.Log($"[Dispatching PointerDown to TouchRight] Before: steerInput={vc.steerInput}");
                ExecuteEvents.Execute(touchRight, pedRight, ExecuteEvents.pointerDownHandler);
                Debug.Log($"[Dispatched PointerDown to TouchRight] After: steerInput={vc.steerInput}");
                ExecuteEvents.Execute(touchRight, pedRight, ExecuteEvents.pointerUpHandler);
                Debug.Log($"[Dispatched PointerUp to TouchRight] After: steerInput={vc.steerInput}");
            }

            // Test dispatching pointer events to TouchDrift
            if (touchDrift != null)
            {
                PointerEventData pedDrift = new PointerEventData(eventSystem) { pointerId = 1, position = screenPositions[2] };
                Debug.Log($"[Dispatching PointerDown to TouchDrift] Before: driftHeld={vc.driftHeld}");
                ExecuteEvents.Execute(touchDrift, pedDrift, ExecuteEvents.pointerDownHandler);
                Debug.Log($"[Dispatched PointerDown to TouchDrift] After: driftHeld={vc.driftHeld}");
                ExecuteEvents.Execute(touchDrift, pedDrift, ExecuteEvents.pointerUpHandler);
                Debug.Log($"[Dispatched PointerUp to TouchDrift] After: driftHeld={vc.driftHeld}");
            }

            // Test dispatching pointer events to TouchBoost
            if (touchBoost != null)
            {
                PointerEventData pedBoost = new PointerEventData(eventSystem) { pointerId = 2, position = screenPositions[3] };
                Debug.Log($"[Dispatching PointerDown to TouchBoost] Before: boostRequested={vc.boostRequested}, isBoosting={vc.isBoosting}");
                ExecuteEvents.Execute(touchBoost, pedBoost, ExecuteEvents.pointerDownHandler);
                Debug.Log($"[Dispatched PointerDown to TouchBoost] After: boostRequested={vc.boostRequested}, isBoosting={vc.isBoosting}");
                ExecuteEvents.Execute(touchBoost, pedBoost, ExecuteEvents.pointerUpHandler);
                Debug.Log($"[Dispatched PointerUp to TouchBoost] After: boostRequested={vc.boostRequested}, isBoosting={vc.isBoosting}");
            }

            // SIMULTANEOUS MULTITOUCH TEST
            Debug.Log(">>> TESTING SIMULTANEOUS MULTITOUCH (LEFT + DRIFT + BOOST) <<<");
            PointerEventData finger0 = new PointerEventData(eventSystem) { pointerId = 0, position = screenPositions[0] };
            PointerEventData finger1 = new PointerEventData(eventSystem) { pointerId = 1, position = screenPositions[2] };
            PointerEventData finger2 = new PointerEventData(eventSystem) { pointerId = 2, position = screenPositions[3] };

            ExecuteEvents.Execute(touchLeft, finger0, ExecuteEvents.pointerDownHandler);
            ExecuteEvents.Execute(touchDrift, finger1, ExecuteEvents.pointerDownHandler);
            ExecuteEvents.Execute(touchBoost, finger2, ExecuteEvents.pointerDownHandler);

            Debug.Log($"[Multitouch DOWN 3 fingers]: steerInput={vc.steerInput} (expected -1), driftHeld={vc.driftHeld} (expected true), boostRequested={vc.boostRequested} (expected true)");

            ExecuteEvents.Execute(touchLeft, finger0, ExecuteEvents.pointerUpHandler);
            Debug.Log($"[Multitouch Release finger 0]: steerInput={vc.steerInput} (expected 0), driftHeld={vc.driftHeld} (expected true)");

            ExecuteEvents.Execute(touchDrift, finger1, ExecuteEvents.pointerUpHandler);
            ExecuteEvents.Execute(touchBoost, finger2, ExecuteEvents.pointerUpHandler);
            Debug.Log($"[Multitouch Release fingers 1 & 2]: steerInput={vc.steerInput} (expected 0), driftHeld={vc.driftHeld} (expected false)");
        }
    }
}
