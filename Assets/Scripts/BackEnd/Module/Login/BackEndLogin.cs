using BackEnd;
using GPGS;
using UnityEngine;

namespace Back.Module.Login
{
    public class BackEndLogin
    {
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

        public static void GetAccessToken()
        {
#if UNITY_ANDROID
            var code = GooglePlayGamesService.AccessCode;
            if (string.IsNullOrEmpty(code))
            {
                Debug.LogError("구글 플레이 게임 서비스 인증 코드가 없습니다.");
                return;
            }

            Backend.BMember.GetGPGS2AccessToken(code, googleCallback =>
            {
                Debug.Log("GetGPGS2AccessToken 호출 결과 : " + googleCallback);

                if (googleCallback.IsSuccess())
                {
                    var accessToken = googleCallback.GetReturnValuetoJSON()["access_token"].ToString();
                    Debug.Log("구글 플레이 게임 서비스 액세스 토큰 : " + accessToken);
                }
            });
#endif
        }
    }
}