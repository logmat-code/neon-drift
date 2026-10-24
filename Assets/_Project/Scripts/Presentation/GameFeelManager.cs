using System.Collections;
using UnityEngine;
using NeonDrift.Vehicle;

namespace NeonDrift.Presentation
{
    public class GameFeelManager : MonoBehaviour
    {
        [SerializeField] private VehicleController vehicle;

        private Coroutine hitStopRoutine;

        private void Start()
        {
            if (vehicle != null)
            {
                vehicle.OnImpact += HandleImpact;
            }
        }

        public void Initialize(VehicleController vc)
        {
            vehicle = vc;
            vehicle.OnImpact += HandleImpact;
        }

        private void HandleImpact(VehicleController.CollisionSeverity severity)
        {
            float duration = severity switch
            {
                VehicleController.CollisionSeverity.WallTouch => 0.03f,
                VehicleController.CollisionSeverity.TrafficHit => 0.06f,
                VehicleController.CollisionSeverity.HeavyCrash => 0.10f,
                _ => 0.04f
            };

            TriggerHitStop(duration);
        }

        public void TriggerHitStop(float duration)
        {
            if (hitStopRoutine != null) StopCoroutine(hitStopRoutine);
            hitStopRoutine = StartCoroutine(DoHitStop(duration));
        }

        private IEnumerator DoHitStop(float duration)
        {
            Time.timeScale = 0.05f;
            yield return new WaitForSecondsRealtime(duration);
            Time.timeScale = 1.0f;
            hitStopRoutine = null;
        }
    }
}
