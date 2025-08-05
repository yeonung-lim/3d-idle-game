namespace Utils
{
    using System;
    using System.Collections.Generic;
    using BackEnd;
    using Newtonsoft.Json;

    public static class StaticReflection
    {
        /// <summary>
        /// Object conversion to Backnd Param
        /// </summary>
        /// <typeparam name="T"> object type data </typeparam>
        /// <param name="obj"> object </param>
        /// <returns> Param ready to send into database </returns>
        public static Param GetParam<T>(T obj)
        {
            Param param = new Param();

            foreach (var field in obj.GetType().GetFields()) // Assign param field for all field in obj
            {
                if (string.Equals(field.Name, StaticConstantDictionary.IN_DATE_KEY)) // Skip indate (it is automaticaly added in backndWBEHJN!#~UBB )
                    continue;

                param.Add(field.Name, field.GetValue(obj));
            }

            return param;
        }

        /// <summary>
        /// Json database conversion to Object
        /// </summary>
        /// <typeparam name="T"> type data </typeparam>
        /// <param name="jsonData"> list of data in json </param>
        /// <returns> list of data </returns>
        public static List<T> DatabaseItemsParse<T>(string jsonData)
        {
            // add string items before json data, to manipulate json
            // after that jsonConvert can convert item in jsonData as collections
            List<T> yieldDatas = JsonConvert.DeserializeObject<DatabaseItems<T>>("{\"Items\":" + jsonData + "}").Items;
            return yieldDatas;
        }

        [Serializable]
        public struct DatabaseItems<T>
        {
            /// <summary>
            /// Item list in database table
            /// </summary>
            public List<T> Items { get; set; }
        }
    }
}