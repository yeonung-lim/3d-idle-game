using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.Core;
using Game.Character;

namespace Game.UI
{
    public class StatUpgradeUI : MonoBehaviour
    {
        [Header("플레이어 참조")]
        public PlayerController playerController; // 씬/인스펙터에서 플레이어 할당
        private StatsController playerStats;

        [Header("공격력 업그레이드 UI")]
        public Button upgradeAttackButton;
        public TextMeshProUGUI attackStatText;
        public TextMeshProUGUI attackCostText;

        [Header("체력 업그레이드 UI")]
        public Button upgradeHealthButton;
        public TextMeshProUGUI healthStatText;
        public TextMeshProUGUI healthCostText;

        [Header("골드 표시 (선택사항)")]
        public TextMeshProUGUI currentGoldText; // 선택사항: 현재 골드 표시용

        [Header("업그레이드 설정")]
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
                    Debug.LogError("StatUpgradeUI: 씬에서 PlayerController를 찾을 수 없고 할당되지 않았습니다! UI가 작동하지 않습니다.", this);
                    enabled = false;
                    return;
                }
            }

            playerStats = playerController.GetComponent<StatsController>();
            if (playerStats == null)
            {
                Debug.LogError("StatUpgradeUI: PlayerController에서 StatsController를 찾을 수 없습니다! UI가 작동하지 않습니다.", this);
                enabled = false;
                return;
            }

            // 버튼 리스너 추가
            if (upgradeAttackButton != null) upgradeAttackButton.onClick.AddListener(UpgradeAttack);
            else Debug.LogError("StatUpgradeUI: UpgradeAttackButton이 할당되지 않았습니다.", this);

            if (upgradeHealthButton != null) upgradeHealthButton.onClick.AddListener(UpgradeHealth);
            else Debug.LogError("StatUpgradeUI: UpgradeHealthButton이 할당되지 않았습니다.", this);

            // UI 업데이트를 위한 이벤트 구독
            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.OnGoldChanged += HandleGoldChanged;
            }
            else Debug.LogWarning("StatUpgradeUI: CurrencyManager.Instance가 null입니다. OnGoldChanged에 구독할 수 없습니다.", this);

            // 더 직접적인 업데이트를 위해 StatsController에 OnStatsChanged 이벤트 추가 고려
            // 현재는 업그레이드와 골드 변경 시 UpdateUI()가 호출됩니다.

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
                Debug.Log("StatUpgradeUI: 공격력 업그레이드를 위한 골드가 부족하거나 CurrencyManager를 찾을 수 없습니다.");
                // 선택사항: "골드 부족" 사운드 재생 또는 UI 메시지 표시
            }
        }

        void UpgradeHealth()
        {
            if (playerStats == null) return;

            if (CurrencyManager.Instance != null && CurrencyManager.Instance.SpendGold(healthUpgradeCost))
            {
                playerStats.ModifyStatValue(StatType.MaxHealth, healthUpgradeAmount);
                // 업그레이드 양만큼 플레이어 체력 회복 (새로운 최대 체력까지)
                float currentHealth = playerStats.GetStatValue(StatType.Health);
                float maxHealth = playerStats.GetStatValue(StatType.MaxHealth);
                playerStats.SetStatValue(StatType.Health, Mathf.Min(currentHealth + healthUpgradeAmount, maxHealth));

                healthUpgradeCost = Mathf.CeilToInt(healthUpgradeCost * costIncreaseMultiplier);
                UpdateUI();
            }
            else
            {
                Debug.Log("StatUpgradeUI: 체력 업그레이드를 위한 골드가 부족하거나 CurrencyManager를 찾을 수 없습니다.");
            }
        }

        public void UpdateUI()
        {
            if (playerStats == null)
            {
                // playerStats가 없는 경우 UI 초기화 또는 에러 표시
                if(attackStatText) attackStatText.text = "공격력: N/A";
                if(attackCostText) attackCostText.text = "비용: N/A";
                if(healthStatText) healthStatText.text = "체력: N/A";
                if(healthCostText) healthCostText.text = "비용: N/A";
                if(currentGoldText && CurrencyManager.Instance != null) currentGoldText.text = "골드: " + CurrencyManager.Instance.GetCurrentGold();
                else if(currentGoldText) currentGoldText.text = "골드: N/A";
                return;
            }

            if(attackStatText != null) attackStatText.text = "공격력: " + playerStats.GetStatValue(StatType.AttackPower).ToString("F0"); // F0는 소수점 없음
            if(attackCostText != null) attackCostText.text = "비용: " + attackUpgradeCost;
            if(healthStatText != null) healthStatText.text = "체력: " + playerStats.GetStatValue(StatType.Health).ToString("F0") + "/" + playerStats.GetStatValue(StatType.MaxHealth).ToString("F0");
            if(healthCostText != null) healthCostText.text = "비용: " + healthUpgradeCost;

            if(currentGoldText != null && CurrencyManager.Instance != null)
            {
                currentGoldText.text = "골드: " + CurrencyManager.Instance.GetCurrentGold();
            }
            else if (currentGoldText != null)
            {
                 currentGoldText.text = "골드: N/A";
            }

            // 골드가 부족한 경우 버튼 비활성화 (선택사항)
            if(upgradeAttackButton != null && CurrencyManager.Instance != null) upgradeAttackButton.interactable = CurrencyManager.Instance.GetCurrentGold() >= attackUpgradeCost;
            if(upgradeHealthButton != null && CurrencyManager.Instance != null) upgradeHealthButton.interactable = CurrencyManager.Instance.GetCurrentGold() >= healthUpgradeCost;
        }

        private void HandleGoldChanged(int newGoldAmount)
        {
            UpdateUI(); // 골드에 의존하는 UI 요소 새로고침 (예: 버튼 상호작용 가능 여부, 골드 표시)
        }

        void OnDestroy()
        {
            // 메모리 누수와 에러 방지를 위해 리스너 제거
            if (upgradeAttackButton != null) upgradeAttackButton.onClick.RemoveListener(UpgradeAttack);
            if (upgradeHealthButton != null) upgradeHealthButton.onClick.RemoveListener(UpgradeHealth);

            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.OnGoldChanged -= HandleGoldChanged;
            }
            // 다른 이벤트에 구독한 경우 (예: 플레이어 스탯 변경), 여기서 구독 해제
        }
    }
}
