using UnityCommunity.UnitySingleton;

namespace Generic
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class ApplicationEvents : PersistentMonoSingleton<ApplicationEvents>
    {
        /// <summary>
        /// max time when player didn't took input, to go to afk scene (1f : 1sec)
        /// </summary>
        private const float AFK_INPUT_TIME = 180f;

        /// <summary>
        /// Power saving UI when player afk
        /// </summary>
        [SerializeField] private GameObject _powerSavingUI;

        /// <summary>
        /// button to trigger back from afk
        /// </summary>
        [SerializeField] private Button _triggerAwakeButton;

        /// <summary>
        /// On Application Pause Events
        /// </summary>
        public Action<bool> OnPause;

        /// <summary>
        /// On Application Exit Events
        /// </summary>
        public Action OnExit;

        /// <summary>
        /// On player want to change to federation login Events
        /// </summary>
        public Action OnChangeToGoogleLogin;

        /// <summary>
        /// Event to player afk listener
        /// true / false
        /// start afk / back from afk
        /// </summary>
        public Action<bool> OnPlayerAfkStateChange;

        /// <summary>
        /// On interupt save data events
        /// for change account etc
        /// </summary>
        public Action<bool> OnSaveDataInteruption;

        /// <summary>
        /// On player want to clear and reset game data
        /// </summary>
        public Action OnPlayerPlanningToDeleteData;

        /// <summary>
        /// Events after player confirm to clear data
        /// And start new game data
        /// </summary>
        public Action DeleteAllGameData;

        /// <summary>
        /// Events to request save all data
        /// </summary>
        public Action RequestSaveData;

        /// <summary>
        /// All registered game save data manager
        /// bool true mean some data in process
        /// </summary>
        private Dictionary<string, bool> _allSaveDataProcessState = new Dictionary<string, bool>();

        /// <summary>
        /// Checking Is all saved data process state done
        /// All progression saved will true when 
        /// All user data that registered in _allSaveDataProcessState, completed their process data (Saving / Deleting)
        /// </summary>
        private bool IsAllSaveDataProcessDone
        {
            get {
                foreach (var saveData in _allSaveDataProcessState)
                    if (saveData.Value)
                        return false;

                return true;
            } 
        }

        /// <summary>
        /// State is player enter the game (already passed tap to start game ui)
        /// </summary>
        private bool isPlayerEnterTheGame;

        /// <summary>
        /// AFK state (player declared as AFK if inputTimer >= AFK_INPUT_TIME)
        /// </summary>
        private bool isAFK;

        /// <summary>
        /// timer to count player input time
        /// </summary>
        private float inputTimer;

        protected override void Awake()
        {
            base.Awake();
            
            Screen.sleepTimeout = SleepTimeout.NeverSleep; // Setting devise sleep time
            _triggerAwakeButton.onClick.AddListener(SetBackFromAFK);
        }

        private void Update()
        {
            if (isAFK || !isPlayerEnterTheGame)
                return;

            inputTimer += Time.deltaTime; // AFK Timer

            if (inputTimer > AFK_INPUT_TIME) // Set player AFK when inputTimer exceed AFK_INPUT_TIME
                SetAFK();
            if (Input.GetMouseButtonDown(0)) // Reset AFK Timer when player perform input
                inputTimer = 0;
        }

        private void OnApplicationPause(bool isPause)
        {
            OnPause?.Invoke(isPause); // Call OnPause events when appliaction pause
        }

        private void OnApplicationQuit()
        {
            OnExit?.Invoke(); // Call OnExit events when appliaction Exit
        }

        private void OnApplicationFocus(bool focus)
        {
            Debug.Log("Xcute Application focus : " + focus);
            RequestSaveData?.Invoke();
        }

        /// <summary>
        /// Register save data
        /// </summary>
        public void RegisterSaveData(string saveDataKey)
        {
            if (!_allSaveDataProcessState.ContainsKey(saveDataKey))
                _allSaveDataProcessState.Add(saveDataKey, false);
        }

        /// <summary>
        /// Set Player state to AFK
        /// </summary>
        private void SetAFK()
        {
            isAFK = true;
            OnPlayerAfkStateChange?.Invoke(isAFK);
            
            GC.Collect(); // Clear memory

            inputTimer = 0;
            Time.timeScale = 0f;

            _powerSavingUI.SetActive(true);
            RequestSaveData?.Invoke(); // Save all progression
        }

        private void SetBackFromAFK()
        {
            Time.timeScale = 1f;

            isAFK = false;
            OnPlayerAfkStateChange?.Invoke(isAFK);

            _powerSavingUI.SetActive(false);
            RequestSaveData?.Invoke(); // Save all progression
        }

        /// <summary>
        /// Set Player Enter The Game State
        /// </summary>
        /// <param name="state"></param>
        public void SetPlayerEnterGameState(bool state)
        {
            isPlayerEnterTheGame = state;
        }

        /// <summary>
        /// Saving All Progression
        /// </summary>
        public void SaveAllProgression()
        {
            RequestSaveData?.Invoke(); // call save all progression events
            StartCoroutine(AwaitSaveAllProgressionSuccess()); // async await to all progression saved
        }

        /// <summary>
        /// Set data on process state
        /// </summary>
        /// <param name="saveDataKey"> save data key </param>
        /// <param name="isDoneState"> true when when data in process, false when process data done </param>
        public void SetDataOnProcessState(string saveDataKey, bool isDoneState)
        {
            if (!_allSaveDataProcessState.ContainsKey(saveDataKey))
                return;

            _allSaveDataProcessState[saveDataKey] = isDoneState;
        }

        /// <summary>
        /// Change player login to google login
        /// Available if player login as guest
        /// </summary>
        public void ChangeToGoogleLogin()
        {
            OnChangeToGoogleLogin?.Invoke(); // call OnChangeToGoogleLogin events
        }

        /// <summary>
        /// Interupt all save data
        /// If set to true all progression save data will (interupted / not saved)
        /// </summary>
        /// <param name="state"></param>
        public void SetInteruptSaveData(bool state)
        {
            OnSaveDataInteruption?.Invoke(state);
        }

        /// <summary>
        /// n player want to clear and reset game data
        /// </summary>
        public void PlayerWantToDeleteData()
        {
            OnPlayerPlanningToDeleteData?.Invoke();
        }

        /// <summary>
        /// Events after player confirm to clear data
        /// </summary>
        public void DeleteGameDataConfirmed(Action onAllProgressionDeletedCallback)
        {
            DeleteAllGameData?.Invoke();
            PlayerPrefs.DeleteAll();

            StartCoroutine(AwaitDeleteAllProgressionSuccess(onAllProgressionDeletedCallback));
        }

        /// <summary>
        /// Restart the game (back to initialize scene)
        /// </summary>
        public void RestartGame()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(Utils.StaticConstantDictionary.SCENE_START_IDX);
        }

        /// <summary>
        /// Waiting all save data finish saving
        /// </summary>
        /// <returns></returns>
        private IEnumerator AwaitSaveAllProgressionSuccess()
        {
            yield return new WaitUntil(() => IsAllSaveDataProcessDone); // await all progresion saved
            
            // TODO: Show notice to player that all progression saved
        }

        /// <summary>
        /// Waiting all deleted data finish saving
        /// </summary>
        /// <param name="onAllProgressionDeletedCallback"></param>
        /// <returns></returns>
        private IEnumerator AwaitDeleteAllProgressionSuccess(Action onAllProgressionDeletedCallback)
        {
            yield return new WaitUntil(() => IsAllSaveDataProcessDone); // await all progresion saved
            onAllProgressionDeletedCallback?.Invoke();
        }
    }

}