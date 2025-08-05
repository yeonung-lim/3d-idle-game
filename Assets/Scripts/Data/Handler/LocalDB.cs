using Newtonsoft.Json;
using UnityEngine;

namespace ProjectIdle
{
    public class LocalDB : ILocalDB
    {
        /// <summary>
        /// 데이터를 PlayerPrefs에 저장합니다.
        /// </summary>
        /// <typeparam name="T">저장할 데이터의 타입</typeparam>
        /// <param name="key">데이터에 접근하기 위한 키</param>
        /// <param name="data">저장할 데이터 인스턴스</param>
        public void Save<T>(string key, T data)
        {
            if (data == null)
            {
                PlayerPrefs.DeleteKey(key);
                return;
            }

            string json = JsonConvert.SerializeObject(data, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
            
            PlayerPrefs.SetString(key, json);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// PlayerPrefs에서 데이터를 불러옵니다.
        /// </summary>
        /// <typeparam name="T">불러올 데이터의 타입</typeparam>
        /// <param name="key">데이터에 접근하기 위한 키</param>
        /// <returns>불러온 데이터. 데이터가 없으면 default 값을 반환합니다.</returns>
        public T Load<T>(string key)
        {
            if (!PlayerPrefs.HasKey(key))
            {
                return default;
            }

            string json = PlayerPrefs.GetString(key);
            if (string.IsNullOrEmpty(json))
            {
                return default;
            }

            return JsonConvert.DeserializeObject<T>(json);
        }

        public bool Exists(string key)
        {
            return PlayerPrefs.HasKey(key);
        }

        /// <summary>
        /// PlayerPrefs의 모든 데이터를 삭제합니다.
        /// </summary>
        public void Clear()
        {
            PlayerPrefs.DeleteAll();
        }

        /// <summary>
        /// PlayerPrefs에서 특정 키의 데이터를 삭제합니다.
        /// </summary>
        /// <param name="key">삭제할 데이터의 키</param>
        public void Delete(string key)
        {
            PlayerPrefs.DeleteKey(key);
        }
    }
}
