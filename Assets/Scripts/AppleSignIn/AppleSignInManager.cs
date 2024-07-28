#if UNITY_IOS
using System;
using Cysharp.Threading.Tasks;
using Load;
using UnityEngine;
using USingleton.AutoSingleton;

namespace AppleSignIn
{
    /// <summary>
    ///     Apple 로그인 관리자
    /// </summary>
    [Singleton(nameof(AppleSignInManager))]
    public class AppleSignInManager : MonoBehaviour, IAsyncInit
    {
        private bool _isInitialized;

        /// <summary>
        ///     비동기 작업의 진행 상황을 반환합니다.
        /// </summary>
        /// <returns></returns>
        public CustomizableAsyncOperation GetAsyncOperation()
        {
            return CustomizableAsyncOperation.Create(() => _isInitialized);
        }

        /// <summary>
        ///     비동기 작업을 시작합니다.
        /// </summary>
        public void StartProcess()
        {
            Init().Forget();
        }

        /// <summary>
        ///     비동기 작업을 초기화합니다.
        /// </summary>
        public void Reset()
        {
            _isInitialized = false;
        }

        /// <summary>
        ///     초기화
        /// </summary>
        private async UniTaskVoid Init()
        {
            if (_isInitialized) return;

            try
            {
                // 로그인 프로세스 진행
                var result = await GameCenterLogin();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                // 프로세스가 실패하더라도 초기화는 완료되었음을 표시합니다.
                _isInitialized = true;
            }
        }

        /// <summary>
        ///     GameCenter 로그인
        /// </summary>
        /// <returns>성공 여부</returns>
        private async UniTask<bool> GameCenterLogin()
        {
            var utcs = new UniTaskCompletionSource<bool>();
            var cancellationToken = this.GetCancellationTokenOnDestroy();
            cancellationToken.Register(() => utcs.TrySetCanceled(cancellationToken));

            Social.localUser.Authenticate(success =>
            {
                if (success)
                {
                    Debug.Log("Success to authenticate");
                    utcs.TrySetResult(true);
                }
                else
                {
                    Debug.Log("Fail to login");
                    utcs.TrySetResult(false);
                }
            });

            return await utcs.Task;
        }
    }
}
#endif