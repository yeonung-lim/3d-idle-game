using UnityEngine;
using Game.Core;
using Game.Combat;

namespace Game.Monster
{
    [RequireComponent(typeof(StatsController))]
    [RequireComponent(typeof(HealthSystem))] // HealthSystem 컴포넌트가 필수적으로 필요
    public class MonsterController : MonoBehaviour
    {
        public int goldValue = 10; // 몬스터 처치 시 획득하는 골드

        private StatsController statsController;
        private HealthSystem healthSystem;

        void Awake()
        {
            statsController = GetComponent<StatsController>();
            if (statsController == null)
            {
                Debug.LogError("MonsterController: 이 게임오브젝트에서 StatsController 컴포넌트를 찾을 수 없습니다.");
            }

            healthSystem = GetComponent<HealthSystem>();
            if (healthSystem == null)
            {
                Debug.LogError("MonsterController: 이 게임오브젝트에서 HealthSystem 컴포넌트를 찾을 수 없습니다.");
            }
            else
            {
                healthSystem.OnDeath += HandleDeath;
            }
        }

        void Start()
        {
            // 스탯은 HealthSystem의 Start 메서드 실행 전에 초기화되어야 합니다.
            // 실행 순서에 의존성이 있는 경우 InitializeMonsterStats를 Awake로 이동하거나,
            // HealthSystem의 Start가 반드시 이후에 실행되어야 하는 경우 Script Execution Order를 설정하세요.
            // 현재는 HealthSystem이 먼저 초기화되더라도 StatsController.AddStat이 기존 스탯을 올바르게 처리한다고 가정합니다.
            InitializeMonsterStats();
        }

        void InitializeMonsterStats()
        {
            if (statsController != null)
            {
                // AddStat을 사용하여 스탯이 이미 존재하는 경우에도 업데이트
                statsController.AddStat(StatType.MaxHealth, 50f);
                statsController.AddStat(StatType.Health, 50f); // 현재 체력은 최대 체력으로 시작
                statsController.AddStat(StatType.AttackPower, 5f);
                statsController.AddStat(StatType.Defense, 0f);
                statsController.AddStat(StatType.MoveSpeed, 3f); // 몬스터의 이동 속도 예시
            }
            else
            {
                Debug.LogError("MonsterController: StatsController를 찾을 수 없어 몬스터 스탯을 초기화할 수 없습니다.");
            }
        }

        void HandleDeath()
        {
            // 골드 지급
            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.AddGold(goldValue);
            }
            else
            {
                Debug.LogError("MonsterController: CurrencyManager 인스턴스를 찾을 수 없습니다! 골드를 지급할 수 없습니다.");
            }

            // StageManager에 알림
            if (StageManager.Instance != null)
            {
                StageManager.Instance.MonsterKilled();
            }
            else
            {
                Debug.LogError("MonsterController: StageManager 인스턴스를 찾을 수 없습니다! 처치 보고를 할 수 없습니다.");
            }

            Debug.Log(gameObject.name + " 몬스터가 처치되었습니다. 획득 골드: " + goldValue);

            // 실제 파괴/비활성화는 HealthSystem.Die()에서 처리됩니다

            // 예시: 추가 행동을 멈추기 위해 이 컨트롤러와 AI를 비활성화
            MonsterAI ai = GetComponent<MonsterAI>();
            if (ai != null)
            {
                ai.enabled = false;
            }
            enabled = false;
        }

        void OnDestroy()
        {
            // 메모리 누수 방지를 위해 이벤트 구독 해제
            if (healthSystem != null)
            {
                healthSystem.OnDeath -= HandleDeath;
            }
        }
    }
}
