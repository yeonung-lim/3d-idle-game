// C#
// Assets/Scripts/AI/Player/Conditions/PlayerIsInAttackRangeCondition.cs
using UnityEngine;
using AI.BehaviorTree;

namespace AI.Player.Conditions
{
    /// <summary>
    /// 플레이어의 현재 타겟이 공격 범위 내에 있는지 확인하는 조건입니다.
    /// BehaviorTreeRunner에 공격 범위 (예: PlayerAttackRange)와 타겟 참조가 필요합니다.
    /// </summary>
    public class PlayerIsInAttackRangeCondition
    {
        private BehaviorTreeRunner runner;
        private Transform playerSelfTransform; // 플레이어 자신의 Transform

        public PlayerIsInAttackRangeCondition(BehaviorTreeRunner runner)
        {
            this.runner = runner;
            this.playerSelfTransform = runner.transform; // 플레이어 자신의 위치
        }

        public bool Evaluate()
        {
            if (runner == null || runner.PlayerTransform == null) // PlayerTransform을 임시 타겟으로 사용
            {
                // Debug.LogWarning("PlayerIsInAttackRangeCondition: Runner 또는 타겟(임시 PlayerTransform)이 설정되지 않음");
                return false;
            }

            // BehaviorTreeRunner에 'PlayerAttackRange'와 'CurrentTarget' 프로퍼티가 있다고 가정
            // float attackRange = runner.PlayerAttackRange;
            // Transform currentTarget = runner.CurrentTarget;
            // float distanceToTarget = Vector3.Distance(playerSelfTransform.position, currentTarget.position);
            // return distanceToTarget <= attackRange;

            // 임시 로직: BehaviorTreeRunner의 AggroRange를 PlayerAttackRange로 간주 (원래 몬스터용)
            // PlayerTransform을 타겟으로 사용 (원래 몬스터가 플레이어를 추적하기 위한 참조)
            // TODO: BehaviorTreeRunner에 PlayerAttackRange 및 CurrentEnemyTarget 필드 정의 필요
            float distanceToTarget = Vector3.Distance(playerSelfTransform.position, runner.PlayerTransform.position);
            bool inRange = distanceToTarget <= runner.AttackRange; // runner.AttackRange를 플레이어 공격 범위로 임시 사용

            // if (inRange) Debug.Log($"PlayerIsInAttackRange: 타겟이 공격 범위 내에 있음 ({distanceToTarget}m / {runner.AttackRange}m) (임시 로직)");
            // else Debug.Log($"PlayerIsInAttackRange: 타겟이 공격 범위 밖에 있음 ({distanceToTarget}m / {runner.AttackRange}m) (임시 로직)");
            return inRange;
        }
    }
}
