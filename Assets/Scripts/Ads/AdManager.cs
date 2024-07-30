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

    /// <summary>
    ///     배너의 마지막 상태를 업데이트 해주는 클래스
    /// </summary>
    public class LastBannerPropertyUpdater
    {
        /// <summary>
        ///     처분 가능한 객체
        /// </summary>
        private IDisposable _disposable;

        /// <summary>
        ///     마지막 배너 속성
        /// </summary>
        public BannerProperty LastBannerProperty;

        /// <summary>
        ///     생성자에서 BannerProperty를 구독해 LastBannerProperty를 업데이트 합니다.
        /// </summary>
        public LastBannerPropertyUpdater()
        {
            _disposable = MessageBroker.Default.Receive<BannerProperty>().Subscribe(x => { LastBannerProperty = x; });
        }

        /// <summary>
        ///     소멸자에서 구독을 해제합니다.
        /// </summary>
        ~LastBannerPropertyUpdater()
        {
            _disposable?.Dispose();
            _disposable = null;
        }
    }

    /// <summary>
    ///     배너 속성
    /// </summary>
    public struct BannerProperty
    {
        /// <summary>
        ///     마지막 배너 속성 업데이트 클래스
        /// </summary>
        private static readonly LastBannerPropertyUpdater LastBannerPropertyUpdater = new();

        /// <summary>
        ///     배너 위치
        /// </summary>
        public readonly BannerPosition BannerPosition;

        /// <summary>
        ///     배너 크기
        /// </summary>
        public readonly Vector2 DeltaSize;

        /// <summary>
        ///     배너가 열려있는지 여부
        /// </summary>
        public readonly bool IsOpened;

        /// <summary>
        ///     생성자
        /// </summary>
        /// <param name="deltaSize">배너 크기</param>
        /// <param name="bannerPosition">배너 위치</param>
        /// <param name="isOpened">배너 열림 여부</param>
        public BannerProperty(Vector2 deltaSize, BannerPosition bannerPosition, bool isOpened)
        {
            DeltaSize = deltaSize;
            BannerPosition = bannerPosition;
            IsOpened = isOpened;
        }

        /// <summary>
        ///     마지막 배너 속성
        /// </summary>
        public static BannerProperty LastBannerProperty => LastBannerPropertyUpdater.LastBannerProperty;

        /// <summary>
        ///     배너가 닫혀있는 상태
        /// </summary>
        public static BannerProperty Closed => new(Vector2.zero, BannerPosition.BOTTOM, false);

        /// <summary>
        ///     배너가 열려있는 상태
        /// </summary>
        /// <param name="deltaSize">배너 크기</param>
        /// <param name="bannerPosition">배너 위치</param>
        /// <returns></returns>
        public static BannerProperty Open(Vector2 deltaSize, BannerPosition bannerPosition)
        {
            return new BannerProperty(deltaSize, bannerPosition, true);
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
        ///     전면 광고가 보여지고 있는지 여부
        /// </summary>
        private bool _isShowingInterstitial;

        /// <summary>
        ///     광고 제거 구독
        /// </summary>
        private IDisposable _removeAdsDisposable;

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

            var cancelToken = this.GetCancellationTokenOnDestroy();

            Advertisements.Instance.Initialize();
            await UniTask.WaitUntil(() => CustomAppLovin.IsInitializedComplete, cancellationToken: cancelToken);

            _isInitialized = true;

            RegisterOnAdRevenue();

            IAPController.Instance.SubscribeBought(removeAdsProduct, SetRemoveAds)?.AddTo(this);

            if (!HasRemoveAds)
                InitAndShowBanner();
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
        ///     배너를 초기화하고 보여줍니다.
        /// </summary>
        private void InitAndShowBanner()
        {
            Advertisements.Instance
                .ObserveEveryValueChanged(x => x.IsBannerOnScreen())
                .Where(x => !x)
                .Subscribe(x =>
                    PublishBannerProperty(BannerProperty.Closed)
                ).AddTo(this);

            ShowBanner();
        }

        /// <summary>
        ///     배너 속성을 게시합니다.
        /// </summary>
        /// <param name="bannerProperty"></param>
        private void PublishBannerProperty(BannerProperty bannerProperty)
        {
            MessageBroker.Default.Publish(bannerProperty);
        }

        /// <summary>
        ///     사용자 동의를 설정합니다.
        /// </summary>
        /// <param name="consent"></param>
        public void SetUserConsent(bool consent)
        {
            if (_isInitialized == false)
            {
                Debug.LogWarning("Ads not initialized");
                return;
            }

            Advertisements.Instance.SetUserConsent(consent);
        }

        /// <summary>
        ///     CCPA 동의를 설정합니다.
        /// </summary>
        /// <param name="consent"></param>
        public void SetCCPAConsent(bool consent)
        {
            if (_isInitialized == false)
            {
                Debug.LogWarning("Ads not initialized");
                return;
            }

            Advertisements.Instance.SetCCPAConsent(consent);
        }

        /// <summary>
        ///     배너 광고를 보여줍니다.
        /// </summary>
        /// <param name="position"></param>
        public void ShowBanner(BannerPosition position = BannerPosition.BOTTOM)
        {
            if (_isInitialized == false)
            {
                Debug.LogWarning("Ads not initialized");
                return;
            }

            Advertisements.Instance.ShowBanner(position, BannerType.Adaptive);
        }

        /// <summary>
        ///     배너 광고를 숨깁니다.
        /// </summary>
        public void HideBanner()
        {
            if (_isInitialized == false)
            {
                Debug.LogWarning("Ads not initialized");
                return;
            }

            Advertisements.Instance.HideBanner();
        }

        /// <summary>
        ///     전면 광고를 보여줍니다.
        /// </summary>
        public async UniTask ShowInterstitial()
        {
            if (_isInitialized == false)
            {
                Debug.LogWarning("Ads not initialized");
                return;
            }

            if (Advertisements.Instance.IsInterstitialAvailable() == false)
            {
                Debug.LogWarning("Interstitial not available");
                return;
            }

            var cancelToken = this.GetCancellationTokenOnDestroy();
            var closed = false;

            ResetAudioPrepare();

            Advertisements.Instance.ShowInterstitial(result =>
            {
                Debug.Log("Interstitial closed");
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
                Debug.LogWarning("Ads not initialized");
                return new RewardAdsResult(false, false);
            }

            if (Advertisements.Instance.IsRewardVideoAvailable() == false)
            {
                Debug.LogWarning("Rewarded video not available");
                return new RewardAdsResult(false, false);
            }

            var cancelToken = this.GetCancellationTokenOnDestroy();
            var closed = false;
            var isRewarded = false;

            ResetAudioPrepare();

            Advertisements.Instance.ShowRewardedVideo(result =>
            {
                Debug.Log("Rewarded video closed");
                closed = true;
                isRewarded = result;
            });

            await UniTask.WaitUntil(() => closed, cancellationToken: cancelToken);

            ResetAudio();

            return new RewardAdsResult(true, isRewarded);
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
                Debug.LogWarning("Ads not initialized");
                return;
            }

            Advertisements.Instance.RemoveAds(removeAds);
        }
    }
}