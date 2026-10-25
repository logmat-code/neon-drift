using System;
using UnityEngine;
using NeonDrift.Progression;
#if YODO1_MAS_INSTALLED || UNITY_ANDROID || UNITY_IOS || UNITY_EDITOR
using Yodo1.MAS;
#endif

namespace NeonDrift.Monetization
{
    public class Yodo1AdsManager : MonoBehaviour
    {
        public static Yodo1AdsManager Instance { get; private set; }

        public const string APP_KEY = "PLd7aFzM4Z";
        public const string ADMOB_APP_ID = "ca-app-pub-5784311004955543~4555927254";

        public event Action<bool> OnRewardedAdCompleted;

        public bool isInitialized { get; private set; } = false;

        private bool _rewardedInFlight = false;
        private bool _rewardEarned = false;
        private Action<bool> _currentRewardCallback = null;

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

            try
            {
                if (Yodo1U3dMasCallback.isInitialized())
                {
                    isInitialized = true;
                    SubscribeRewardedEvents();
                    PreloadRewardedAd();
                    return;
                }

                // Configure callbacks
                Yodo1U3dMasCallback.OnSdkInitializationEvent += OnSdkInitializationCompleted;

                // Subscribe strictly to Rewarded Ad Events (No banner / interstitial)
                SubscribeRewardedEvents();

                // Privacy compliance settings
                Yodo1U3dMas.SetCOPPA(false);
                Yodo1U3dMas.SetGDPR(true);
                Yodo1U3dMas.SetCCPA(false);

                var buildConfig = new Yodo1AdBuildConfig()
                    .enableUserPrivacyDialog(false)
                    .enableUserMessageingPlatform(Yodo1MasUMPState.ENABLE)
                    .enableATTAuthorization(true);
                Yodo1U3dMas.SetAdBuildConfig(buildConfig);

                Yodo1U3dMas.InitializeMasSdk();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Yodo1AdsManager] MAS Init exception: {ex.Message}");
                #if UNITY_EDITOR
                isInitialized = true;
                #endif
            }
        }

        private void OnSdkInitializationCompleted(Yodo1MasSdkConfiguration configuration, Yodo1U3dAdError error)
        {
            if (configuration != null)
            {
                isInitialized = true;
                Debug.Log("[Yodo1AdsManager] Official Yodo1 MAS SDK Initialized successfully.");
                PreloadRewardedAd();
            }
            else
            {
                isInitialized = false;
                Debug.LogError($"[Yodo1AdsManager] MAS Init Failed: {error?.ToString()}");
            }
        }

        private void SubscribeRewardedEvents()
        {
            var rewardAd = Yodo1U3dRewardAd.GetInstance();
            rewardAd.OnAdLoadedEvent += OnRewardedLoaded;
            rewardAd.OnAdLoadFailedEvent += OnRewardedLoadFailed;
            rewardAd.OnAdOpenedEvent += OnRewardedOpened;
            rewardAd.OnAdOpenFailedEvent += OnRewardedOpenFailed;
            rewardAd.OnAdClosedEvent += OnRewardedClosed;
            rewardAd.OnAdEarnedEvent += OnRewardedEarned;
        }

        public void PreloadRewardedAd()
        {
            if (!isInitialized) return;
            var rewardAd = Yodo1U3dRewardAd.GetInstance();
            if (!rewardAd.IsLoaded())
            {
                Debug.Log("[Yodo1AdsManager] Preloading next Rewarded Ad...");
                rewardAd.LoadAd();
            }
        }

        public bool IsRewardedAdLoaded()
        {
            if (!isInitialized) return false;
            return Yodo1U3dRewardAd.GetInstance().IsLoaded();
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
                else
                {
                    onRewardGranted?.Invoke(0);
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
                else
                {
                    onRewardGranted?.Invoke(0);
                }
            });
        }

        public void ShowRewardedAd(Action<bool> onResult)
        {
            if (_rewardedInFlight)
            {
                Debug.LogWarning("[Yodo1AdsManager] Rewarded Ad already in flight. Request ignored.");
                onResult?.Invoke(false);
                return;
            }

            if (!IsRewardedAdLoaded())
            {
                Debug.LogWarning("[Yodo1AdsManager] Rewarded ad not loaded or unavailable. No reward granted.");
                onResult?.Invoke(false);
                PreloadRewardedAd();
                return;
            }

            _rewardedInFlight = true;
            _rewardEarned = false;
            _currentRewardCallback = onResult;

            Debug.Log("[Yodo1AdsManager] Showing real Rewarded Video Ad via Yodo1 MAS...");
            Yodo1U3dRewardAd.GetInstance().ShowAd();
        }

        private void OnRewardedLoaded(Yodo1U3dRewardAd ad)
        {
            Debug.Log("[Yodo1AdsManager] Rewarded Ad ready.");
        }

        private void OnRewardedLoadFailed(Yodo1U3dRewardAd ad, Yodo1U3dAdError error)
        {
            Debug.LogWarning($"[Yodo1AdsManager] Rewarded Ad failed to load: {error?.ToString()}");
        }

        private void OnRewardedOpened(Yodo1U3dRewardAd ad)
        {
            Debug.Log("[Yodo1AdsManager] Rewarded Ad opened on screen.");
        }

        private void OnRewardedOpenFailed(Yodo1U3dRewardAd ad, Yodo1U3dAdError error)
        {
            Debug.LogError($"[Yodo1AdsManager] Rewarded Ad failed to open: {error?.ToString()}");
            FinishRewardedFlow(false);
        }

        private void OnRewardedEarned(Yodo1U3dRewardAd ad)
        {
            Debug.Log("[Yodo1AdsManager] OFFICIAL CALLBACK: Rewarded Ad earned!");
            _rewardEarned = true;
        }

        private void OnRewardedClosed(Yodo1U3dRewardAd ad)
        {
            Debug.Log($"[Yodo1AdsManager] Rewarded Ad closed. Reward earned: {_rewardEarned}");
            FinishRewardedFlow(_rewardEarned);
            PreloadRewardedAd();
        }

        private void FinishRewardedFlow(bool earned)
        {
            if (_rewardedInFlight)
            {
                _rewardedInFlight = false;
                var cb = _currentRewardCallback;
                _currentRewardCallback = null;
                cb?.Invoke(earned);
                OnRewardedAdCompleted?.Invoke(earned);
            }
        }
    }
}
