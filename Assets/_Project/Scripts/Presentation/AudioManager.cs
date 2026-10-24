using UnityEngine;
using NeonDrift.Vehicle;

namespace NeonDrift.Presentation
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource engineSource;
        [SerializeField] private AudioSource driftSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("Clips")]
        [SerializeField] private AudioClip musicLoop;
        [SerializeField] private AudioClip engineLoop;
        [SerializeField] private AudioClip driftScreech;
        [SerializeField] private AudioClip boostSfx;
        [SerializeField] private AudioClip crashSfx;
        [SerializeField] private AudioClip nearMissSfx;
        [SerializeField] private AudioClip uiClickSfx;
        [SerializeField] private AudioClip gameOverSfx;

        [Header("Tuning")]
        public float masterVolume = 1f;
        public float musicVolume = 0.8f;
        public float engineVolume = 0.8f;
        public float sfxVolume = 1f;

        // Custom Clip Properties for ProjectBuilder wiring
        public AudioClip CustomBoostClip { get => boostSfx; set => boostSfx = value; }
        public AudioClip CustomCrashClip { get => crashSfx; set => crashSfx = value; }
        public AudioClip CustomHeavyCrashClip { get => crashSfx; set => crashSfx = value; }
        public AudioClip CustomUiClickClip { get => uiClickSfx; set => uiClickSfx = value; }
        public AudioClip CustomGameOverClip { get => gameOverSfx; set => gameOverSfx = value; }
        public AudioClip CustomNearMissClip { get => nearMissSfx; set => nearMissSfx = value; }
        public AudioClip CustomMusicClip { get => musicLoop; set => musicLoop = value; }

        private VehicleController targetVehicle;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            EnsureSources();
        }

        private void EnsureSources()
        {
            if (musicSource == null) musicSource = gameObject.AddComponent<AudioSource>();
            if (engineSource == null) engineSource = gameObject.AddComponent<AudioSource>();
            if (driftSource == null) driftSource = gameObject.AddComponent<AudioSource>();
            if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();

            musicSource.loop = true;
            engineSource.loop = true;
            driftSource.loop = true;

            musicSource.playOnAwake = false;
            engineSource.playOnAwake = false;
            driftSource.playOnAwake = false;
            sfxSource.playOnAwake = false;
        }

        public void SetTargetVehicle(VehicleController vehicle)
        {
            targetVehicle = vehicle;
        }

        private void Update()
        {
            if (targetVehicle == null) return;

            // 1. Reactive Engine Audio (Pitch & Volume response to vehicle speed & boost)
            if (engineSource != null && engineSource.isPlaying)
            {
                float speedRatio = Mathf.Clamp01(targetVehicle.currentSpeed / 28f);
                float basePitch = 0.8f + speedRatio * 1.2f;
                if (targetVehicle.isBoosting) basePitch += 0.35f;
                engineSource.pitch = Mathf.Lerp(engineSource.pitch, basePitch, Time.deltaTime * 8f);
                engineSource.volume = masterVolume * engineVolume * (0.4f + speedRatio * 0.6f);
            }

            // 2. Reactive Drift Tire Screech (Smooth fade & intensity based on slip angle)
            if (driftSource != null)
            {
                bool isDrifting = targetVehicle.currentState == DriftState.Drifting || targetVehicle.currentState == DriftState.DriftEntry;
                if (isDrifting && targetVehicle.currentSpeed > 5f)
                {
                    if (!driftSource.isPlaying && driftScreech != null)
                    {
                        driftSource.clip = driftScreech;
                        driftSource.Play();
                    }
                    float slipIntensity = Mathf.Clamp01(targetVehicle.currentSlipAngle / 40f);
                    driftSource.volume = Mathf.Lerp(driftSource.volume, masterVolume * sfxVolume * slipIntensity * 0.85f, Time.deltaTime * 10f);
                    driftSource.pitch = Mathf.Lerp(driftSource.pitch, 0.9f + slipIntensity * 0.35f, Time.deltaTime * 6f);
                }
                else
                {
                    driftSource.volume = Mathf.Lerp(driftSource.volume, 0f, Time.deltaTime * 12f);
                    if (driftSource.volume <= 0.01f && driftSource.isPlaying)
                    {
                        driftSource.Stop();
                    }
                }
            }
        }

        public void StartMusic()
        {
            if (musicSource != null && musicLoop != null && !musicSource.isPlaying)
            {
                musicSource.clip = musicLoop;
                musicSource.volume = masterVolume * musicVolume;
                musicSource.Play();
            }
        }

        public void StartEngine()
        {
            if (engineSource != null && engineLoop != null)
            {
                engineSource.clip = engineLoop;
                engineSource.volume = masterVolume * engineVolume;
                engineSource.Play();
            }
        }

        public void StopEngine()
        {
            engineSource?.Stop();
            driftSource?.Stop();
        }

        public void PlayBoost() => PlaySFX(boostSfx);
        public void PlayCrash() => PlaySFX(crashSfx);
        public void PlayHeavyCrash() => PlaySFX(crashSfx);
        public void PlayNearMiss() => PlaySFX(nearMissSfx);
        public void PlayUIClick() => PlaySFX(uiClickSfx);
        public void PlayGameOver() => PlaySFX(gameOverSfx);

        private void PlaySFX(AudioClip clip)
        {
            if (sfxSource != null && clip != null)
            {
                sfxSource.PlayOneShot(clip, masterVolume * sfxVolume);
            }
        }

        public void SetMasterVolume(float vol) { masterVolume = Mathf.Clamp01(vol); ApplyVolumes(); }
        public void SetMusicVolume(float vol) { musicVolume = Mathf.Clamp01(vol); ApplyVolumes(); }
        public void SetEngineVolume(float vol) { engineVolume = Mathf.Clamp01(vol); ApplyVolumes(); }
        public void SetSFXVolume(float vol) { sfxVolume = Mathf.Clamp01(vol); ApplyVolumes(); }

        private void ApplyVolumes()
        {
            if (musicSource != null) musicSource.volume = masterVolume * musicVolume;
            if (engineSource != null) engineSource.volume = masterVolume * engineVolume;
        }
    }
}
