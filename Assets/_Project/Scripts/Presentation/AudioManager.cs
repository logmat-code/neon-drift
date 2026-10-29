using UnityEngine.Audio;
using System;
using UnityEngine;
using NeonDrift.Vehicle;

namespace NeonDrift.Presentation
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources (Procedural Multi-Channel Arcade Mix)")]
        [SerializeField] private AudioMixer masterMixer;
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource engineIdleSource;
        [SerializeField] private AudioSource engineHighSource;
        [SerializeField] private AudioSource tireRollSource;
        [SerializeField] private AudioSource driftSource;
        [SerializeField] private AudioSource windSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource uiSource;

        [Header("Engine Loops (Crossfade System)")]
        [SerializeField] private AudioClip engineIdleLoop;
        [SerializeField] private AudioClip engineHighLoop;

        [Header("Tires & Road")]
        [SerializeField] private AudioClip tireRollLoop;
        [SerializeField] private AudioClip driftScreech;

        [Header("Environment")]
        [SerializeField] private AudioClip windLoop;
        [SerializeField] private AudioClip musicLoop;

        [Header("SFX & Gameplay Hits")]
        [SerializeField] private AudioClip boostSfx;
        [SerializeField] private AudioClip crashSfx;
        [SerializeField] private AudioClip heavyCrashSfx;
        [SerializeField] private AudioClip obstacleHitSfx;
        [SerializeField] private AudioClip trafficPassbySfx;
        [SerializeField] private AudioClip nearMissSfx;
        [SerializeField] private AudioClip gameOverSfx;

        [Header("Rewards & UI")]
        [SerializeField] private AudioClip neonCreditsSfx;
        [SerializeField] private AudioClip comboMultiplierSfx;
        [SerializeField] private AudioClip purchaseSuccessSfx;
        [SerializeField] private AudioClip purchaseDeniedSfx;
        [SerializeField] private AudioClip uiClickSfx;

        [Header("Channel Volume Sliders (0.0 to 1.0)")]
        [Range(0f, 1f)] public float masterVolume = 1f;
        [Range(0f, 1f)] public float musicVolume = 0.75f;
        [Range(0f, 1f)] public float engineVolume = 0.85f;
        [Range(0f, 1f)] public float sfxVolume = 0.90f;
        [Range(0f, 1f)] public float ambienceVolume = 0.65f;
        [Range(0f, 1f)] public float uiVolume = 0.80f;

        // Custom Clip Properties for ProjectBuilder and Editor compatibility
        public AudioClip CustomBoostClip { get => boostSfx; set => boostSfx = value; }
        public AudioClip CustomCrashClip { get => crashSfx; set => crashSfx = value; }
        public AudioClip CustomHeavyCrashClip { get => heavyCrashSfx != null ? heavyCrashSfx : crashSfx; set => heavyCrashSfx = value; }
        public AudioClip CustomUiClickClip { get => uiClickSfx; set => uiClickSfx = value; }
        public AudioClip CustomGameOverClip { get => gameOverSfx; set => gameOverSfx = value; }
        public AudioClip CustomNearMissClip { get => nearMissSfx; set => nearMissSfx = value; }
        public AudioClip CustomMusicClip { get => musicLoop; set => musicLoop = value; }

        private VehicleController targetVehicle;
        private bool isEngineRunning = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            EnsureAllSources();
        }

        private void Start()
        {
            EnsureAllSources();
            LoadUserVolumePreferences();
        }

        public void EnsureAllSources()
        {
            musicSource = ConfigureSource(musicSource, "MusicSource", true);
            engineIdleSource = ConfigureSource(engineIdleSource, "EngineIdleSource", true);
            engineHighSource = ConfigureSource(engineHighSource, "EngineHighSource", true);
            tireRollSource = ConfigureSource(tireRollSource, "TireRollSource", true);
            driftSource = ConfigureSource(driftSource, "DriftSource", true);
            windSource = ConfigureSource(windSource, "WindSource", true);
            sfxSource = ConfigureSource(sfxSource, "SfxSource", false);
            uiSource = ConfigureSource(uiSource, "UiSource", false);
        }

        private AudioSource ConfigureSource(AudioSource src, string childName, bool loop)
        {
            if (src == null)
            {
                Transform t = transform.Find(childName);
                if (t != null) src = t.GetComponent<AudioSource>();
                if (src == null)
                {
                    GameObject child = new GameObject(childName);
                    child.transform.SetParent(transform, false);
                    src = child.AddComponent<AudioSource>();
                }
            }

            src.loop = loop;
            src.playOnAwake = false;
            src.spatialBlend = 0f; // 2D clean arcade punch
            src.bypassEffects = false;
            return src;
        }

        public void SetTargetVehicle(VehicleController vehicle)
        {
            targetVehicle = vehicle;
        }

        private void Update()
        {
            if (targetVehicle == null)
            {
                targetVehicle = UnityEngine.Object.FindAnyObjectByType<VehicleController>();
            }

            if (targetVehicle == null || !isEngineRunning) return;

            float dt = Time.deltaTime;
            float speed = Mathf.Max(0f, targetVehicle.currentSpeed);
            float topSpeed = targetVehicle.Config != null ? targetVehicle.Config.baseTopSpeed : 22f;
            float speedRatio = Mathf.Clamp01(speed / Mathf.Max(10f, topSpeed));
            bool isBoosting = targetVehicle.isBoosting;

            // ==========================================
            // 1. DUAL-LAYER ENGINE AUDIO (Crossfade Low / High)
            // ==========================================
            if (engineIdleSource != null)
            {
                if (!engineIdleSource.isPlaying && engineIdleLoop != null)
                {
                    engineIdleSource.clip = engineIdleLoop;
                    engineIdleSource.Play();
                }
                // Idle fades out as high RPM screams
                float idleVol = masterVolume * engineVolume * Mathf.Lerp(0.85f, 0.15f, speedRatio);
                engineIdleSource.volume = Mathf.MoveTowards(engineIdleSource.volume, idleVol, dt * 4f);
                engineIdleSource.pitch = Mathf.Lerp(0.9f, 1.45f, speedRatio);
            }

            if (engineHighSource != null)
            {
                if (!engineHighSource.isPlaying && engineHighLoop != null)
                {
                    engineHighSource.clip = engineHighLoop;
                    engineHighSource.Play();
                }
                // High RPM screams in with speed and boost
                float highVolTarget = masterVolume * engineVolume * Mathf.Lerp(0.05f, 0.95f, speedRatio);
                if (isBoosting) highVolTarget = masterVolume * engineVolume * 1.0f;
                engineHighSource.volume = Mathf.MoveTowards(engineHighSource.volume, highVolTarget, dt * 5f);
                
                float targetPitch = Mathf.Lerp(0.75f, 1.55f, speedRatio) + (isBoosting ? 0.35f : 0f);
                engineHighSource.pitch = Mathf.MoveTowards(engineHighSource.pitch, targetPitch, dt * 6f);
            }

            // ==========================================
            // 2. ASPHALT TIRE ROLLING NOISE
            // ==========================================
            if (tireRollSource != null)
            {
                if (!tireRollSource.isPlaying && tireRollLoop != null)
                {
                    tireRollSource.clip = tireRollLoop;
                    tireRollSource.Play();
                }
                float rollVol = masterVolume * sfxVolume * Mathf.Lerp(0.0f, 0.70f, speedRatio);
                tireRollSource.volume = Mathf.MoveTowards(tireRollSource.volume, rollVol, dt * 5f);
                tireRollSource.pitch = Mathf.Lerp(0.85f, 1.35f, speedRatio);
            }

            // ==========================================
            // 3. REACTIVE DRIFT SCREECH (Angle & Speed responsive)
            // ==========================================
            if (driftSource != null)
            {
                bool isDrifting = targetVehicle.currentState == DriftState.Drifting || targetVehicle.currentState == DriftState.DriftEntry;
                if (isDrifting && speed > 4f)
                {
                    if (!driftSource.isPlaying && driftScreech != null)
                    {
                        driftSource.clip = driftScreech;
                        driftSource.Play();
                    }
                    float slipIntensity = Mathf.Clamp01(targetVehicle.currentSlipAngle / 42f);
                    float targetDriftVol = masterVolume * sfxVolume * Mathf.Lerp(0.35f, 0.95f, slipIntensity);
                    driftSource.volume = Mathf.MoveTowards(driftSource.volume, targetDriftVol, dt * 10f);
                    driftSource.pitch = Mathf.Lerp(0.92f, 1.25f, slipIntensity);
                }
                else
                {
                    driftSource.volume = Mathf.MoveTowards(driftSource.volume, 0f, dt * 12f);
                    if (driftSource.volume <= 0.01f && driftSource.isPlaying)
                    {
                        driftSource.Stop();
                    }
                }
            }

            // ==========================================
            // 4. AERODYNAMIC HIGHWAY WIND RUSH
            // ==========================================
            if (windSource != null)
            {
                if (!windSource.isPlaying && windLoop != null)
                {
                    windSource.clip = windLoop;
                    windSource.Play();
                }
                float windTarget = (speedRatio > 0.4f) ? (speedRatio - 0.4f) / 0.6f : 0f;
                if (isBoosting) windTarget = 1.0f;
                windSource.volume = Mathf.MoveTowards(windSource.volume, masterVolume * ambienceVolume * windTarget * 0.75f, dt * 4f);
                windSource.pitch = Mathf.Lerp(0.9f, 1.4f, speedRatio);
            }
        }

        // =========================================================================
        // PUBLIC PLAYBACK API
        // =========================================================================

        public void StartMusic()
        {
            if (musicSource != null && musicLoop != null)
            {
                if (!musicSource.isPlaying || musicSource.clip != musicLoop)
                {
                    musicSource.clip = musicLoop;
                    musicSource.volume = masterVolume * musicVolume;
                    musicSource.Play();
                }
            }
        }

        public void StopMusic()
        {
            musicSource?.Stop();
        }

        public void StartEngine()
        {
            isEngineRunning = true;
            EnsureAllSources();
            if (engineIdleSource != null && engineIdleLoop != null && !engineIdleSource.isPlaying)
            {
                engineIdleSource.clip = engineIdleLoop;
                engineIdleSource.Play();
            }
            if (engineHighSource != null && engineHighLoop != null && !engineHighSource.isPlaying)
            {
                engineHighSource.clip = engineHighLoop;
                engineHighSource.Play();
            }
            if (tireRollSource != null && tireRollLoop != null && !tireRollSource.isPlaying)
            {
                tireRollSource.clip = tireRollLoop;
                tireRollSource.Play();
            }
            if (windSource != null && windLoop != null && !windSource.isPlaying)
            {
                windSource.clip = windLoop;
                windSource.Play();
            }
        }

        public void StopEngine()
        {
            isEngineRunning = false;
            engineIdleSource?.Stop();
            engineHighSource?.Stop();
            tireRollSource?.Stop();
            driftSource?.Stop();
            windSource?.Stop();
        }

        public void PlayBoost() => PlaySFX(boostSfx, 1.0f);
        public void PlayCrash() => PlaySFX(crashSfx, 0.95f);
        public void PlayHeavyCrash() => PlaySFX(heavyCrashSfx != null ? heavyCrashSfx : crashSfx, 1.0f);
        public void PlayObstacleHit() => PlaySFX(obstacleHitSfx, 0.85f);
        public void PlayTrafficPassby() => PlaySFX(trafficPassbySfx, 0.75f);
        public void PlayNearMiss() => PlaySFX(nearMissSfx, 0.90f);
        public void PlayGameOver() => PlaySFX(gameOverSfx, 1.0f);

        // UI & Economy Audio
        public void PlayUIClick() => PlayUI(uiClickSfx, 0.80f);
        public void PlayNeonCredits() => PlayUI(neonCreditsSfx, 0.85f);
        public void PlayComboMultiplier() => PlayUI(comboMultiplierSfx, 0.85f);
        public void PlayPurchaseSuccess() => PlayUI(purchaseSuccessSfx, 0.95f);
        public void PlayPurchaseDenied() => PlayUI(purchaseDeniedSfx, 0.90f);

        private void PlaySFX(AudioClip clip, float scale = 1.0f)
        {
            if (sfxSource != null && clip != null)
            {
                sfxSource.PlayOneShot(clip, masterVolume * sfxVolume * scale);
            }
        }

        private void PlayUI(AudioClip clip, float scale = 1.0f)
        {
            if (uiSource != null && clip != null)
            {
                uiSource.PlayOneShot(clip, masterVolume * uiVolume * scale);
            }
        }

        // Settings & Persistence
        public void SetMasterVolume(float vol) { masterVolume = Mathf.Clamp01(vol); SaveUserVolumePreferences(); ApplyAllVolumes(); }
        public void SetMusicVolume(float vol) { musicVolume = Mathf.Clamp01(vol); SaveUserVolumePreferences(); ApplyAllVolumes(); }
        public void SetEngineVolume(float vol) { engineVolume = Mathf.Clamp01(vol); SaveUserVolumePreferences(); ApplyAllVolumes(); }
        public void SetSFXVolume(float vol) { sfxVolume = Mathf.Clamp01(vol); SaveUserVolumePreferences(); ApplyAllVolumes(); }
        public void SetAmbienceVolume(float vol) { ambienceVolume = Mathf.Clamp01(vol); SaveUserVolumePreferences(); ApplyAllVolumes(); }
        public void SetUIVolume(float vol) { uiVolume = Mathf.Clamp01(vol); SaveUserVolumePreferences(); ApplyAllVolumes(); }

        public void ApplyAllVolumes()
        {
            if (musicSource != null) musicSource.volume = masterVolume * musicVolume;
            if (engineIdleSource != null) engineIdleSource.volume = masterVolume * engineVolume * 0.7f;
            if (engineHighSource != null) engineHighSource.volume = masterVolume * engineVolume * 0.5f;
            if (tireRollSource != null) tireRollSource.volume = masterVolume * sfxVolume * 0.5f;
            if (windSource != null) windSource.volume = masterVolume * ambienceVolume * 0.4f;
        }

        private void SaveUserVolumePreferences()
        {
            PlayerPrefs.SetFloat("ND_Vol_Master", masterVolume);
            PlayerPrefs.SetFloat("ND_Vol_Music", musicVolume);
            PlayerPrefs.SetFloat("ND_Vol_Engine", engineVolume);
            PlayerPrefs.SetFloat("ND_Vol_SFX", sfxVolume);
            PlayerPrefs.SetFloat("ND_Vol_Ambience", ambienceVolume);
            PlayerPrefs.SetFloat("ND_Vol_UI", uiVolume);
            PlayerPrefs.Save();
        }

        private void LoadUserVolumePreferences()
        {
            masterVolume = PlayerPrefs.GetFloat("ND_Vol_Master", 1.0f);
            musicVolume = PlayerPrefs.GetFloat("ND_Vol_Music", 0.75f);
            engineVolume = PlayerPrefs.GetFloat("ND_Vol_Engine", 0.85f);
            sfxVolume = PlayerPrefs.GetFloat("ND_Vol_SFX", 0.90f);
            ambienceVolume = PlayerPrefs.GetFloat("ND_Vol_Ambience", 0.65f);
            uiVolume = PlayerPrefs.GetFloat("ND_Vol_UI", 0.80f);
            ApplyAllVolumes();
        }
    }
}
