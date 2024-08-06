using System;

namespace Ads
{
    public interface IAdController
    {
        public void Initialize(Action<bool> onInitialized);

        public bool IsRewardAdAvailable();
        public void LoadRewardAd();
        public void ShowRewardAd(Action<RewardAdsResult> onResult);

        public bool IsInterstitialAdAvailable();
        public void LoadInterstitialAd();
        public void ShowInterstitialAd(Action<InterstitialAdsResult> onResult);
    }
}