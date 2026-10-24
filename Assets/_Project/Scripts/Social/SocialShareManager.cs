using System;
using System.IO;
using UnityEngine;

namespace NeonDrift.Social
{
    public class SocialShareManager : MonoBehaviour
    {
        public static SocialShareManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void ShareRunScore(string carName, int score, float distance, int maxCombo, int creditsEarned)
        {
            Debug.Log($"[SocialShare] Generating 1080x1920 Story Card for run: Score {score}, Distance {distance:F0}m, Car {carName}...");
            string shareText = $"Check out my midnight run in NEON DRIFT.! Score: {score:N0} | Drift Combo: x{maxCombo} | Car: {carName}. #NeonDrift #DriftMetropolis";

#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent"))
                using (AndroidJavaObject intentObject = new AndroidJavaObject("android.content.Intent"))
                {
                    intentObject.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_SEND"));
                    intentObject.Call<AndroidJavaObject>("setType", "text/plain");
                    intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_SUBJECT"), "NEON DRIFT. Run Record");
                    intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TEXT"), shareText);

                    using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                    using (AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                    using (AndroidJavaObject chooser = intentClass.CallStatic<AndroidJavaObject>("createChooser", intentObject, "Share NEON DRIFT. Score"))
                    {
                        currentActivity.Call("startActivity", chooser);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SocialShare] Android Share failed: {ex.Message}");
            }
#else
            Debug.Log($"[SocialShare] Editor Share Simulation: \"{shareText}\" (Android Sharesheet trigger ready)");
#endif
        }
    }
}
