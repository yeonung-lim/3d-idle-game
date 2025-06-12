// C#
// Assets/Scripts/AI/Monster/Actions/AttackPlayerAction.cs
using UnityEngine;
using AI.BehaviorTree;

namespace AI.Monster.Actions
{
    /// <summary>
    /// 플레이어를 공격하는 행동입니다. (현재는 로그만 출력)
    /// </summary>
    public class AttackPlayerAction
    {
        private BehaviorTreeRunner runner;
        private Transform playerTransform;
        private Transform monsterTransform;

        // 공격 관련 파라미터 (애니메이션, 실제 데미지 처리 등은 여기에 추가)
        private float attackCooldown = 2f; // 공격 쿨타임
        private float lastAttackTime = -Mathf.Infinity; // 마지막 공격 시간

        /// <summary>
        /// AttackPlayerAction 생성자입니다.
        /// </summary>
        /// <param name="runner">행동을 실행하는 BehaviorTreeRunner 인스턴스입니다.</param>
        public AttackPlayerAction(BehaviorTreeRunner runner)
        {
            this.runner = runner;
            this.playerTransform = runner.PlayerTransform;
            this.monsterTransform = runner.transform;
        }

        /// <summary>
        /// 플레이어를 공격합니다.
        /// </summary>
        /// <returns>
        /// 공격 범위 내에 있고 쿨타임이 지났으면 공격 후 Success,
        /// 범위 밖이거나 쿨타임 중이면 Failure (또는 Running으로 설계 가능) 를 반환합니다.
        /// </returns>
        public NodeState Execute()
        {
            if (playerTransform == null)
            {
                Debug.LogWarning("AttackPlayerAction: PlayerTransform이 유효하지 않습니다.");
                return NodeState.Failure;
            }

            float distanceToPlayer = Vector3.Distance(monsterTransform.position, playerTransform.position);

            if (distanceToPlayer <= runner.AttackRange)
            {
                if (Time.time >= lastAttackTime + attackCooldown)
                {
                    // 공격 실행
                    Debug.Log($"AttackPlayerAction: {monsterTransform.name}이(가) 플레이어를 공격합니다!");
                    // TODO: 실제 공격 로직 (애니메이션 재생, 데미지 전달 등) 구현
                    lastAttackTime = Time.time;
                    // 공격 애니메이션 등이 있다면 Running 상태를 반환하고, 애니메이션 완료 시 Success를 반환하도록 수정 가능
                    return NodeState.Success;
                }
                else
                {
                    // Debug.Log("AttackPlayerAction: 공격 쿨타임 중...");
                    return NodeState.Running; // 쿨타임 중에는 대기 (Running)
                }
            }
            else
            {
                // Debug.Log("AttackPlayerAction: 플레이어가 공격 범위 밖에 있습니다.");
                return NodeState.Failure; // 공격 범위 밖
            }
        }
    }
}
