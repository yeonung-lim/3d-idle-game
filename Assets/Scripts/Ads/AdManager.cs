using System;
using System.Collections.Generic;
using AsyncInitialize;
using Cysharp.Threading.Tasks;
using IAP;
using UniRx;
using UnityCommunity.UnitySingleton;
using UnityEngine;

namespace Ads
{
    /// <summary>
    ///     젼면 광고 결과
    /// </summary>
    public struct InterstitialAdsResult
    {
        /// <summary>
        ///     광고가 보여졌는지 여부
        /// </summary>
        public readonly bool IsShown;

        /// <summary>
        ///     전면 광고 결과
        /// </summary>
        /// <param name="isShown"></param>
        public InterstitialAdsResult(bool isShown)
        {
            IsShown = isShown;
        }
    }

    /// <summary>
    ///     리워드 광고 결과
    /// </summary>
    public struct RewardAdsResult
    {
        /// <summary>
        ///     보상을 받았는지 여부
        /// </summary>
        public readonly bool IsRewarded;

        /// <summary>
        ///     광고가 보여졌는지 여부
        /// </summary>
        public readonly bool IsShown;

        public RewardAdsResult(bool isShown, bool isRewarded)
        {
            IsShown = isShown;
            IsRewarded = isRewarded;
        }
    }

    public class AdManager : PersistentMonoSingleton<AdManager>, IAsyncInit
    {
        /// <summary>
        ///     광고 제거 아이템의 상품 이름
        /// </summary>
        [SerializeField] private ShopProductNames removeAdsProduct;

        /// <summary>
        ///     테스트 디바이스 해시 ID 목록
        /// </summary>
        [SerializeField] private List<string> testDeviceHashedIds;

        /// <summary>
        ///     리셋 가능한 오디오 컨트롤러
        ///     비디오 광고가 끝난 후 오디오를 리셋하기 위해 사용됩니다
        /// </summary>
        private IResettableAudioController _audioController;

        /// <summary>
        ///     동의 정보가 업데이트되었는지 여부
        /// </summary>
        private bool _consentInfoUpdated;

        /// <summary>
        ///     초기화되었는지 여부
        /// </summary>
        private bool _isInitialized;

        /// <summary>
        ///     광고 제거가 되었는지 여부
        /// </summary>
        private bool _isRemoveAds;

        /// <summary>
        ///     전면 광고가 보여지고 있는지 여부
        /// </summary>
        private bool _isShowingInterstitial;

        /// <summary>
        ///     광고 제거 구독
        /// </summary>
        private IDisposable _removeAdsDisposable;

        /// <summary>
        ///     광고 컨트롤러
        /// </summary>
        private IAdController adController;

        /// <summary>
        ///     광고 제거 아이템을 가지고 있는지 여부
        /// </summary>
        public bool HasRemoveAds => IAPController.Instance.HasProduct(removeAdsProduct);

        /// <summary>
        ///     전면 광고 중 게임을 닫았는지 여부
        /// </summary>
        public bool IsForceCloseInterstitial
        {
            get => Convert.ToBoolean(PlayerPrefs.GetInt("AdManager.IsForceCloseInterstitial", 0));
            private set
            {
                PlayerPrefs.SetInt("AdManager.IsForceCloseInterstitial", Convert.ToInt32(value));
                PlayerPrefs.Save();
            }
        }

        /// <summary>
        ///     전면 광고 중 게임을 닫았는지 여부를 설정합니다
        /// </summary>
        /// <param name="hasFocus"></param>
        private void OnApplicationFocus(bool hasFocus)
        {
            if (!_isShowingInterstitial) return;

            IsForceCloseInterstitial = !hasFocus;
        }

        /// <summary>
        ///     초기화되었는지 여부를 반환합니다
        /// </summary>
        /// <returns></returns>
        public CustomizableAsyncOperation GetAsyncOperation()
        {
            return CustomizableAsyncOperation.Create(() => _isInitialized);
        }

        /// <summary>
        ///     비동기 작업을 시작합니다
        /// </summary>
        public async void StartInitialize()
        {
            if (_isInitialized)
                return;

            adController.LoadRewardAd();
            adController.LoadInterstitialAd();

            _isInitialized = true;

            IAPController.Instance.SubscribeBought(removeAdsProduct, SetRemoveAds)?.AddTo(this);
        }

        public void Reset()
        {
        }

        /// <summary>
        ///     오디오 컨트롤러를 설정합니다
        /// </summary>
        /// <param name="audioController"></param>
        public void SetAudioController(IResettableAudioController audioController)
        {
            _audioController = audioController;
        }

        /// <summary>
        ///     비디오 광고를 노출 하기 전 오디오 컨트롤러를 리셋 준비시킵니다.
        /// </summary>
        private void ResetAudioPrepare()
        {
            _audioController.ResetPrepare();
        }

        /// <summary>
        ///     비디오 광고가 끝난 후 오디오 컨트롤러를 리셋합니다.
        /// </summary>
        private void ResetAudio()
        {
            _audioController.ResetAudio();
        }

        /// <summary>
        ///     전면 광고를 보여줍니다.
        /// </summary>
        public async UniTask ShowInterstitial()
        {
            if (_isInitialized == false)
            {
                Debug.LogWarning("초기화가 되지 않았습니다.");
                return;
            }

            if (adController.IsInterstitialAdAvailable() == false)
            {
                Debug.LogWarning("전면 광고를 사용할 수 없습니다.");
                return;
            }

            var cancelToken = this.GetCancellationTokenOnDestroy();
            var closed = false;

            ResetAudioPrepare();

            adController.ShowInterstitialAd(result =>
            {
                Debug.Log("전면 광고 닫힘");
                closed = true;
            });

            _isShowingInterstitial = true;
            await UniTask.WaitUntil(() => closed, cancellationToken: cancelToken);
            IsForceCloseInterstitial = _isShowingInterstitial = false;

            ResetAudio();
        }

        /// <summary>
        ///     리워드 광고를 보여줍니다.
        /// </summary>
        /// <returns></returns>
        public async UniTask<RewardAdsResult> ShowRewardedVideo()
        {
            if (_isInitialized == false)
            {
                Debug.LogWarning("초기화가 되지 않았습니다.");
                return new RewardAdsResult(false, false);
            }

            if (adController.IsRewardAdAvailable() == false)
            {
                Debug.LogWarning("보상 광고를 사용할 수 없습니다.");
                return new RewardAdsResult(false, false);
            }

            var cancelToken = this.GetCancellationTokenOnDestroy();
            var closed = false;
            var rewardAdsResult = new RewardAdsResult(false, false);

            ResetAudioPrepare();

            adController.ShowRewardAd(result =>
            {
                Debug.Log("보상 광고 닫힘");
                rewardAdsResult = result;
                closed = true;
            });

            await UniTask.WaitUntil(() => closed, cancellationToken: cancelToken);

            ResetAudio();

            return rewardAdsResult;
        }

        /// <summary>
        ///     광고 제거를 설정합니다.
        ///     설정되면 배너/전면 광고가 보이지 않습니다.
        /// </summary>
        /// <param name="removeAds"></param>
        private void SetRemoveAds(bool removeAds)
        {
            if (_isInitialized == false)
            {
                Debug.LogWarning("초기화가 되지 않았습니다.");
                return;
            }

            _isRemoveAds = removeAds;
        }
    }
}