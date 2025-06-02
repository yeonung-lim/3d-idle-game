using UnityEngine;

namespace Game.Core
{
    public class StageManager : MonoBehaviour
    {
        public static StageManager Instance { get; private set; }

        public int currentStage { get; private set; }
        public event System.Action<int> OnStageChanged;

        public int monstersKilledThisStage { get; private set; }
        public int monstersPerStage = 10; // Default, can be configured in Inspector

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                // DontDestroyOnLoad(gameObject); // Optional: if it should persist
            }
            else
            {
                Debug.LogWarning("StageManager: Another instance already exists. Destroying this one.");
                Destroy(gameObject);
            }
        }

        void Start()
        {
            InitializeStage();
        }

        private void InitializeStage()
        {
            currentStage = 1;
            monstersKilledThisStage = 0;
            OnStageChanged?.Invoke(currentStage);
            Debug.Log("StageManager initialized. Current Stage: " + currentStage);
        }

        public void AdvanceToNextStage()
        {
            currentStage++;
            monstersKilledThisStage = 0;
            OnStageChanged?.Invoke(currentStage);
            Debug.Log("Advanced to Stage: " + currentStage);
            // Future enhancements:
            // - Notify MonsterSpawner to adjust difficulty (e.g., spawn stronger monsters, reduce spawnInterval)
            // - Change environment, music, etc.
        }

        public void MonsterKilled()
        {
            if (currentStage == 0) // If not initialized or game over state
            {
                Debug.LogWarning("StageManager: MonsterKilled called but no active stage (currentStage is 0).");
                return;
            }

            monstersKilledThisStage++;
            Debug.Log("Monster killed for stage progress. Total this stage: " + monstersKilledThisStage + "/" + monstersPerStage);

            if (monstersKilledThisStage >= monstersPerStage)
            {
                AdvanceToNextStage();
            }
        }

        void Update()
        {
            // Debug key to advance stage
            if (Input.GetKeyDown(KeyCode.N))
            {
                Debug.Log("Debug: Advancing to next stage via key press.");
                AdvanceToNextStage();
            }
        }
    }
}
