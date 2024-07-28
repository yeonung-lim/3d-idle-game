using Cysharp.Threading.Tasks;
using Data;
using UnityEngine;
using USingleton;
#if UNITY_ANDROID
using Google.Play.Review;
#endif

namespace Review
{
    /// <summary>
    ///     스토어 리뷰 관리자입니다.
    /// </summary>
    public static class StoreReview
    {
        /// <summary>
        ///     저장 관리자가 로드되었는지 여부
        /// </summary>
        public static bool IsSaveManagerLoaded => Singleton.HasInstance<SaveManager>() &&
                                                  Singleton.Instance<SaveManager>().GetAsyncOperation().IsDone;

        /// <summary>
        ///     리뷰가 가능한지 여부
        /// </summary>
        public static bool IsAvailableReview => IsSaveManagerLoaded &&
                                                StaticData.Review.IsReviewed == false;

#if UNITY_ANDROID
        /// <summary>
        ///     리뷰 요청을 수행합니다.
        /// </summary>
        /// <returns></returns>
        private static async UniTask<(bool, ReviewManager, PlayReviewInfo)> RequestReviewFlow()
        {
            var reviewManager = new ReviewManager();

            var requestFlowOperation = reviewManager.RequestReviewFlow();
            await requestFlowOperation;

            if (requestFlowOperation.Error != ReviewErrorCode.NoError)
            {
                Debug.LogError(requestFlowOperation.Error.ToString());
                return (false, reviewManager, null);
            }

            var playReviewInfo = requestFlowOperation.GetResult();
            return (true, reviewManager, playReviewInfo);
        }
#endif

        /// <summary>
        ///     리뷰 플로우를 시작합니다.
        /// </summary>
        /// <returns></returns>
        public static async UniTask<bool> LaunchReviewFlow()
        {
            if (IsAvailableReview == false) return false;

            var launchSuccess = false;

#if UNITY_ANDROID
            var (requestSuccess, reviewManager, playReviewInfo) = await RequestReviewFlow();
            if (requestSuccess == false) return false;

            var launchFlowOperation = reviewManager.LaunchReviewFlow(playReviewInfo);
            await launchFlowOperation;

            launchSuccess = launchFlowOperation.Error == ReviewErrorCode.NoError;
            if (launchSuccess == false)
                Debug.LogError(launchFlowOperation.Error.ToString());

#elif UNITY_IOS
            launchSuccess = UnityEngine.iOS.Device.RequestStoreReview();
#endif

            if (launchSuccess) StaticData.Review.IsReviewed = true;
            return launchSuccess;
        }
    }
}