using AsyncInitialize;
using Cysharp.Threading.Tasks;
using UI.Loading;
using UnityEngine;
using State = Core.StateMachine.State;

namespace Sequence
{
    /// <summary>
    ///     게임 진행을 관리하는 클래스
    /// </summary>
    public class SequenceManager : MonoBehaviour
    {
        /// <summary>
        ///     초기화 시 로딩 할 리소스들
        ///     순차적으로 로딩됌
        /// </summary>
        [SerializeField] private MonoAsyncInit[] _asyncLoadingResources;

        [Header("Events")] [SerializeField] private AbstractGameEvent continueEvent; // 계속하기 이벤트
        [SerializeField] private AbstractGameEvent backEvent; // 뒤로가기 이벤트
        [SerializeField] private AbstractGameEvent winEvent; // 승리 클리어 이벤트
        [SerializeField] private AbstractGameEvent loseEvent; // 승리 실패 이벤트
        [SerializeField] private AbstractGameEvent pauseEvent; // 일시정지 이벤트

        /// <summary>
        ///     메인 메뉴 상태
        /// </summary>
        private IState _mainMenuState;

        /// <summary>
        ///     상태 관리자
        /// </summary>
        private StateMachine _stateMachine;

        /// <summary>
        ///     초기화
        /// </summary>
        public void Initialize()
        {
            var loadingState = CreateLoadingState(); // Loading UI를 띄우고, 리소스를 로딩합니다.
            _mainMenuState = CreateMainMenuState();

            loadingState.AddLink(new EventLink(continueEvent, _mainMenuState));

            _stateMachine = new StateMachine();
            _stateMachine.Run(loadingState);
        }

        /// <summary>
        ///     리소스를 로드하는 상태를 생성합니다.
        ///     UI 로딩 화면을 띄우고, 리소스를 로드합니다.
        /// </summary>
        /// <returns></returns>
        private IState CreateLoadingState()
        {
            return new State(() =>
            {
                var indicator = new LoadingIndicator();

                // ShowUI<LoadingView, LoadingViewSetting>(
                //     new LoadingViewSetting("loading_title", "loading_text", indicator));

                LoadingResources(_asyncLoadingResources, indicator).ContinueWith(() => { continueEvent.Raise(); })
                    .Forget();
            });
        }

        /// <summary>
        ///     IAsyncInit를 상속받은 클래스들을 로드합니다.
        /// </summary>
        /// <param name="asyncLoadingResources">로딩 할 리소스</param>
        /// <param name="indicator">로딩 인디케이터</param>
        private async UniTask LoadingResources(MonoAsyncInit[] asyncLoadingResources, LoadingIndicator indicator)
        {
            var token = this.GetCancellationTokenOnDestroy();

            await UniTask.Yield(token);

            for (var i = 0; i < asyncLoadingResources.Length; ++i)
            {
                var resource = Instantiate(asyncLoadingResources[i]);
                var operation = resource.GetAsyncOperation();
                resource.StartProcess();

                await UniTask.WaitUntil(() =>
                {
                    indicator.LoadingPercentage = CalculateLoadingProgress(operation,
                        i, asyncLoadingResources.Length + 1);
                    return operation.IsDone;
                }, cancellationToken: token);
            }

            Debug.Log("로딩이 끝났습니다");
        }

        /// <summary>
        ///     로딩 진행률을 계산합니다.
        /// </summary>
        /// <param name="currentLoader">현재 로더</param>
        /// <param name="currentLoaderIdx">현재 로더 인덱스</param>
        /// <param name="totalLoaderCount">전체 로더 개수</param>
        /// <returns></returns>
        private float CalculateLoadingProgress(CustomizableAsyncOperation currentLoader, int currentLoaderIdx,
            int totalLoaderCount)
        {
            var currentProgress = currentLoader.Progress;
            var progressPerLoader = 1f / totalLoaderCount;
            var progress = currentProgress * progressPerLoader + currentLoaderIdx * progressPerLoader;
            return progress;
        }

        /// <summary>
        ///     메인 메뉴에 진입 시에 상태를 생성합니다.
        /// </summary>
        /// <returns>메인 메뉴 상태</returns>
        private IState CreateMainMenuState()
        {
            return new State(OnMainMenu);
        }

        /// <summary>
        ///     메인 메뉴를 갱신합니다.
        /// </summary>
        public void RefreshMainMenu()
        {
            OnMainMenu();
        }

        private void OnMainMenu()
        {
        }
    }
}