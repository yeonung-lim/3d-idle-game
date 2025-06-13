// C#
// Assets/Scripts/AI/BehaviorTree/BehaviorTreeRunner.cs
using UnityEngine;
using UnityEngine.AI; // NavMeshAgent를 사용하기 위해 추가

namespace AI.BehaviorTree
{
    /// <summary>
    /// 행동 트리를 실행하는 MonoBehaviour입니다.
    /// 이 컴포넌트를 게임 오브젝트에 추가하고 루트 노드를 설정하여 BT를 실행합니다.
    /// 플레이어 및 몬스터 AI 모두에 사용될 수 있도록 확장됩니다.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class BehaviorTreeRunner : MonoBehaviour
    {
        private BTNode rootNode;

        [Header("Core Components")] public NavMeshAgent Agent;
        // public HealthComponent SelfHealth { get; private set; } // 예시: 자신의 체력 컴포넌트
        [Header("Player AI Specific")] public Transform CurrentTarget; // 플레이어의 현재 공격 타겟
        
        public bool IsManualMode { get; set; } = false; // 기본값: 자동 모드
        public float AttackRange { get; set; } = 2.0f; // 플레이어 공격 범위
        public float AttackCooldown { get; set; } = 1.5f; // 플레이어 공격 쿨타임
        public float LastPlayerAttackTime { get; set; } = -Mathf.Infinity; // 마지막 공격 시간

        [Header("Monster AI Specific (Legacy/Example)")]
        // 아래는 몬스터 AI 예제에서 사용되었던 필드들입니다.
        // 플레이어 AI와 함께 사용하거나, 별도의 Runner 또는 설정 클래스로 분리할 수 있습니다.
        public Transform PlayerTransform; // 몬스터가 플레이어를 추적하기 위한 참조 (플레이어 AI에서는 사용되지 않을 수 있음)
        public float AggroRange { get; set; } = 10f; // 몬스터 어그로 범위
        // public float MonsterAttackRange { get; set; } = 2f; // 몬스터 공격 범위 (위의 PlayerAttackRange와 구분)


        void Awake()
        {
            Agent = GetComponent<NavMeshAgent>();
            // SelfHealth = GetComponent<HealthComponent>(); // 자신의 체력 컴포넌트 초기화

            // 플레이어 AI의 경우, CurrentTarget은 외부에서 (예: 타겟팅 시스템) 설정해야 합니다.
            // 몬스터 AI의 경우, PlayerTransform을 찾아 설정할 수 있습니다.
            if (gameObject.CompareTag("Player")) // 이 Runner가 플레이어에게 붙어있다면
            {
                // 플레이어 관련 초기화 (필요하다면)
            }
            else if (gameObject.CompareTag("Monster")) // 이 Runner가 몬스터에게 붙어있다면
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    PlayerTransform = playerObj.transform;
                }
                else
                {
                    Debug.LogWarning("BehaviorTreeRunner (Monster): Player Transform을 찾을 수 없습니다. 'Player' 태그가 설정된 오브젝트가 있는지 확인하세요.");
                }
            }
        }

        /// <summary>
        /// 이 BehaviorTreeRunner에 대한 루트 노드를 설정합니다.
        /// </summary>
        /// <param name="node">실행할 루트 노드입니다.</param>
        public void SetRootNode(BTNode node)
        {
            rootNode = node;
        }

        void Update()
        {
            if (rootNode != null)
            {
                rootNode.Execute();
            }
        }

        // 예시: 외부에서 타겟을 설정하는 메소드 (플레이어 AI용)
        public void SetEnemyTarget(Transform newTarget)
        {
            CurrentTarget = newTarget;
        }
    }
}
