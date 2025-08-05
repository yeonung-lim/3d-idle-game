using System.Threading.Tasks;

namespace Back.Module.Login
{
     using System;
     using UnityEngine;
     using BackEnd;
     using Newtonsoft.Json;

     public static class BackEndLogin
     {
          public static bool IsAuthenticated { get; private set; }
          
//         /// <summary>
//         /// Login with token
//         /// Available once player have success login
//         /// </summary>
//         /// <returns></returns>
//         public static bool IsLoginWithTokenSuccess()
//         {
//             LoadingUIController.Instance.Loading(); // Start Loading
//
//             Debug.Log("Requesting login with token");
//             var bro = Backend.BMember.LoginWithTheBackendToken();
//
//             LoadingUIController.Instance.FinishLoading(); // End Loading
//             return bro.IsSuccess();
//         }
//
//         /// <summary>
//         /// Custom Login
//         /// Debug Mode Only (not for open player)
//         /// </summary>
//         /// <param name="id"></param>
//         /// <param name="pw"></param>
//         public static void CustomSignUp(string id, string pw)
//         {
//             LoadingUIController.Instance.Loading(); // Start Loading
//
//             var bro = Backend.BMember.CustomSignUp(id, pw);
//
//             if (!bro.IsSuccess()) // if failed, showing error message
//                 NoticeUIController.Instance.ShowNotice(bro.Message, null);
//
//             LoadingUIController.Instance.FinishLoading(); // End Loading
//         }
//
//         /// <summary>
//         /// Backnd Guest login
//         /// Count as custom login
//         /// </summary>
//         public static void GuestLogin()
//         {
//             LoadingUIController.Instance.Loading(); // Start Loading
//
//             Debug.Log("Requesting guest login.");
//
//             var bro = Backend.BMember.GuestLogin("Guest Login");
//
//             if (!bro.IsSuccess()) // if failed, showing error message
//             {
//                 Backend.BMember.DeleteGuestInfo();
//                 Backend.Utils.GetGoogleHash();
//                 NoticeUIController.Instance.ShowNotice(bro.Message +" hash key "+ Backend.Utils.GetGoogleHash(), null);
//             }
//
//             LoadingUIController.Instance.FinishLoading(); // End Loading
//         }
//
//         /// <summary>
//         /// Federation login with google
//         /// </summary>
//         public static void GoogleLogin()
//         {
//             LoadingUIController.Instance.Loading(); // Start Loading
//
//             LoginWithGoogle((isSuccess, errorMessage, token) =>
//             {
//                 if (!isSuccess) // if failed, showing error message
//                 {
//                     NoticeUIController.Instance.ShowNotice(errorMessage, null);
//                     LoadingUIController.Instance.FinishLoading(); // End Loading
//                     return;
//                 }
//
//                 LoginGoogleWithFederationToken(token);
//
//                 LoadingUIController.Instance.FinishLoading(); // End Loading
//             });
//         }
//
//         /// <summary>
//         /// Federation login with google
//         /// </summary>
//         public static void LoginGoogleWithFederationToken(string federationToken)
//         {
//             var bro = Backend.BMember.AuthorizeFederation(federationToken, FederationType.Google);
//
//             if (!bro.IsSuccess()) // if failed, showing error message
//                 NoticeUIController.Instance.ShowNotice(bro.Message, null);
//         }
//
//         /// <summary>
//         /// Change to federation login
//         /// available at custom and guest login only
//         /// </summary>
//         /// <param name="resultCallback"> result callback </param>
//         /// <param name="overwriteCallback"> callback when thereis conflict account data </param>
//         public static void ChangeCustomToFederationLogin(Action<bool> resultCallback, Action<string> conflictDataCallback)
//         {
//             LoadingUIController.Instance.Loading(); // Start Loading
//
//             LoginWithGoogle((isSuccess, errorMessage, token) =>
//             {
//                 if (!isSuccess)
//                 {
//                     NoticeUIController.Instance.ShowNotice(errorMessage, null);
//                     LoadingUIController.Instance.FinishLoading(); // End Loading
//                     return;
//                 }
//
//
//                 int accountStatus = CheckUserInBackend(token, FederationType.Google);
//                 if (accountStatus == 200) // There is data in federation account
//                 {
//                     if (!BackndUserInfo.Instance.IsGuestAccount || !LogoutAccount())
//                         NoticeUIController.Instance.ShowNotice("Change to federation process failed", null);
//
//                     var bro = Backend.BMember.AuthorizeFederation(token, FederationType.Google);
//
//                     if (!bro.IsSuccess()) // if failed, showing error message
//                         NoticeUIController.Instance.ShowNotice(bro.Message, null);
//
//                     conflictDataCallback?.Invoke(token);
//
//                     if (!LogoutAccount())
//                     {
//                         Debug.Log("Change to federation process signout failed");
//                         return;
//                     }
//
//                     GuestLogin();
//                 }
//                 else if (accountStatus == 204) // The federationToken has not been signed up
//                 {
//                     Debug.Log("Change from guest login to federation login");
//
//                     var bro = Backend.BMember.ChangeCustomToFederation(token, FederationType.Google);
//
//                     if (!bro.IsSuccess()) // if failed, showing error message
//                     {
//                         GoogleSignOut(null);
//                         NoticeUIController.Instance.ShowNotice(bro.Message, null);
//                     }
//
//                     resultCallback?.Invoke(bro.IsSuccess()); // send result to callback
//                 }
//
//                 LoadingUIController.Instance.FinishLoading(); // End Loading
//             });
//         }
//
//         /// <summary>
//         /// Checking federation account state
//         /// </summary>
//         /// <param name="token"> federation token </param>
//         /// <param name="federationType"> Federation account type </param>
//         /// <returns> 204 When the federationToken has not been signed up,
//         /// 200 When the federationToken has been signed up </returns>
//         private static int CheckUserInBackend(string token, FederationType federationType)
//         {
//             BackendReturnObject bro = Backend.BMember.CheckUserInBackend(token, federationType);
//
//             if (!bro.IsSuccess())
//                 NoticeUIController.Instance.ShowNotice(bro.Message, null);
//             else
//             {
//                 Debug.Log("Message: " + bro.Message);
//                 Debug.Log("Account info: " + bro.ToString());
//             }
//
//             return bro.StatusCode; // return status code
//         }
//
//         /// <summary>
//         /// Login to google and get federation token
//         /// </summary>
//         /// <param name="googleLoginCallback"> result callback </param>
//         private static void LoginWithGoogle(Action<bool, string, string> googleLoginCallback)
//         {
//             Debug.Log("Requesting login with google play.");
//
//             // Login google to get federation token
//             TheBackend.ToolKit.GoogleLogin.Android.GoogleLogin(true,
//                 (isSuccess, errorMessage, token) =>
//                     googleLoginCallback?.Invoke(isSuccess, errorMessage, token));
//         }
//
//         /// <summary>
//         /// Federation google sign out
//         /// </summary>
//         /// <param name="resultCallback"> result callback </param>
//         public static void GoogleSignOut(Action<bool> resultCallback)
//         {
//             Debug.Log("Requesting google sign out.");
//
//             TheBackend.ToolKit.GoogleLogin.Android.GoogleSignOut(true,
//                 (isSuccess, errorMessage) =>
//                 {
//                     if (!isSuccess)
//                         NoticeUIController.Instance.ShowNotice(errorMessage, null);
//
//                     resultCallback?.Invoke(isSuccess);
//                 });
//         }
//
//         /// <summary>
//         /// Logout backnd account
//         /// </summary>
//         /// <returns> result state </returns>
//         public static bool LogoutAccount()
//         {
//             var bro = Backend.BMember.Logout();
//             
//             if (!bro.IsSuccess())
//                 NoticeUIController.Instance.ShowNotice(bro.Message, null);
//
//             Debug.Log("Message : " + bro.Message);
//             Debug.Log("Bro : " + bro.ToString());
//
//             return bro.IsSuccess();
//         }
          public static async Task<bool> AuthorizeFederation()
          {
              return true;
          }
     }
}