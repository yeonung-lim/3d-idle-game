using System;
using System.Collections.Generic;
using Ads;
using GoogleMobileAds.Api;
using GoogleMobileAds.Ump.Api;
using UnityEngine;

namespace GoogleMobileAds.Samples
{
    public class GoogleMobileAdsController : MonoBehaviour, IAdController
    {
        internal static List<string> TestDeviceIds = new()
        {
            AdRequest.TestDeviceSimulator,
#if UNITY_IPHONE
            "96e23e80653bb28980d3f40beb58915c",
#elif UNITY_ANDROID
            "702815ACFC14FF222DA1DC767672A573"
#endif
        };

        private static bool? _isInitialized;

        [SerializeField] [Tooltip("Google 사용자 메시징 플랫폼(UMP) Unity 플러그인용 컨트롤러입니다.")]
        private GoogleMobileAdsConsentController _consentController;

        private Action<bool> _onInitialized;

        private Action<InterstitialAdsResult> _onInterstitialAdResult;

        /// <summary>
        ///     전면 광고 컨트롤러입니다.
        /// </summary>
        private InterstitialAdController interstitialAdController;

        /// <summary>
        ///     보상 광고 컨트롤러입니다.
        /// </summary>
        private RewardedAdController rewardAdController;

        public bool IsInterstitialAdAvailable()
        {
            return interstitialAdController.IsCanShowAd;
        }

        public void Initialize(Action<bool> onInitialized)
        {
            if (_isInitialized == true)
            {
                onInitialized?.Invoke(true);
                return;
            }

            _onInitialized = onInitialized;
            interstitialAdController = new InterstitialAdController(OnInterstitialAdOpened, OnInterstitialAdClosed);
            rewardAdController = new RewardedAdController();

            Initialize();
        }

        public bool IsRewardAdAvailable()
        {
            if (rewardAdController == null)
            {
                Debug.LogError("보상형 광고 컨트롤러가 없습니다.");
                return false;
            }

            return rewardAdController.IsCanShowAd;
        }

        public void LoadRewardAd()
        {
            if (rewardAdController == null)
            {
                Debug.LogError("보상형 광고 컨트롤러가 없습니다.");
                return;
            }

            rewardAdController.LoadAd();
        }

        public void ShowRewardAd(Action<RewardAdsResult> onResult)
        {
            if (IsRewardAdAvailable() == false)
            {
                onResult?.Invoke(new RewardAdsResult(false, false));
                return;
            }

            rewardAdController.ShowAd(reward =>
            {
                onResult?.Invoke(
                    reward == null ? new RewardAdsResult(true, false) : new RewardAdsResult(true, true));
            });
        }

        public void LoadInterstitialAd()
        {
            if (IsInterstitialAdAvailable()) return;

            interstitialAdController.LoadAd();
        }

        public void ShowInterstitialAd(Action<InterstitialAdsResult> onResult)
        {
            if (_isInitialized == false)
            {
                Debug.LogWarning("초기화되지 않은 광고");
                onResult?.Invoke(new InterstitialAdsResult(false));
                return;
            }

            if (IsInterstitialAdAvailable() == false)
            {
                Debug.LogWarning("전면 광고를 사용할 수 없습니다.");
                onResult?.Invoke(new InterstitialAdsResult(false));
                return;
            }

            interstitialAdController.ShowAd();
            _onInterstitialAdResult = onResult;
        }

        private void OnInterstitialAdClosed()
        {
            _onInterstitialAdResult?.Invoke(new InterstitialAdsResult(true));
            _onInterstitialAdResult = null;
        }

        private void OnInterstitialAdOpened()
        {
        }

        public void Initialize()
        {
            MobileAds.SetiOSAppPauseOnBackground(true);
            MobileAds.RaiseAdEventsOnUnityMainThread = true;
            MobileAds.SetRequestConfiguration(new RequestConfiguration
            {
                TestDeviceIds = TestDeviceIds
            });

            if (_consentController.CanRequestAds) InitializeGoogleMobileAds();

            InitializeGoogleMobileAdsConsent();
        }

        /// <summary>
        ///     개인정보 및 동의 정보가 최신 상태인지 확인합니다.
        /// </summary>
        private void InitializeGoogleMobileAdsConsent()
        {
            Debug.Log("Google 모바일 광고 수집 동의");

            _consentController.GatherConsent(error =>
            {
                if (error != null)
                    Debug.LogError("오류로 동의를 수집하지 못했습니다: " +
                                   error);
                else
                    Debug.Log("Google 모바일 광고 동의가 업데이트되었습니다: "
                              + ConsentInformation.ConsentStatus);

                if (_consentController.CanRequestAds) InitializeGoogleMobileAds();
            });
        }

        /// <summary>
        ///     구글 모바일 광고 유니티 플러그인을 초기화합니다.
        /// </summary>
        private void InitializeGoogleMobileAds()
        {
            // Google 모바일 광고 유니티 플러그인은 광고를 로드하기 전에 한 번만 실행하면 됩니다.
            if (_isInitialized.HasValue) return;

            _isInitialized = false;

            // 구글 모바일 광고 유니티 플러그인을 초기화합니다.
            Debug.Log("Google 모바일 광고 초기화 중입니다.");
            MobileAds.Initialize(initStatus =>
            {
                if (initStatus == null)
                {
                    Debug.LogError("Google 모바일 광고 초기화에 실패했습니다.");
                    _isInitialized = null;
                    _onInitialized?.Invoke(false);
                    return;
                }

                // 중개를 사용하는 경우 각 어댑터의 상태를 확인할 수 있습니다.
                var adapterStatusMap = initStatus.getAdapterStatusMap();
                if (adapterStatusMap != null)
                    foreach (var item in adapterStatusMap)
                        Debug.Log($"어댑터 {item.Key}는 {item.Value.InitializationState}입니다");

                Debug.Log("Google 모바일 광고 초기화가 완료되었습니다.");
                _isInitialized = true;

                _onInitialized?.Invoke(true);
            });
        }

        /// <summary>
        ///     광고 인스펙터를 엽니다.
        /// </summary>
        public void OpenAdInspector()
        {
            Debug.Log("광고 인스펙터를 엽니다.");
            MobileAds.OpenAdInspector(error =>
            {
                // 작업이 실패하면 오류가 반환됩니다.
                if (error != null)
                {
                    Debug.Log("광고 인스펙터를 열지 못했습니다: " + error);
                    return;
                }

                Debug.Log("광고 인스펙터가 성공적으로 열렸습니다.");
            });
        }

        /// <summary>
        ///     사용자의 개인정보 보호 옵션 양식을 엽니다.
        /// </summary>
        /// <remarks>
        ///     앱은 사용자가 언제든지 동의 상태를 변경할 수 있도록 허용해야 합니다.
        /// </remarks>
        public void OpenPrivacyOptions()
        {
            _consentController.ShowPrivacyOptionsForm(error =>
            {
                if (error != null)
                    Debug.LogError("동의 개인정보 보호 양식을 표시하지 못했습니다: " +
                                   error);
                else
                    Debug.Log("개인정보 양식이 성공적으로 열렸습니다.");
            });
        }
    }
}