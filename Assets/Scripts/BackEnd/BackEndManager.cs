using AsyncInitialize;
using Back.Module.Login;
using BackEnd;
using UnityCommunity.UnitySingleton;
using UnityEngine;

namespace Back
{
    /// <summary>
    ///     뒤끝 관리자
    /// </summary>
    public class BackEndManager : Singleton<BackEndManager>, IAsyncInit
    {
        public override bool IsInitialized => base.IsInitialized && Backend.IsInitialized; // 뒤끝이 초기화되었는지 확인

        public void StartInitialize()
        {
            Initialize();

            if (!IsInitialized) return;

            Login();
        }

        public void Reset()
        {
        }

        public CustomizableAsyncOperation GetAsyncOperation()
        {
            return CustomizableAsyncOperation.Create(() => IsInitialized);
        }

        public void Initialize()
        {
            if (IsInitialized)
                return;

            var bro = Backend.Initialize(true); // 뒤끝 초기화

            // 뒤끝 초기화에 대한 응답값
            if (bro.IsSuccess())
                Debug.Log("초기화 성공 : " + bro); // 성공일 경우 statusCode 204 Success
            else
                Debug.LogError("초기화 실패 : " + bro); // 실패일 경우 statusCode 400대 에러 발생
        }

        private void SignUp()
        {
            BackEndLogin.CustomSignUp("id", "pw");
        }

        private void Login()
        {
            BackEndLogin.GetAccessToken();
        }
    }
}