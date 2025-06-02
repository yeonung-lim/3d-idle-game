using UnityEngine;
using Game.Core; // StatsController와 StatType을 위한 import

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
                Debug.LogError("HealthSystem: 이 GameObject에서 StatsController 컴포넌트를 찾을 수 없습니다.");
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
                // MaxHealth가 있는지 확인한 후 Health를 확인합니다.
                // MaxHealth가 정의되지 않은 경우, 이 개체의 스탯이 완전히 초기화되지 않았을 수 있습니다.
                if (statsController.stats.ContainsKey(StatType.MaxHealth))
                {
                    currentHealth = statsController.GetStatValue(StatType.MaxHealth);
                    // StatsController에 Health 스탯이 있다면 동기화합니다
                    statsController.AddStat(StatType.Health, currentHealth);
                }
                else if (statsController.stats.ContainsKey(StatType.Health))
                {
                    // Health만 정의된 경우의 대체 방안 (덜 이상적임)
                    currentHealth = statsController.GetStatValue(StatType.Health);
                    Debug.LogWarning($"{gameObject.name}: HealthSystem이 Health 스탯으로 초기화되었습니다. 올바른 초기화를 위해 MaxHealth 스탯 추가를 고려하세요.");
                }
                else
                {
                    currentHealth = 100f; // 체력 스탯이 없을 경우 기본값
                    statsController.AddStat(StatType.MaxHealth, currentHealth); // 스탯 추가
                    statsController.AddStat(StatType.Health, currentHealth);
                    Debug.LogWarning($"{gameObject.name}: HealthSystem에서 Health/MaxHealth 스탯을 찾을 수 없습니다. {currentHealth}로 기본 설정됩니다. 스탯이 생성되었습니다.");
                }
            }
            else
            {
                currentHealth = 100f; // StatsController가 없는 경우 대체값
                Debug.LogError($"{gameObject.name}: HealthSystem에 StatsController가 없습니다. 체력이 {currentHealth}로 기본 설정됩니다. 이는 이상적이지 않습니다.");
            }
            Debug.Log($"{gameObject.name}의 체력이 {currentHealth}로 초기화되었습니다.");
        }

        public void TakeDamage(float damageAmount)
        {
            if (currentHealth <= 0) return; // 이미 사망한 상태

            float defense = 0f;
            if (statsController != null && statsController.stats.ContainsKey(StatType.Defense))
            {
                defense = statsController.GetStatValue(StatType.Defense);
            }

            float actualDamage = Mathf.Max(0, damageAmount - defense); // 데미지가 음수가 되지 않도록 보장
            currentHealth -= actualDamage;

            if (statsController != null)
            {
                // StatsController의 Health 스탯도 업데이트
                statsController.SetStatValue(StatType.Health, currentHealth);
            }

            Debug.Log($"{gameObject.name}이(가) {actualDamage}의 데미지를 받았습니다 (공격력: {damageAmount}, 방어력: {defense}). 현재 체력: {currentHealth}");

            if (currentHealth <= 0)
            {
                currentHealth = 0;
                Die(); // 이벤트가 발생하기 전에 Die를 호출하여 객체가 죽은 상태에서 이벤트가 발생하도록 함
                OnDeath?.Invoke();
            }
        }

        private void Die()
        {
            Debug.Log($"{gameObject.name}이(가) HealthSystem을 통해 사망했습니다.");
            // gameObject.SetActive(false); // 옵션 1: 비활성화
            Destroy(gameObject, 0.1f);    // 옵션 2: 제거 (이벤트 처리를 위해 약간의 지연 시간 추가)
        }

        // 선택사항: 치유 메서드
        public void Heal(float amount)
        {
            if (currentHealth <= 0) return; // 사망 상태에서는 치유 불가

            float maxHealth = statsController != null ? statsController.GetStatValue(StatType.MaxHealth) : currentHealth; // 최대 체력이 없는 경우 대체값
            currentHealth += amount;
            if (currentHealth > maxHealth)
            {
                currentHealth = maxHealth;
            }

            if (statsController != null)
            {
                statsController.SetStatValue(StatType.Health, currentHealth);
            }
            Debug.Log($"{gameObject.name}이(가) {amount}만큼 치유되었습니다. 현재 체력: {currentHealth}");
        }
    }
}
