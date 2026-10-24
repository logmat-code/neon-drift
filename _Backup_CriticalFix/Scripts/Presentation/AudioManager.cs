using UnityEngine;

namespace NeonDrift.Presentation
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource engineSource;
        [SerializeField] private AudioSource skidSource;
        [SerializeField] private AudioSource boostSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("Optional Custom Audio Clips (If null, synthesized clips are generated)")]
        [SerializeField] private AudioClip customEngineClip;
        [SerializeField] private AudioClip customSkidClip;
        [SerializeField] private AudioClip customBoostClip;
        [SerializeField] private AudioClip customCrashClip;
        [SerializeField] private AudioClip customUiClickClip;
        [SerializeField] private AudioClip customPerfectExitClip;

        private AudioClip synthEngineClip;
        private AudioClip synthSkidClip;
        private AudioClip synthBoostClip;
        private AudioClip synthCrashClip;
        private AudioClip synthUiClickClip;
        private AudioClip synthPerfectClip;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            SetupAudioSources();
            GenerateProceduralAudioClips();
        }

        private void SetupAudioSources()
        {
            if (engineSource == null)
            {
                engineSource = gameObject.AddComponent<AudioSource>();
                engineSource.loop = true;
                engineSource.playOnAwake = false;
                engineSource.volume = 0.35f;
            }
            if (skidSource == null)
            {
                skidSource = gameObject.AddComponent<AudioSource>();
                skidSource.loop = true;
                skidSource.playOnAwake = false;
                skidSource.volume = 0f;
            }
            if (boostSource == null)
            {
                boostSource = gameObject.AddComponent<AudioSource>();
                boostSource.loop = false;
                boostSource.playOnAwake = false;
                boostSource.volume = 0.6f;
            }
            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.loop = false;
                sfxSource.playOnAwake = false;
                sfxSource.volume = 0.5f;
            }
        }

        private void GenerateProceduralAudioClips()
        {
            int sampleRate = 44100;

            // 1. Engine Loop (harmonic low rumble)
            float engineDuration = 0.5f;
            int engineSamples = Mathf.RoundToInt(sampleRate * engineDuration);
            float[] engineData = new float[engineSamples];
            for (int i = 0; i < engineSamples; i++)
            {
                float t = (float)i / sampleRate;
                float wave = Mathf.Sin(2f * Mathf.PI * 65f * t) * 0.5f 
                           + Mathf.Sin(2f * Mathf.PI * 130f * t) * 0.3f 
                           + Mathf.Sin(2f * Mathf.PI * 195f * t) * 0.15f;
                engineData[i] = wave;
            }
            synthEngineClip = AudioClip.Create("SynthEngine", engineSamples, 1, sampleRate, false);
            synthEngineClip.SetData(engineData, 0);

            // 2. Tire Skid (noise band)
            float skidDuration = 0.4f;
            int skidSamples = Mathf.RoundToInt(sampleRate * skidDuration);
            float[] skidData = new float[skidSamples];
            for (int i = 0; i < skidSamples; i++)
            {
                float noise = (Random.value * 2f - 1f) * 0.4f;
                float t = (float)i / sampleRate;
                skidData[i] = noise * Mathf.Sin(2f * Mathf.PI * 850f * t);
            }
            synthSkidClip = AudioClip.Create("SynthSkid", skidSamples, 1, sampleRate, false);
            synthSkidClip.SetData(skidData, 0);

            // 3. Crash Impact (noise punch + sub bass decay)
            float crashDuration = 0.35f;
            int crashSamples = Mathf.RoundToInt(sampleRate * crashDuration);
            float[] crashData = new float[crashSamples];
            for (int i = 0; i < crashSamples; i++)
            {
                float t = (float)i / crashSamples;
                float env = Mathf.Exp(-t * 8f);
                float sub = Mathf.Sin(2f * Mathf.PI * 90f * ((float)i / sampleRate));
                float noise = (Random.value * 2f - 1f);
                crashData[i] = (sub * 0.6f + noise * 0.4f) * env;
            }
            synthCrashClip = AudioClip.Create("SynthCrash", crashSamples, 1, sampleRate, false);
            synthCrashClip.SetData(crashData, 0);

            // 4. Boost Surge
            float boostDuration = 0.6f;
            int boostSamples = Mathf.RoundToInt(sampleRate * boostDuration);
            float[] boostData = new float[boostSamples];
            for (int i = 0; i < boostSamples; i++)
            {
                float t = (float)i / sampleRate;
                float freq = Mathf.Lerp(250f, 650f, (float)i / boostSamples);
                float env = Mathf.Sin(Mathf.PI * ((float)i / boostSamples));
                boostData[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * env * 0.5f;
            }
            synthBoostClip = AudioClip.Create("SynthBoost", boostSamples, 1, sampleRate, false);
            synthBoostClip.SetData(boostData, 0);

            // 5. UI Click
            float clickDuration = 0.08f;
            int clickSamples = Mathf.RoundToInt(sampleRate * clickDuration);
            float[] clickData = new float[clickSamples];
            for (int i = 0; i < clickSamples; i++)
            {
                float t = (float)i / sampleRate;
                clickData[i] = Mathf.Sin(2f * Mathf.PI * 1200f * t) * Mathf.Exp(-t * 40f) * 0.4f;
            }
            synthUiClickClip = AudioClip.Create("SynthClick", clickSamples, 1, sampleRate, false);
            synthUiClickClip.SetData(clickData, 0);

            // 6. Perfect Exit Chime
            float perfDuration = 0.3f;
            int perfSamples = Mathf.RoundToInt(sampleRate * perfDuration);
            float[] perfData = new float[perfSamples];
            for (int i = 0; i < perfSamples; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Exp(-((float)i / perfSamples) * 4f);
                perfData[i] = (Mathf.Sin(2f * Mathf.PI * 880f * t) + Mathf.Sin(2f * Mathf.PI * 1320f * t) * 0.5f) * env * 0.4f;
            }
            synthPerfectClip = AudioClip.Create("SynthPerfect", perfSamples, 1, sampleRate, false);
            synthPerfectClip.SetData(perfData, 0);

            // Assign defaults
            engineSource.clip = customEngineClip != null ? customEngineClip : synthEngineClip;
            skidSource.clip = customSkidClip != null ? customSkidClip : synthSkidClip;
        }

        public void StartEngine()
        {
            if (engineSource != null && !engineSource.isPlaying)
                engineSource.Play();
            if (skidSource != null && !skidSource.isPlaying)
                skidSource.Play();
        }

        public void StopEngine()
        {
            if (engineSource != null) engineSource.Stop();
            if (skidSource != null) skidSource.Stop();
        }

        public void UpdateEnginePitch(float speedRatio)
        {
            if (engineSource != null)
            {
                engineSource.pitch = Mathf.Lerp(0.85f, 2.2f, speedRatio);
            }
        }

        public void SetSkidVolume(float volume)
        {
            if (skidSource != null)
            {
                skidSource.volume = Mathf.Clamp01(volume * 0.6f);
            }
        }

        public void PlayBoost()
        {
            AudioClip clip = customBoostClip != null ? customBoostClip : synthBoostClip;
            boostSource.PlayOneShot(clip);
        }

        public void PlayCrash()
        {
            AudioClip clip = customCrashClip != null ? customCrashClip : synthCrashClip;
            sfxSource.PlayOneShot(clip);
        }

        public void PlayUiClick()
        {
            AudioClip clip = customUiClickClip != null ? customUiClickClip : synthUiClickClip;
            sfxSource.PlayOneShot(clip);
        }

        public void PlayPerfectExit()
        {
            AudioClip clip = customPerfectExitClip != null ? customPerfectExitClip : synthPerfectClip;
            sfxSource.PlayOneShot(clip);
        }
    }
}
