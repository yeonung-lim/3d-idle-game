using UnityEngine;
using Game.Core; // For StatsController and StatType

namespace Game.Combat
{
    [RequireComponent(typeof(StatsController))]
    public class HealthSystem : MonoBehaviour
    {
        private StatsController statsController;
        public float currentHealth { get; private set; }

        public event System.Action OnDeath;

        void Awake()
        {
            statsController = GetComponent<StatsController>();
            if (statsController == null)
            {
                Debug.LogError("HealthSystem: StatsController component not found on this GameObject.");
            }
        }

        void Start()
        {
            InitializeHealth();
        }

        public void InitializeHealth()
        {
            if (statsController != null)
            {
                // Ensure MaxHealth is present, then Health.
                // If MaxHealth isn't defined, it might mean stats aren't fully initialized for this entity.
                if (statsController.stats.ContainsKey(StatType.MaxHealth))
                {
                    currentHealth = statsController.GetStatValue(StatType.MaxHealth);
                    // Also ensure the Health stat in StatsController is synchronized if it exists
                    statsController.AddStat(StatType.Health, currentHealth);
                }
                else if (statsController.stats.ContainsKey(StatType.Health))
                {
                    // Fallback if only Health is defined (less ideal)
                    currentHealth = statsController.GetStatValue(StatType.Health);
                     Debug.LogWarning($"{gameObject.name}: HealthSystem initialized from Health stat. Consider adding MaxHealth stat for proper initialization.");
                }
                else
                {
                    currentHealth = 100f; // Default fallback if no health stats are found
                    statsController.AddStat(StatType.MaxHealth, currentHealth); // Add them
                    statsController.AddStat(StatType.Health, currentHealth);
                    Debug.LogWarning($"{gameObject.name}: HealthSystem found no Health/MaxHealth stats. Defaulting to {currentHealth}. Stats created.");
                }
            }
            else
            {
                currentHealth = 100f; // Fallback if no StatsController
                Debug.LogError($"{gameObject.name}: HealthSystem has no StatsController. Defaulting health to {currentHealth}. This is not ideal.");
            }
             Debug.Log($"{gameObject.name} initialized with {currentHealth} health.");
        }

        public void TakeDamage(float damageAmount)
        {
            if (currentHealth <= 0) return; // Already dead

            float defense = 0f;
            if (statsController != null && statsController.stats.ContainsKey(StatType.Defense))
            {
                defense = statsController.GetStatValue(StatType.Defense);
            }

            float actualDamage = Mathf.Max(0, damageAmount - defense); // Ensure damage isn't negative
            currentHealth -= actualDamage;

            if (statsController != null)
            {
                // Update the Health stat in StatsController as well
                statsController.SetStatValue(StatType.Health, currentHealth);
            }

            Debug.Log($"{gameObject.name} took {actualDamage} damage (Damage: {damageAmount}, Defense: {defense}). Current health: {currentHealth}");

            if (currentHealth <= 0)
            {
                currentHealth = 0;
                Die(); // Call Die before invoking OnDeath, so object is dead when event fires
                OnDeath?.Invoke();
            }
        }

        private void Die()
        {
            Debug.Log($"{gameObject.name} has died through HealthSystem.");
            // gameObject.SetActive(false); // Option 1: Deactivate
            Destroy(gameObject, 0.1f);    // Option 2: Destroy (slight delay to allow event processing)
        }

        // Optional: Method to heal
        public void Heal(float amount)
        {
            if (currentHealth <= 0) return; // Cannot heal if dead

            float maxHealth = statsController != null ? statsController.GetStatValue(StatType.MaxHealth) : currentHealth; // Fallback if no max health
            currentHealth += amount;
            if (currentHealth > maxHealth)
            {
                currentHealth = maxHealth;
            }

            if (statsController != null)
            {
                statsController.SetStatValue(StatType.Health, currentHealth);
            }
            Debug.Log($"{gameObject.name} healed by {amount}. Current health: {currentHealth}");
        }
    }
}
