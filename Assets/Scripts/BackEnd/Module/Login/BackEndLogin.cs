using BackEnd;
using Cysharp.Threading.Tasks;
using GPGS;
using UnityEngine;
using Utils.Async;

namespace Back.Module.Login
{
    public class BackEndLogin
    {
        public static bool IsAuthenticated { get; private set; }

        private static string GetMessageWithFederationResult(string statusCode)
        {
            return statusCode switch
            {
                "200" => "로그인 성공",
                "201" => "신규 회원가입 성공",
                "400" => "디바이스 정보가 없습니다.",
                "403" => "차단 당한 계정/디바이스 입니다.",
                "410" => "탈퇴가 진행중인 계정입니다.",
                _ => "알 수 없는 오류"
            };
        }

        public static async UniTask<bool> AuthorizeFederation()
        {
            if (GooglePlayGamesService.IsAuthenticated == false)
                await GooglePlayGamesService.Authenticate();

            if (GooglePlayGamesService.IsAuthenticated == false)
                return false;

            var tcs = Completion.CreateWithDefaultCancelToken<bool>();

            var token = await GetAccessToken();
            Backend.BMember.AuthorizeFederation(
                token, FederationType.GPGS2, "GPGS2로 가입", bro =>
                {
                    IsAuthenticated = bro.IsSuccess();
                    tcs.TrySetResult(IsAuthenticated);
                    Debug.Log("인증 결과 : " + GetMessageWithFederationResult(bro.GetStatusCode()));
                });

            return await tcs.Task;
        }

        public static bool CustomSignUp(string id, string pw)
        {
            // 커스텀 회원가입 로직
            Debug.Log("회원가입을 요청합니다.");

            var bro = Backend.BMember.CustomSignUp(id, pw);

            if (bro.IsSuccess())
                Debug.Log("회원가입에 성공했습니다. : " + bro);
            else
                Debug.LogError("회원가입에 실패했습니다. : " + bro);

            return bro.IsSuccess();
        }

        public static void CustomLogin(string id, string pw)
        {
            // Step 3. 로그인 구현하기 로직
        }

        public static void UpdateNickname(string nickname)
        {
            // Step 4. 닉네임 변경 구현하기 로직
        }

        private static async UniTask<string> GetAccessToken()
        {
#if UNITY_ANDROID
            var code = await GooglePlayGamesService.RequestAccessCode();

            if (string.IsNullOrEmpty(code))
            {
                Debug.LogError("구글 플레이 게임 서비스 인증 코드가 없습니다.");
                return code;
            }

            var tcs = Completion.CreateWithDefaultCancelToken<string>();
            Backend.BMember.GetGPGS2AccessToken(code, googleCallback =>
            {
                Debug.Log("GetGPGS2AccessToken 호출 결과 : " + googleCallback);

                if (googleCallback.IsSuccess())
                {
                    var accessToken = googleCallback.GetReturnValuetoJSON()["access_token"].ToString();
                    tcs.TrySetResult(accessToken);
                    Debug.Log("구글 플레이 게임 서비스 액세스 토큰 : " + accessToken);
                }
                else
                {
                    tcs.TrySetResult(string.Empty);
                }
            });

            return await tcs.Task;
#endif
        }
    }
}