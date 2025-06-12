// C#
// Assets/Scripts/AI/Player/Actions/PlayerWaitForInputAction.cs
using UnityEngine;
using AI.BehaviorTree;

namespace AI.Player.Actions
{
    /// <summary>
    /// 플레이어가 수동 입력 대기 상태일 때 실행되는 행동입니다.
    /// AI는 이 상태에서 특별한 행동을 하지 않고 Running 상태를 반환하여 대기합니다.
    /// </summary>
    public class PlayerWaitForInputAction
    {
        private BehaviorTreeRunner runner;

        public PlayerWaitForInputAction(BehaviorTreeRunner runner)
        {
            this.runner = runner;
        }

        public NodeState Execute()
        {
            if (runner == null)
            {
                Debug.LogError("PlayerWaitForInputAction: BehaviorTreeRunner가 주입되지 않았습니다.");
                return NodeState.Failure;
            }
            // Debug.Log("PlayerWaitForInputAction: 수동 입력 대기 중...");
            // 수동 모드에서는 AI가 다른 행동을 하지 않도록 Running을 반환하여 현재 상태를 유지합니다.
            // 또는 Success를 반환하여 트리가 다른 부분을 탐색하지 않도록 할 수도 있습니다.
            // 여기서는 Running을 반환하여 이 노드가 계속 활성 상태임을 나타냅니다.
            return NodeState.Running;
        }
    }
}
