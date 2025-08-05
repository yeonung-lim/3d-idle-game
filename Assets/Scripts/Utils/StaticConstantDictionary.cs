using UnityEngine;

namespace Utils
{
    public static class StaticConstantDictionary
    {
        #region Backend

        public const string GAME_URL_IN_PLAYSTORE = 
            "https://play.google.com/";

        public const string USER_IN_DATE_KEY = "owner_inDate";
        public const string IN_DATE_KEY = "inDate";


        public const string LAST_UPDATED_DATE_TIME_KEY = "updatedAt";

        public const int SEND_DATA_TIME = 60;
        public const int MAX_FAIL_SEND_DATA_COUNT = 3;

        #endregion
        
        #region InGameElement

        public const int PERCENT = 100;

        public const int SCENE_START_IDX = 0;
        public const int SCENE_GAMEPLAY_IDX = 1;

        public const string DEFAULT_DRESS_ID = "default";
        public const string NOT_ENOUGH_RESOURCES_MESSAGE = "Insufficient resources.";
        public const float CUSTOMER_ORDER_TIME = 1f;

        public const int BOOL_TO_INT_FALSE_VALUE = 0;
        public const int BOOL_TO_INT_TRUE_VALUE = 1;

        public static Vector2 MIDLE_SCREEN_POSITION;

        #endregion
    }
}