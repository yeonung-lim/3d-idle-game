using System.Collections;
using Core.StateMachine;
using Cysharp.Threading.Tasks;

namespace Ads
{
    /// <summary>
    ///     보상형 광고를 보여주는 상태
    /// </summary>
    public class RewardAdState : AbstractState
    {
        /// <summary>
        ///     보상을 받지 못했을 때 발생하는 이벤트
        /// </summary>
        private readonly AbstractGameEvent _noRewardEvent;

        /// <summary>
        ///     보상을 받았을 때 발생하는 이벤트 이름 (Analytics)
        /// </summary>
        private readonly string _rewardedEventName;

        /// <summary>
        ///     보상을 받았을 때 발생하는 이벤트
        /// </summary>
        private readonly AbstractGameEvent _rewardEvent;

        /// <summary>
        ///     보상을 받았는지 여부
        /// </summary>
        private bool _isRewarded;

        /// <summary>
        ///     광고를 보여주는 중인지 여부
        /// </summary>
        private bool _isShowing;

        /// <summary>
        ///     보상형 광고를 보여주는 상태
        /// </summary>
        /// <param name="rewardEvent">리워드를 받았을 때 발생하는 이벤트</param>
        /// <param name="noRewardEvent">리워드를 받지 못했을 때 발생하는 이벤트</param>
        /// <param name="rewardedEventName">리워드를 받았을 때 발생하는 이벤트 이름 (Analytics)</param>
        public RewardAdState(AbstractGameEvent rewardEvent, AbstractGameEvent noRewardEvent,
            string rewardedEventName = null)
        {
            _rewardEvent = rewardEvent;
            _noRewardEvent = noRewardEvent;
            _rewardedEventName = rewardedEventName;
        }

        public override string Name => $"{nameof(RewardAdState)}";

        /// <summary>
        ///     상태 진입
        /// </summary>
        public override void Enter()
        {
            _isShowing = true;
            _isRewarded = false;
        }

        /// <summary>
        ///     상태 실행
        /// </summary>
        /// <returns></returns>
        public override IEnumerator Execute()
        {
            ShowRewardedVideo().Forget();

            while (_isShowing)
                yield return null;

            // 광고를 보여주지 않았을 때는 위 대기가 이뤄지지 않으므로
            // 프레임 대기가 있어야 이벤트 발생이 정상적으로 이루어집니다.
            yield return null;

            // 보상을 받았을 때
            if (_isRewarded)
                _rewardEvent.Raise();
            else
                _noRewardEvent.Raise();
        }

        /// <summary>
        ///     보상형 광고를 보여줍니다.
        /// </summary>
        private async UniTaskVoid ShowRewardedVideo()
        {
            _isShowing = true;

            var result = await AdManager.Instance.ShowRewardedVideo();
            _isRewarded = result.IsRewarded;

            _isShowing = false;
        }

        public override void Exit()
        {
        }
    }
}