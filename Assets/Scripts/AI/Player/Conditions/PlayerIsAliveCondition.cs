// C#
// Assets/Scripts/AI/Player/Conditions/PlayerIsAliveCondition.cs
using UnityEngine;
using AI.BehaviorTree; // Required for BehaviorTreeRunner

namespace AI.Player.Conditions
{
    /// <summary>
    /// 플레이어가 살아있는지 확인하는 조건입니다.
    /// BehaviorTreeRunner에 PlayerHealthComponent와 같은 체력 관련 참조가 필요합니다.
    /// </summary>
    public class PlayerIsAliveCondition
    {
        private BehaviorTreeRunner runner;
        // private PlayerHealth playerHealth; // 예시: 플레이어 체력 컴포넌트

        public PlayerIsAliveCondition(BehaviorTreeRunner runner)
        {
            this.runner = runner;
            // this.playerHealth = runner.GetComponent<PlayerHealth>(); // 또는 runner를 통해 접근
            // runner에 PlayerHealthComponent를 가져오는 로직이나 참조가 필요합니다.
            // 예: this.playerHealth = runner.PlayerHealthComponent;
        }

        public bool Evaluate()
        {
            // TODO: 실제 플레이어 체력 시스템과 연동 필요
            // 예시: return playerHealth != null && playerHealth.CurrentHealth > 0;
            // 현재는 항상 살아있는 것으로 간주합니다. Runner에 관련 필드가 없으므로 임시 처리.
            if (runner == null)
            {
                Debug.LogError("PlayerIsAliveCondition: BehaviorTreeRunner가 주입되지 않았습니다.");
                return false;
            }
            // Debug.Log("PlayerIsAliveCondition: 현재는 항상 true 반환 (체력 시스템 연동 필요)");
            return true; // 임시 반환값
        }
    }
}
