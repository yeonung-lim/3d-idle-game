// C#
// Assets/Scripts/AI/Player/Conditions/PlayerTargetIsAliveCondition.cs
using UnityEngine;
using AI.BehaviorTree;

namespace AI.Player.Conditions
{
    /// <summary>
    /// 플레이어의 현재 타겟이 살아있는지 확인하는 조건입니다.
    /// 타겟 오브젝트에 체력 컴포넌트(예: EnemyHealth)가 있고, BehaviorTreeRunner를 통해 접근 가능해야 합니다.
    /// </summary>
    public class PlayerTargetIsAliveCondition
    {
        private BehaviorTreeRunner runner;

        public PlayerTargetIsAliveCondition(BehaviorTreeRunner runner)
        {
            this.runner = runner;
        }

        public bool Evaluate()
        {
            if (runner == null)
            {
                Debug.LogError("PlayerTargetIsAliveCondition: BehaviorTreeRunner가 주입되지 않았습니다.");
                return false;
            }
            // BehaviorTreeRunner를 통해 현재 타겟(runner.CurrentTarget)을 가져오고,
            // 해당 타겟의 체력 컴포넌트를 확인해야 합니다.
            // 예: if (runner.CurrentTarget == null) return false;
            //      Health targetHealth = runner.CurrentTarget.GetComponent<Health>();
            //      return targetHealth != null && targetHealth.IsAlive;

            // Debug.Log("PlayerTargetIsAliveCondition: 현재는 항상 true 반환 (타겟 체력 시스템 연동 필요)");
            // 임시로, 타겟이 있다면 항상 살아있는 것으로 간주 (runner.PlayerTransform을 타겟으로 임시 사용 중)
            // TODO: BehaviorTreeRunner에 CurrentEnemyTarget 필드 및 해당 타겟의 Health 연동 필요
            return runner.PlayerTransform != null; // 임시: 타겟이 존재하면 살아있다고 가정.
        }
    }
}
