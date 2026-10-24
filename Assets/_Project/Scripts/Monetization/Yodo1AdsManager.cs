using System;
using UnityEngine;
using NeonDrift.Progression;

namespace NeonDrift.Monetization
{
    public class Yodo1AdsManager : MonoBehaviour
    {
        public static Yodo1AdsManager Instance { get; private set; }

        public const string APP_KEY = "PLd7aFzM4Z";
        public const string ADMOB_APP_ID = "ca-app-pub-5784311004955543~4555927254";

        public event Action<bool> OnRewardedAdCompleted;

        public bool isInitialized { get; private set; } = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSDK();
        }

        public void InitializeSDK()
        {
            Debug.Log($"[Yodo1AdsManager] Initializing MAS SDK with AppKey: {APP_KEY} (Rewarded Videos Only Mode)...");
            isInitialized = true;
        }

        public bool IsRewardedAdLoaded()
        {
            return isInitialized;
        }

        public void ShowDoubleCreditsRewardedAd(int baseCredits, Action<int> onRewardGranted)
        {
            ShowRewardedAd((success) =>
            {
                if (success)
                {
                    ProgressionManager.Instance?.RewardRewardedAdNC(baseCredits);
                    onRewardGranted?.Invoke(baseCredits);
                }
            });
        }

        public void ShowGarageBonusRewardedAd(Action<int> onRewardGranted)
        {
            const int bonusNC = 250;
            ShowRewardedAd((success) =>
            {
                if (success)
                {
                    ProgressionManager.Instance?.RewardRewardedAdNC(bonusNC);
                    onRewardGranted?.Invoke(bonusNC);
                }
            });
        }

        public void ShowRewardedAd(Action<bool> onResult)
        {
            if (!IsRewardedAdLoaded())
            {
                Debug.LogWarning("[Yodo1AdsManager] Rewarded ad not available or offline.");
                onResult?.Invoke(false);
                return;
            }

            Debug.Log("[Yodo1AdsManager] Showing voluntary Rewarded Video Ad...");
            onResult?.Invoke(true);
            OnRewardedAdCompleted?.Invoke(true);
        }
    }
}
