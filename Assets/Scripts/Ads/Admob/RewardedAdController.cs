using System;
using GoogleMobileAds.Api;
using UnityEngine;

namespace GoogleMobileAds.Samples
{
    public class RewardedAdController
    {
#if UNITY_ANDROID
        private const string _adUnitId = "ca-app-pub-3940256099942544/5224354917";
#elif UNITY_IPHONE
        private const string _adUnitId = "ca-app-pub-3940256099942544/1712485313";
#else
        private const string _adUnitId = "unused";
#endif

        private RewardedAd _rewardedAd;
        public bool IsCanShowAd => _rewardedAd != null && _rewardedAd.CanShowAd();

        /// <summary>
        ///     광고를 로드합니다.
        /// </summary>
        public void LoadAd()
        {
            if (_rewardedAd != null) DestroyAd();

            Debug.Log("보상형 광고를 로드 중입니다.");

            var adRequest = new AdRequest();
            RewardedAd.Load(_adUnitId, adRequest, (ad, error) =>
            {
                if (error != null)
                {
                    Debug.LogError("보상형 광고에 오류가 발생하여 광고를 로드하지 못했습니다: " + error);
                    return;
                }

                if (ad == null)
                {
                    Debug.LogError("예기치 않은 오류가 발생했습니다: 보상 로드 이벤트가 널 광고 및 널 오류와 함께 실행되었습니다.");
                    return;
                }

                Debug.Log("반응이 있는 보상형 광고 : " + ad.GetResponseInfo());
                _rewardedAd = ad;

                RegisterEventHandlers(ad);
            });
        }

        /// <summary>
        ///     광고를 표시합니다.
        /// </summary>
        public void ShowAd(Action<Reward> onRewarded)
        {
            if (IsCanShowAd)
            {
                Debug.Log("보상형 광고 표시.");
                _rewardedAd.Show(reward =>
                {
                    Debug.Log($"보상형 광고에 보상이 부여됨: {reward.Amount} {reward.Type}");
                    onRewarded?.Invoke(reward);
                });
            }
            else
            {
                Debug.LogError("보상형 광고가 아직 준비되지 않았습니다.");
                onRewarded?.Invoke(null);
            }
        }

        /// <summary>
        ///     광고를 삭제합니다.
        /// </summary>
        public void DestroyAd()
        {
            if (_rewardedAd == null) return;

            Debug.Log("보상형 광고 삭제하기.");
            _rewardedAd.Destroy();
            _rewardedAd = null;
        }

        /// <summary>
        ///     응답 정보를 기록합니다.
        /// </summary>
        public void LogResponseInfo()
        {
            if (_rewardedAd == null) return;

            var responseInfo = _rewardedAd.GetResponseInfo();
            Debug.Log(responseInfo);
        }

        private void RegisterEventHandlers(RewardedAd ad)
        {
            // 광고가 수익을 올린 것으로 추정될 때 발생합니다.
            ad.OnAdPaid += adValue => { Debug.Log($"보상형 광고 수익 발생 : {adValue.Value} {adValue.CurrencyCode}."); };

            // 광고에 대한 노출이 기록될 때 발생합니다.
            ad.OnAdImpressionRecorded += () => { Debug.Log("보상형 광고가 노출을 기록했습니다."); };

            // 광고 클릭이 기록될 때 발생합니다.
            ad.OnAdClicked += () => { Debug.Log("보상형 광고를 클릭했습니다."); };

            // 광고가 전체 화면 콘텐츠를 열었을 때 발생합니다.
            ad.OnAdFullScreenContentOpened += () => { Debug.Log("보상형 광고 전체 화면 콘텐츠가 열렸습니다."); };

            // 광고가 전체 화면 콘텐츠를 닫을 때 발생합니다.
            ad.OnAdFullScreenContentClosed += () => { Debug.Log("보상형 광고 전체 화면 콘텐츠가 닫혔습니다."); };

            // 광고가 전체 화면 콘텐츠를 열지 못했을 때 발생합니다.
            ad.OnAdFullScreenContentFailed += error =>
            {
                Debug.LogError("보상형 광고가 오류로 전체 화면 콘텐츠를 열지 못했습니다: "
                               + error);
            };
        }
    }
}