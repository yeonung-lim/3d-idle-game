using System.Collections.Generic;
using AsyncInitialize;
using Back;
using Cysharp.Threading.Tasks;
using GPGS;
using UnityEngine;
using View.ViewComponents.Loading;
using State = Core.StateMachine.State;

namespace Sequence
{
    /// <summary>
    ///     게임 진행을 관리하는 클래스
    /// </summary>
    public class SequenceManager : MonoBehaviour
    {
        [SerializeField] private GameObject[] preloadedAssets; // 미리 로드할 프리팹

        [Header("Events")] [SerializeField] private AbstractGameEvent continueEvent; // 계속하기 이벤트
        [SerializeField] private AbstractGameEvent backEvent; // 뒤로가기 이벤트
        [SerializeField] private AbstractGameEvent winEvent; // 승리 클리어 이벤트
        [SerializeField] private AbstractGameEvent loseEvent; // 승리 실패 이벤트
        [SerializeField] private AbstractGameEvent pauseEvent; // 일시정지 이벤트

        /// <summary>
        ///     초기화 시 로딩 할 리소스들
        ///     순차적으로 로딩됌
        /// </summary>
        private List<IAsyncInit> _asyncLoadingResources;

        /// <summary>
        ///     메인 메뉴 상태
        /// </summary>
        private IState _mainMenuState;

        /// <summary>
        ///     상태 관리자
        /// </summary>
        private StateMachine _stateMachine;

        /// <summary>
        ///     초기화 상태
        ///     - 미리 로드할 프리팹을 인스턴스화합니다.
        /// </summary>
        /// <returns></returns>
        private IState CreateInitState()
        {
            return new State(InstantiatePreloadedAssets);
        }

        /// <summary>
        ///     미리 로드한 에셋을 인스턴스화합니다.
        /// </summary>
        private void InstantiatePreloadedAssets()
        {
            foreach (var asset in preloadedAssets) Instantiate(asset);
        }

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

                _asyncLoadingResources = new List<IAsyncInit>
                {
                    GooglePlayGamesService.Instance,
                    BackEndManager.Instance
                };

                LoadingResources(_asyncLoadingResources, indicator).ContinueWith(() => { continueEvent.Raise(); })
                    .Forget();
            });
        }

        /// <summary>
        ///     IAsyncInit를 상속받은 클래스들을 로드합니다.
        /// </summary>
        /// <param name="asyncInits">로딩 할 리소스</param>
        /// <param name="indicator">로딩 인디케이터</param>
        private async UniTask LoadingResources(List<IAsyncInit> asyncInits, LoadingIndicator indicator)
        {
            var token = this.GetCancellationTokenOnDestroy();

            await UniTask.Yield(token);

            for (var i = 0; i < asyncInits.Count; ++i)
            {
                var operation = asyncInits[i].GetAsyncOperation();
                asyncInits[i].StartInitialize();

                await UniTask.WaitUntil(() =>
                {
                    indicator.LoadingPercentage = CalculateLoadingProgress(operation,
                        i, asyncInits.Count);
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

        private void OnMainMenu()
        {
        }
    }
}