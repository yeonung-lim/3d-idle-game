namespace Back.Module.ETC
{
    using System;
    using BackEnd;

    public static class BackEndServerTime
    {
        /// <summary>
        /// universal time code
        /// </summary>
        private const string TIME_DATA_KEY = "utcTime";

        /// <summary>
        /// Get current server time (Asynchronous)
        /// </summary>
        /// <param name="callback"> callback after successful get server time </param>
        public static void GetServerTime(Action<DateTime> callback)
        {
            Backend.Utils.GetServerTime((bro) =>
            {
                if (!bro.IsSuccess())
                {
                    // NoticeUIController.Instance.ShowNotice("Failed to get server time\nMessage: " + bro.Message, () => GetServerTime(callback));
                    return;
                }

                var currentTimeStr = bro.GetReturnValuetoJSON()[TIME_DATA_KEY].ToString();
                callback?.Invoke(DateTime.Parse(currentTimeStr));
            });
        }

        /// <summary>
        /// Get current server time (Synchronous)
        /// </summary>
        /// <returns> server time </returns>
        public static DateTime GetServerTime()
        {
            var bro = Backend.Utils.GetServerTime();

            if (!bro.IsSuccess())
            {
                // NoticeUIController.Instance.ShowNotice("Failed to get server time\nMessage: " + bro.Message, null);
                return new DateTime();
            }

            var currentTimeStr = bro.GetReturnValuetoJSON()[TIME_DATA_KEY].ToString();
            return DateTime.Parse(currentTimeStr);
        }
    }
}