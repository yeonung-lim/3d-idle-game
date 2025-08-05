using UnityCommunity.UnitySingleton;

namespace Generic
{
    using System;
    using System.Collections;
    using UnityEngine;

    public class AutoSaveTimer : MonoBehaviour, ISaveTrigger
    {
        /// <summary>
        /// Time to save events
        /// </summary>
        public event Action OnSave;

        /// <summary>
        /// Auto save timer
        /// </summary>
        private float timerCount;

        /// <summary>
        /// One second timer
        /// </summary>
        private YieldInstruction awaitOneSec = new WaitForSeconds(1f);
        
        void Awake()
        {
            timerCount = 0; // set timer
        }

        private void Update()
        {
            timerCount += Time.deltaTime; // auto save timer

            if (timerCount >= Utils.StaticConstantDictionary.SEND_DATA_TIME) // When timer exceed SEND_DATA_TIME
            {
                OnSave?.Invoke(); // push Save events
                timerCount -= Utils.StaticConstantDictionary.SEND_DATA_TIME; // Reset timer
            }
        }

        /// <summary>
        /// For data that needed to immediately saved
        /// For example. save new dress after open lootbox
        /// </summary>
        /// <param name="processSaveData"></param>
        public void SaveDataImmediately(Action processSaveData)
        {
            StartCoroutine(CollectDataAwait(processSaveData));
        }

        /// <summary>
        /// Waiting for additional data before saving to avoid double execution saving data
        /// </summary>
        /// <param name="processSaveData"></param>
        /// <returns></returns>
        private IEnumerator CollectDataAwait(Action processSaveData)
        {
            yield return awaitOneSec;
            processSaveData?.Invoke();
        }
    }
}