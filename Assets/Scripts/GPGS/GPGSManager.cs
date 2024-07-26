using AsyncInitialize;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityCommunity.UnitySingleton;
using UnityEngine;

namespace GPGS
{
    public class GooglePlayGamesService : PersistentMonoSingleton<GooglePlayGamesService>, IAsyncInit
    {
        private static uint _initializeCount;
        public static string AccessCode { get; private set; }

        public CustomizableAsyncOperation GetAsyncOperation()
        {
            return CustomizableAsyncOperation.Create(() => _initializeCount > 0);
        }

        public void StartInitialize()
        {
        }

        public void Reset()
        {
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            PlayGamesPlatform.Instance.Authenticate(ProcessAuthentication);
        }

        private void ProcessAuthentication(SignInStatus status)
        {
            _initializeCount++;

            if (status != SignInStatus.Success)
                // Disable your integration with Play Games Services or show a login button
                // to ask users to sign-in. Clicking it shuld call
                // PlayGamesPlatform.Instance.ManuallyAuthenticate(Process);
                return;

            RequestAccessCode();
        }

        private void RequestAccessCode()
        {
            PlayGamesPlatform.Instance.RequestServerSideAccess(false, code =>
            {
                Debug.Log("구글 인증 코드 : " + code);
                AccessCode = code;
            });
        }
    }
}