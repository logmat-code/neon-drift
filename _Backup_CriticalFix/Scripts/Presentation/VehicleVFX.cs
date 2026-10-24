using UnityEngine;
using NeonDrift.Vehicle;

namespace NeonDrift.Presentation
{
    public class VehicleVFX : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private VehicleController vehicle;

        [Header("Sprites")]
        [SerializeField] private Sprite smokeSprite;
        [SerializeField] private Sprite skidmarkSprite;
        [SerializeField] private Sprite speedStreakSprite;
        [SerializeField] private Sprite headlightConeSprite;

        private GameObject headlightBeam;
        private GameObject boostStreaksContainer;
        private float skidmarkSpawnTimer = 0f;
        private float smokeSpawnTimer = 0f;

        private void Start()
        {
            if (vehicle == null) vehicle = GetComponentInParent<VehicleController>();
            SetupHeadlights();
            SetupBoostStreaks();

            if (vehicle != null)
            {
                vehicle.OnBoostStarted += () => SetBoostVFX(true);
                vehicle.OnBoostEnded += () => SetBoostVFX(false);
            }
        }

        public void Initialize(VehicleController vc, Sprite smoke, Sprite skid, Sprite streak, Sprite lightCone)
        {
            vehicle = vc;
            smokeSprite = smoke;
            skidmarkSprite = skid;
            speedStreakSprite = streak;
            headlightConeSprite = lightCone;

            SetupHeadlights();
            SetupBoostStreaks();
        }

        private void SetupHeadlights()
        {
            if (headlightBeam == null && headlightConeSprite != null)
            {
                headlightBeam = new GameObject("HeadlightBeam");
                headlightBeam.transform.SetParent(transform);
                headlightBeam.transform.localPosition = new Vector3(0f, 1.8f, 0f);
                headlightBeam.transform.localRotation = Quaternion.identity;

                SpriteRenderer sr = headlightBeam.AddComponent<SpriteRenderer>();
                sr.sprite = headlightConeSprite;
                sr.sortingOrder = 5;
            }
        }

        private void SetupBoostStreaks()
        {
            if (boostStreaksContainer == null)
            {
                boostStreaksContainer = new GameObject("BoostStreaks");
                boostStreaksContainer.transform.SetParent(transform);
                boostStreaksContainer.transform.localPosition = Vector3.zero;

                if (speedStreakSprite != null)
                {
                    for (int i = -2; i <= 2; i++)
                    {
                        GameObject streak = new GameObject($"Streak_{i}");
                        streak.transform.SetParent(boostStreaksContainer.transform);
                        streak.transform.localPosition = new Vector3(i * 0.7f, -1.5f, 0f);
                        SpriteRenderer sr = streak.AddComponent<SpriteRenderer>();
                        sr.sprite = speedStreakSprite;
                        sr.sortingOrder = 3;
                    }
                }
                boostStreaksContainer.SetActive(false);
            }
        }

        private void Update()
        {
            if (vehicle == null) return;

            bool isDrifting = vehicle.currentState == DriftState.Drifting;

            if (isDrifting)
            {
                smokeSpawnTimer -= Time.deltaTime;
                if (smokeSpawnTimer <= 0f && smokeSprite != null)
                {
                    smokeSpawnTimer = 0.08f;
                    SpawnSmokePuff();
                }

                skidmarkSpawnTimer -= Time.deltaTime;
                if (skidmarkSpawnTimer <= 0f && skidmarkSprite != null)
                {
                    skidmarkSpawnTimer = 0.06f;
                    SpawnSkidmark();
                }
            }
        }

        private void SpawnSmokePuff()
        {
            GameObject smoke = new GameObject("SmokePuff");
            Vector3 spawnPos = transform.position - (transform.up * 1.8f) + (transform.right * Random.Range(-0.8f, 0.8f));
            smoke.transform.position = spawnPos;
            smoke.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));

            SpriteRenderer sr = smoke.AddComponent<SpriteRenderer>();
            sr.sprite = smokeSprite;
            sr.sortingOrder = 4;

            Destroy(smoke, 0.45f);
        }

        private void SpawnSkidmark()
        {
            GameObject skid = new GameObject("Skidmark");
            skid.transform.position = transform.position - (transform.up * 1.6f);
            skid.transform.rotation = transform.rotation;

            SpriteRenderer sr = skid.AddComponent<SpriteRenderer>();
            sr.sprite = skidmarkSprite;
            sr.sortingOrder = -8; // Lay flat right above asphalt

            Destroy(skid, 5.0f);
        }

        private void SetBoostVFX(bool active)
        {
            if (boostStreaksContainer != null)
            {
                boostStreaksContainer.SetActive(active);
            }
        }
    }
}
