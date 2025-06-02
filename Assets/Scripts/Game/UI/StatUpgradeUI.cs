using UnityEngine;
using UnityEngine.UI; // Required for Button
using TMPro;         // Required for TextMeshProUGUI
using Game.Core;     // For StatsController, CurrencyManager, StatType
using Game.Character;  // For PlayerController

namespace Game.UI
{
    public class StatUpgradeUI : MonoBehaviour
    {
        [Header("Player References")]
        public PlayerController playerController; // Assign player from scene/inspector
        private StatsController playerStats;

        [Header("Attack Upgrade UI")]
        public Button upgradeAttackButton;
        public TextMeshProUGUI attackStatText;
        public TextMeshProUGUI attackCostText;

        [Header("Health Upgrade UI")]
        public Button upgradeHealthButton;
        public TextMeshProUGUI healthStatText;
        public TextMeshProUGUI healthCostText;

        [Header("Gold Display (Optional)")]
        public TextMeshProUGUI currentGoldText; // Optional: to display current gold

        [Header("Upgrade Settings")]
        [SerializeField] private int attackUpgradeCost = 10;
        [SerializeField] private int healthUpgradeCost = 10;
        [SerializeField] private float attackUpgradeAmount = 5f;
        [SerializeField] private float healthUpgradeAmount = 20f;
        [SerializeField] private float costIncreaseMultiplier = 1.2f;

        void Start()
        {
            if (playerController == null)
            {
                playerController = FindObjectOfType<PlayerController>();
                if (playerController == null)
                {
                    Debug.LogError("StatUpgradeUI: PlayerController not found in scene and not assigned! UI will not function.", this);
                    enabled = false;
                    return;
                }
            }

            playerStats = playerController.GetComponent<StatsController>();
            if (playerStats == null)
            {
                Debug.LogError("StatUpgradeUI: StatsController not found on PlayerController! UI will not function.", this);
                enabled = false;
                return;
            }

            // Add button listeners
            if (upgradeAttackButton != null) upgradeAttackButton.onClick.AddListener(UpgradeAttack);
            else Debug.LogError("StatUpgradeUI: UpgradeAttackButton not assigned.", this);

            if (upgradeHealthButton != null) upgradeHealthButton.onClick.AddListener(UpgradeHealth);
            else Debug.LogError("StatUpgradeUI: UpgradeHealthButton not assigned.", this);

            // Subscribe to events for UI updates
            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.OnGoldChanged += HandleGoldChanged;
            }
            else Debug.LogWarning("StatUpgradeUI: CurrencyManager.Instance is null. Cannot subscribe to OnGoldChanged.", this);

            // Consider adding an OnStatsChanged event in StatsController for more direct updates
            // For now, UpdateUI() is called after upgrades and on gold changes.

            UpdateUI();
        }

        void UpgradeAttack()
        {
            if (playerStats == null) return;

            if (CurrencyManager.Instance != null && CurrencyManager.Instance.SpendGold(attackUpgradeCost))
            {
                playerStats.ModifyStatValue(StatType.AttackPower, attackUpgradeAmount);
                attackUpgradeCost = Mathf.CeilToInt(attackUpgradeCost * costIncreaseMultiplier);
                UpdateUI();
            }
            else
            {
                Debug.Log("StatUpgradeUI: Not enough gold to upgrade Attack or CurrencyManager not found.");
                // Optionally: Play a sound or show a UI message for "not enough gold"
            }
        }

        void UpgradeHealth()
        {
            if (playerStats == null) return;

            if (CurrencyManager.Instance != null && CurrencyManager.Instance.SpendGold(healthUpgradeCost))
            {
                playerStats.ModifyStatValue(StatType.MaxHealth, healthUpgradeAmount);
                // Also heal the player by the upgrade amount, capped at new MaxHealth
                float currentHealth = playerStats.GetStatValue(StatType.Health);
                float maxHealth = playerStats.GetStatValue(StatType.MaxHealth);
                playerStats.SetStatValue(StatType.Health, Mathf.Min(currentHealth + healthUpgradeAmount, maxHealth));

                healthUpgradeCost = Mathf.CeilToInt(healthUpgradeCost * costIncreaseMultiplier);
                UpdateUI();
            }
            else
            {
                Debug.Log("StatUpgradeUI: Not enough gold to upgrade Health or CurrencyManager not found.");
            }
        }

        public void UpdateUI()
        {
            if (playerStats == null)
            {
                // Clear UI or show error if playerStats is missing
                if(attackStatText) attackStatText.text = "Attack: N/A";
                if(attackCostText) attackCostText.text = "Cost: N/A";
                if(healthStatText) healthStatText.text = "Health: N/A";
                if(healthCostText) healthCostText.text = "Cost: N/A";
                if(currentGoldText && CurrencyManager.Instance != null) currentGoldText.text = "Gold: " + CurrencyManager.Instance.GetCurrentGold();
                else if(currentGoldText) currentGoldText.text = "Gold: N/A";
                return;
            }

            if(attackStatText != null) attackStatText.text = "Attack: " + playerStats.GetStatValue(StatType.AttackPower).ToString("F0"); // F0 for no decimals
            if(attackCostText != null) attackCostText.text = "Cost: " + attackUpgradeCost;
            if(healthStatText != null) healthStatText.text = "Health: " + playerStats.GetStatValue(StatType.Health).ToString("F0") + "/" + playerStats.GetStatValue(StatType.MaxHealth).ToString("F0");
            if(healthCostText != null) healthCostText.text = "Cost: " + healthUpgradeCost;

            if(currentGoldText != null && CurrencyManager.Instance != null)
            {
                currentGoldText.text = "Gold: " + CurrencyManager.Instance.GetCurrentGold();
            }
            else if (currentGoldText != null)
            {
                 currentGoldText.text = "Gold: N/A";
            }

            // Optionally disable buttons if not enough gold
            if(upgradeAttackButton != null && CurrencyManager.Instance != null) upgradeAttackButton.interactable = CurrencyManager.Instance.GetCurrentGold() >= attackUpgradeCost;
            if(upgradeHealthButton != null && CurrencyManager.Instance != null) upgradeHealthButton.interactable = CurrencyManager.Instance.GetCurrentGold() >= healthUpgradeCost;
        }

        private void HandleGoldChanged(int newGoldAmount)
        {
            UpdateUI(); // Refresh UI elements that depend on gold (e.g. button interactability, gold display)
        }

        void OnDestroy()
        {
            // Remove listeners to prevent memory leaks and errors
            if (upgradeAttackButton != null) upgradeAttackButton.onClick.RemoveListener(UpgradeAttack);
            if (upgradeHealthButton != null) upgradeHealthButton.onClick.RemoveListener(UpgradeHealth);

            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.OnGoldChanged -= HandleGoldChanged;
            }
            // If subscribed to other events (e.g. player stat changes), unsubscribe here too.
        }
    }
}
