namespace Back.Module.ETC
{
    using BackEnd;
    using System;
    using UnityEngine;
    
    public static class BackEndServerState
    {
        /// <summary>
        /// Server status type
        /// -1: not initailize yet
        /// 0: online
        /// 1: offline
        /// 2: maintenance
        /// </summary>
        private static int _serverStatus = -1;

        /// <summary>
        /// Is server state available to play
        /// Server status should be online (0)
        /// </summary>
        public static bool IsServerStatusAvailable => _serverStatus == 0;

        /// <summary>
        /// latest version that registered in backnd console version control
        /// </summary>
        private static string _latestGameVersion;

        /// <summary>
        /// Checking is user version valid to play
        /// Checking if user played app version equals latest version on backnd console
        /// Checking if update issued type is not forced to update (2)
        /// </summary>
        public static bool IsCurrentGameVersionValid => _latestGameVersion == Application.version ||
            _updateIssuedType == 1;

        /// <summary>
        /// Update issued type
        /// -1: Not initailize yet
        /// 0: current installed is latest version
        /// 1: Thereis optional update
        /// 2: Forced to update
        /// </summary>
        private static int _updateIssuedType = -1;

        /// <summary>
        /// Checking server status
        /// </summary>
        /// <param name="onServerMaintenanceCallback"> callback when server is under maintenance </param>
        public static void CheckServerStatus(Action onServerMaintenanceCallback)
        {
            // LoadingUIController.Instance.Loading();
            
            _serverStatus = -1;
            Backend.Utils.GetServerStatus(bro =>
            {
                _serverStatus = (int)bro.GetReturnValuetoJSON()["serverStatus"];

                if (!IsServerStatusAvailable)
                    GetServerDataResultAnalyst(onServerMaintenanceCallback);

                // LoadingUIController.Instance.FinishLoading();
            });

        }

        /// <summary>
        /// Get server status issued when server unavailable
        /// </summary>
        /// <param name="onServerMaintenanceCallback"> callback when server is under maintenance </param>
        private static void GetServerDataResultAnalyst(Action onServerMaintenanceCallback)
        {
            switch(_serverStatus)
            {
                case 1:
                    // NoticeUIController.Instance.ShowNotice("Unable to Access to Server, Please Try Again", ApplicationEvents.Instance.RestartGame);
                    break;
                case 2:
                    onServerMaintenanceCallback?.Invoke();
                    break;
            }
        }

        /// <summary>
        /// Check version control
        /// </summary>
        public static void CheckVersionStatus()
        {
            #if UNITY_EDITOR
            _latestGameVersion = Application.version;
                // NoticeUIController.Instance.ShowFlashNotice("Version: " + Application.version);
                return;
            #endif

            // LoadingUIController.Instance.Loading(); // End Loading

            _latestGameVersion = string.Empty;
            _updateIssuedType = -1;

            Backend.Utils.GetLatestVersion((bro) =>
            {
                if (!bro.IsSuccess())
                {
                    // NoticeUIController.Instance.ShowNotice("Failed to get latest version\nMessage: " + bro.Message, Application.Quit);
                    return;
                }

                _latestGameVersion = bro.GetReturnValuetoJSON()["version"].ToString();
                // NoticeUIController.Instance.ShowFlashNotice("Version: " + Application.version);
                if (!IsCurrentGameVersionValid)
                {
                    _updateIssuedType = int.Parse(bro.GetReturnValuetoJSON()["type"].ToString());
                    DetectingUpdateMode();
                }

                // LoadingUIController.Instance.FinishLoading();
            });
        }

        /// <summary>
        /// Checking update issued type
        /// When there is update available
        /// </summary>
        private static void DetectingUpdateMode()
        {
            switch (_updateIssuedType)
            {
                case 1:
                    // normal update, asking for player if they want to update or not
                    Debug.Log("Updated Available");
                    break;
                case 2:
                    //force update required   
                    Debug.Log("Updated Required");

                    // NoticeUIController.Instance.ShowNotice(
                    //     "Update Required",
                    //     () => Application.OpenURL(Utility.StaticConstantDictionary.GAME_URL_IN_PLAYSTORE)
                    //     );
                    break;
                default:
                    break;
            }
        }
    }
}