using System;
using UnityEngine;
using GoogleMobileAds.Api;

namespace GoogleMobileAds.Sample
{
    /// <summary>
    /// Demonstrates how to use Google Mobile Ads rewarded interstitial ads.
    /// </summary>
    [AddComponentMenu("GoogleMobileAds/Samples/RewardedInterstitialAdController")]
    public class RewardedInterstitialAdController : MonoBehaviour
    {
        /// <summary>
        /// UI element activated when an ad is ready to show.
        /// </summary>
        public GameObject AdLoadedStatus;

        // These ad units are configured to always serve test ads.
#if UNITY_ANDROID
        private const string _adUnitId = "ca-app-pub-3940256099942544/5354046379";
#elif UNITY_IPHONE
        private const string _adUnitId = "ca-app-pub-3940256099942544/6978759866";
#else
        private const string _adUnitId = "unused";
#endif

        private RewardedInterstitialAd _rewardedInterstitialAd;

        /// <summary>
        /// Loads the ad.
        /// </summary>
        public void LoadAd()
        {
            // Clean up the old ad before loading a new one.
            if (_rewardedInterstitialAd != null)
            {
                DestroyAd();
            }

            Debug.Log("보상형 전면 광고를 로드 중입니다.");

            // Create our request used to load the ad.
            var adRequest = new AdRequest();

            // Send the request to load the ad.
            RewardedInterstitialAd.Load(_adUnitId, adRequest,
                (RewardedInterstitialAd ad, LoadAdError error) =>
                {
                    // If the operation failed with a reason.
                    if (error != null)
                    {
                        Debug.LogError("보상형 전면 광고가 오류로 광고를 로드하지 못했습니다: "
                                        + error);
                        return;
                    }
                    // If the operation failed for unknown reasons.
                    // This is an unexpexted error, please report this bug if it happens.
                    if (ad == null)
                    {
                        Debug.LogError("예기치 않은 오류: 보상형 전면 로드 이벤트가 널 광고 및 널 오류와 함께 실행되었습니다.");
                        return;
                    }

                    // The operation completed successfully.
                    Debug.Log("보상형 전면 광고가 응답과 함께 로드되었습니다: "
                        + ad.GetResponseInfo());
                    _rewardedInterstitialAd = ad;

                    // Register to ad events to extend functionality.
                    RegisterEventHandlers(ad);

                    // Inform the UI that the ad is ready.
                    AdLoadedStatus?.SetActive(true);
                });
        }

        /// <summary>
        /// Shows the ad.
        /// </summary>
        public void ShowAd()
        {
            if (_rewardedInterstitialAd != null && _rewardedInterstitialAd.CanShowAd())
            {
                _rewardedInterstitialAd.Show((Reward reward) =>
                {
                    Debug.Log("보상형 전면 광고 보상: " + reward.Amount);
                });
            }
            else
            {
                Debug.LogError("보상형 전면 광고가 아직 준비되지 않았습니다.");
            }

            // Inform the UI that the ad is not ready.
            AdLoadedStatus?.SetActive(false);
        }

        /// <summary>
        /// Destroys the ad.
        /// </summary>
        public void DestroyAd()
        {
            if (_rewardedInterstitialAd != null)
            {
                Debug.Log("보상형 전면 광고를 제거합니다.");
                _rewardedInterstitialAd.Destroy();
                _rewardedInterstitialAd = null;
            }

            // Inform the UI that the ad is not ready.
            AdLoadedStatus?.SetActive(false);
        }

        /// <summary>
        /// Logs the ResponseInfo.
        /// </summary>
        public void LogResponseInfo()
        {
            if (_rewardedInterstitialAd != null)
            {
                var responseInfo = _rewardedInterstitialAd.GetResponseInfo();
                UnityEngine.Debug.Log(responseInfo);
            }
        }

        protected void RegisterEventHandlers(RewardedInterstitialAd ad)
        {
            // Raised when the ad is estimated to have earned money.
            ad.OnAdPaid += (AdValue adValue) =>
            {
                Debug.Log(String.Format("보상형 전면 광고 수익 {0} {1}.",
                    adValue.Value,
                    adValue.CurrencyCode));
            };
            // Raised when an impression is recorded for an ad.
            ad.OnAdImpressionRecorded += () =>
            {
                Debug.Log("보상형 전면 광고 노출이 기록되었습니다.");
            };
            // Raised when a click is recorded for an ad.
            ad.OnAdClicked += () =>
            {
                Debug.Log("보상형 전면 광고가 클릭되었습니다.");
            };
            // Raised when an ad opened full screen content.
            ad.OnAdFullScreenContentOpened += () =>
            {
                Debug.Log("보상형 전면 광고 전체 화면 콘텐츠가 열렸습니다.");
            };
            // Raised when the ad closed full screen content.
            ad.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("보상형 전면 광고 전체 화면 콘텐츠가 닫혔습니다.");
            };
            // Raised when the ad failed to open full screen content.
            ad.OnAdFullScreenContentFailed += (AdError error) =>
            {
                Debug.LogError("보상형 전면 광고가 오류로 전체 화면 콘텐츠를 열지 못했습니다: "
                               + error);
            };
        }
    }
}
