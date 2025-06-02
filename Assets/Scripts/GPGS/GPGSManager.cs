using System.Threading;
using Cysharp.Threading.Tasks;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine;
using Utils.Async;

namespace GPGS
{
    public static class GooglePlayGamesService
    {
        private static bool _autoAuthFailed;
        private static string AccessToken { get; set; } = string.Empty;
        public static bool IsAuthenticated => PlayGamesPlatform.Instance.IsAuthenticated();

        public static UniTask<bool> Authenticate(CancellationToken token = default)
        {
            if (PlayGamesPlatform.Instance.IsAuthenticated())
                return CompletedTasks.True;

            var tcs = Completion.CreateWithDefaultCancelToken<bool>();

            if (_autoAuthFailed)
                PlayGamesPlatform.Instance.ManuallyAuthenticate(ProcessSignIn);
            else
                PlayGamesPlatform.Instance.Authenticate(ProcessSignIn);

            return tcs.Task;

            // 구글 플레이 게임 서비스 로그인 처리 (중첩 함수)
            void ProcessSignIn(SignInStatus status)
            {
                if (status != SignInStatus.Success)
                {
                    _autoAuthFailed = true;
                    Debug.Log("구글 플레이 게임 서비스 로그인 실패: " + status);

                    tcs.TrySetResult(false);
                    return;
                }

                Debug.Log("구글 플레이 게임 서비스 로그인 성공");
                tcs.TrySetResult(true);
            }
        }

        public static async UniTask<string> RequestAccessCode()
        {
            if (IsAuthenticated == false)
                return string.Empty;

            if (string.IsNullOrEmpty(AccessToken) == false)
                return AccessToken;

            var tcs = Completion.CreateWithDefaultCancelToken<string>();
            PlayGamesPlatform.Instance.RequestServerSideAccess(false, code =>
            {
                AccessToken = code;
                tcs.TrySetResult(code);
            });

            return await tcs.Task;
        }
    }
}