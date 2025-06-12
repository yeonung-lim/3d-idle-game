// C#
// Assets/Scripts/AI/Player/Conditions/IsManualModeCondition.cs
using UnityEngine;
using AI.BehaviorTree; // Required for BehaviorTreeRunner

namespace AI.Player.Conditions
{
    /// <summary>
    /// 게임이 수동 모드인지 확인하는 조건입니다.
    /// BehaviorTreeRunner 또는 다른 게임 관리자에서 이 상태를 제공해야 합니다.
    /// </summary>
    public class IsManualModeCondition
    {
        private BehaviorTreeRunner runner;

        public IsManualModeCondition(BehaviorTreeRunner runner)
        {
            this.runner = runner;
        }

        public bool Evaluate()
        {
            if (runner == null)
            {
                Debug.LogError("IsManualModeCondition: BehaviorTreeRunner가 주입되지 않았습니다.");
                return false; // 기본적으로 자동 모드로 가정하거나 오류 처리
            }
            // BehaviorTreeRunner에 'IsManualMode'와 같은 프로퍼티가 있다고 가정합니다.
            // 예: return runner.IsManualMode;
            // Debug.Log("IsManualModeCondition: 현재는 항상 false 반환 (수동/자동 모드 전환 로직 필요)");
            return false; // 임시로 항상 자동 모드(false)로 가정
        }
    }
}
