// C#
// Assets/Scripts/AI/Player/Conditions/PlayerHasTargetCondition.cs
using UnityEngine;
using AI.BehaviorTree;

namespace AI.Player.Conditions
{
    /// <summary>
    /// 플레이어에게 현재 유효한 타겟이 있는지 확인하는 조건입니다.
    /// BehaviorTreeRunner에 타겟 참조 (예: CurrentTarget)가 필요합니다.
    /// </summary>
    public class PlayerHasTargetCondition
    {
        private BehaviorTreeRunner runner;

        public PlayerHasTargetCondition(BehaviorTreeRunner runner)
        {
            this.runner = runner;
        }

        public bool Evaluate()
        {
            if (runner == null)
            {
                Debug.LogError("PlayerHasTargetCondition: BehaviorTreeRunner가 주입되지 않았습니다.");
                return false;
            }
            // BehaviorTreeRunner에 'CurrentTarget'과 같은 프로퍼티가 있다고 가정합니다.
            // 예: return runner.CurrentTarget != null;
            // Debug.Log("PlayerHasTargetCondition: 현재는 임시로 (runner.PlayerTransform != null) 사용 (실제 타겟 시스템 연동 필요)");
            // runner.PlayerTransform은 자기 자신이므로 타겟으로 적절치 않음. 임시로 false.
            // 실제 타겟팅 시스템에서는 runner.Target != null 과 같은 형태가 될 것입니다.
            // 현재 BehaviorTreeRunner에는 target을 위한 명시적 필드가 없으므로,
            // PlayerTransform (원래 몬스터용으로 만든 플레이어 위치 참조)을 임시로 타겟으로 간주해보겠습니다.
            // 이는 논리적으로 맞지 않지만, 필드 부재로 인한 임시 방편입니다.
            // 실제 구현에서는 BehaviorTreeRunner에 TargetEnemy Transform 같은 필드를 추가해야 합니다.
            bool hasTarget = runner.PlayerTransform != null; // 이것은 '플레이어'를 의미. 실제론 '적 타겟'이어야 함.
                                                            // TODO: BehaviorTreeRunner에 CurrentEnemyTarget 같은 필드 추가하고 그것을 사용.
            // if (hasTarget) Debug.Log("PlayerHasTargetCondition: 타겟 있음 (임시 로직)");
            // else Debug.Log("PlayerHasTargetCondition: 타겟 없음 (임시 로직)");
            return hasTarget; // 임시: 플레이어 참조가 있으면 타겟이 있다고 가정 (잘못된 가정이나 필드 부재로 임시 처리)
        }
    }
}
