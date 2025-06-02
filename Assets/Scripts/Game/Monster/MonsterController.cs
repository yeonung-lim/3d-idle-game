using UnityEngine;
using Game.Core; // Required for StatType and StatsController
using Game.Combat; // Required for HealthSystem

namespace Game.Monster
{
    [RequireComponent(typeof(StatsController))]
    [RequireComponent(typeof(HealthSystem))] // Ensure HealthSystem is present
    public class MonsterController : MonoBehaviour
    {
        public int goldValue = 10; // Gold awarded on death

        private StatsController statsController;
        private HealthSystem healthSystem;

        void Awake()
        {
            statsController = GetComponent<StatsController>();
            if (statsController == null)
            {
                Debug.LogError("MonsterController: StatsController component not found on this GameObject.");
            }

            healthSystem = GetComponent<HealthSystem>();
            if (healthSystem == null)
            {
                Debug.LogError("MonsterController: HealthSystem component not found on this GameObject.");
            }
            else
            {
                healthSystem.OnDeath += HandleDeath;
            }
        }

        void Start()
        {
            // Stats should be initialized before HealthSystem's Start which relies on them.
            // Consider moving InitializeMonsterStats to Awake if there are ordering dependencies,
            // or ensure Script Execution Order is set if HealthSystem's Start must run after this.
            // For now, assuming StatsController.AddStat handles existing stats correctly if HealthSystem initializes first.
            InitializeMonsterStats();
        }

        void InitializeMonsterStats()
        {
            if (statsController != null)
            {
                // Using AddStat which also updates if stat already exists.
                statsController.AddStat(StatType.MaxHealth, 50f);
                statsController.AddStat(StatType.Health, 50f); // Current health starts at max
                statsController.AddStat(StatType.AttackPower, 5f);
                statsController.AddStat(StatType.Defense, 0f);
                statsController.AddStat(StatType.MoveSpeed, 3f); // Example move speed for monster
            }
            else
            {
                Debug.LogError("MonsterController: StatsController not found, cannot initialize monster stats.");
            }
        }

        void HandleDeath()
        {
            // Award gold
            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.AddGold(goldValue);
            }
            else
            {
                Debug.LogError("MonsterController: CurrencyManager instance not found! Cannot award gold.");
            }

            // Notify StageManager
            if (StageManager.Instance != null)
            {
                StageManager.Instance.MonsterKilled();
            }
            else
            {
                Debug.LogError("MonsterController: StageManager instance not found! Cannot report kill.");
            }

            Debug.Log(gameObject.name + " death processed by MonsterController. Gold value: " + goldValue);

            // The actual destruction/deactivation is handled by HealthSystem.Die()

            // Example: Disable this controller and AI to stop further actions
            MonsterAI ai = GetComponent<MonsterAI>();
            if (ai != null)
            {
                ai.enabled = false;
            }
            enabled = false;
        }

        void OnDestroy()
        {
            // Unsubscribe from events to prevent memory leaks
            if (healthSystem != null)
            {
                healthSystem.OnDeath -= HandleDeath;
            }
        }
    }
}
