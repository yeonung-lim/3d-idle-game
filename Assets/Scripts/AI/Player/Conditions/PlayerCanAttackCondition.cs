// C#
// Assets/Scripts/AI/Player/Conditions/PlayerCanAttackCondition.cs
using UnityEngine;
using AI.BehaviorTree;

namespace AI.Player.Conditions
{
    /// <summary>
    /// 플레이어가 현재 공격할 수 있는지 (예: 공격 쿨타임) 확인하는 조건입니다.
    /// 공격 쿨타임 관리는 BehaviorTreeRunner 또는 PlayerAttackAction 내부에서 처리될 수 있습니다.
    /// </summary>
    public class PlayerCanAttackCondition
    {
        private BehaviorTreeRunner runner;
        // private float lastAttackTime; // 이 값은 PlayerAttackAction에서 관리하고 Runner를 통해 공유하거나,
                                      // Runner 자체에서 관리할 수 있습니다.
        // private float attackCooldown;

        public PlayerCanAttackCondition(BehaviorTreeRunner runner)
        {
            this.runner = runner;
            // this.attackCooldown = runner.PlayerAttackCooldown; // Runner에서 쿨다운 값 가져오기
            // lastAttackTime은 해당 ActionNode 실행 시 업데이트 되므로, 여기서는 직접 참조하기 어려울 수 있음.
            // 가장 간단한 방법은 BehaviorTreeRunner가 마지막 공격 시간을 관리하는 것입니다.
            // 예: this.lastAttackTime = runner.LastPlayerAttackTime;
        }

        public bool Evaluate()
        {
            if (runner == null)
            {
                 Debug.LogError("PlayerCanAttackCondition: BehaviorTreeRunner가 주입되지 않았습니다.");
                return false;
            }
            // BehaviorTreeRunner에 'LastPlayerAttackTime'과 'PlayerAttackCooldown' 프로퍼티가 있다고 가정
            // return Time.time >= runner.LastPlayerAttackTime + runner.PlayerAttackCooldown;

            // Debug.Log("PlayerCanAttackCondition: 현재는 항상 true 반환 (공격 쿨타임 시스템 연동 필요)");
            // TODO: BehaviorTreeRunner에 공격 쿨타임 관련 필드 (LastAttackTime, AttackCooldown) 추가 및 연동 필요
            return true; // 임시로 항상 공격 가능
        }
    }
}
