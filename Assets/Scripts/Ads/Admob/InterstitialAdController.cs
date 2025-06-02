using System;
using GoogleMobileAds.Api;
using UnityEngine;

namespace GoogleMobileAds.Samples
{
    public class InterstitialAdController
    {
#if UNITY_ANDROID
        private const string _adUnitId = "ca-app-pub-3940256099942544/1033173712";
#elif UNITY_IPHONE
        private const string _adUnitId = "ca-app-pub-3940256099942544/4411468910";
#else
        private const string _adUnitId = "unused";
#endif

        private InterstitialAd _interstitialAd;

        private readonly Action _onAdOpened;
        private readonly Action _onAdClosed;

        internal InterstitialAdController(Action onAdOpened, Action onAdClosed)
        {
            _onAdOpened = onAdOpened;
            _onAdClosed = onAdClosed;
        }

        /// <summary>
        ///     광고를 로드합니다.
        /// </summary>
        public void LoadAd()
        {
            // 새 광고를 로드하기 전에 이전 광고를 정리합니다.
            if (_interstitialAd != null) DestroyAd();

            Debug.Log("전면 광고를 로드 중입니다.");

            // 광고를 로드하는 데 사용되는 요청을 생성합니다.
            var adRequest = new AdRequest();

            // 광고 로딩 요청을 보냅니다.
            InterstitialAd.Load(_adUnitId, adRequest, (ad, error) =>
            {
                // 작업이 이유와 함께 실패한 경우.
                if (error != null)
                {
                    Debug.LogError("전면 광고가 오류로 광고를 로드하지 못했습니다: " + error);
                    return;
                }

                // 알 수 없는 이유로 작업이 실패한 경우.
                // 예기치 않은 오류이므로 이 버그가 발생하면 신고해 주세요.
                if (ad == null)
                {
                    Debug.LogError("예기치 않은 오류가 발생했습니다: 널 광고 및 널 오류와 함께 삽입 광고 로드 이벤트가 실행되었습니다.");
                    return;
                }

                // 작업이 성공적으로 완료되었습니다.
                Debug.Log("전면 광고가 응답과 함께 로드되었습니다 : " + ad.GetResponseInfo());
                _interstitialAd = ad;

                // 광고 이벤트에 등록하여 기능을 확장하세요.
                RegisterEventHandlers(ad);
            });
        }

        /// <summary>
        ///     광고를 표시합니다.
        /// </summary>
        public void ShowAd()
        {
            if (IsCanShowAd)
            {
                Debug.Log("전면 광고 표시.");
                _interstitialAd.Show();
            }
            else
            {
                Debug.LogError("전면 광고는 아직 준비되지 않았습니다.");
            }
        }

        public bool IsCanShowAd => _interstitialAd != null && _interstitialAd.CanShowAd();

        /// <summary>
        ///     광고를 삭제합니다.
        /// </summary>
        public void DestroyAd()
        {
            if (_interstitialAd == null) return;

            Debug.Log("전면 광고 삭제하기.");
            _interstitialAd.Destroy();
            _interstitialAd = null;
        }

        /// <summary>
        ///     응답 정보를 기록합니다.
        /// </summary>
        public void LogResponseInfo()
        {
            if (_interstitialAd == null) return;

            var responseInfo = _interstitialAd.GetResponseInfo();
            Debug.Log(responseInfo);
        }

        private void RegisterEventHandlers(InterstitialAd ad)
        {
            // 광고가 수익을 올린 것으로 추정될 때 발생합니다.
            ad.OnAdPaid += adValue => { Debug.Log($"전면 광고 수익: {adValue.Value} {adValue.CurrencyCode}."); };

            // 광고에 대한 노출이 기록될 때 발생합니다.
            ad.OnAdImpressionRecorded += () => { Debug.Log("전면 광고 노출을 기록했습니다."); };

            // 광고 클릭이 기록될 때 발생합니다.
            ad.OnAdClicked += () => { Debug.Log("전면 광고가 클릭되었습니다."); };

            // 광고가 전체 화면 콘텐츠를 열었을 때 발생합니다.
            ad.OnAdFullScreenContentOpened += () =>
            {
                _onAdOpened?.Invoke();
                Debug.Log("전면 광고 전체 화면 콘텐츠가 열렸습니다.");
            };
            // 광고가 전체 화면 콘텐츠를 닫을 때 발생합니다.
            ad.OnAdFullScreenContentClosed += () =>
            {
                _onAdClosed?.Invoke();
                Debug.Log("전면 광고 전체 화면 콘텐츠가 닫혔습니다.");
            };

            // 광고가 전체 화면 콘텐츠를 열지 못했을 때 발생합니다.
            ad.OnAdFullScreenContentFailed += error =>
            {
                Debug.LogError("전면 광고가 오류와 함께 전체 화면 콘텐츠를 열지 못했습니다 : "
                               + error);
            };
        }
    }
}