// C#
// Assets/Scripts/AI/Player/Actions/PlayerAttackAction.cs
using UnityEngine;
using AI.BehaviorTree;

namespace AI.Player.Actions
{
    /// <summary>
    /// 플레이어가 현재 타겟을 공격하는 행동입니다.
    /// 공격 애니메이션, 데미지 적용, 공격 쿨타임 관리가 필요합니다.
    /// </summary>
    public class PlayerAttackAction
    {
        private BehaviorTreeRunner runner;
        private Transform playerSelfTransform;

        // BehaviorTreeRunner 또는 이 클래스 내에서 관리될 수 있는 공격 쿨타임 관련 변수
        // 예시: private float playerAttackCooldown = 1.5f;
        //       private float lastAttackTimestamp = -Mathf.Infinity;
        //       runner를 통해 공유해야 CanAttackCondition과 동기화됨.

        public PlayerAttackAction(BehaviorTreeRunner runner)
        {
            this.runner = runner;
            this.playerSelfTransform = runner.transform;
            // TODO: runner로부터 PlayerAttackCooldown, LastPlayerAttackTimestamp 같은 값을 가져오거나 설정할 수 있어야 함.
            // 예: this.playerAttackCooldown = runner.PlayerAttackCooldown;
            //     this.lastAttackTimestamp = runner.LastPlayerAttackTimestamp; // 읽기 전용일 수 있음
        }

        public NodeState Execute()
        {
            if (runner == null)
            {
                Debug.LogError("PlayerAttackAction: BehaviorTreeRunner가 주입되지 않았습니다.");
                return NodeState.Failure;
            }

            // Transform currentTarget = runner.CurrentTarget; // Runner에 CurrentTarget 필드 필요
            Transform currentTarget = runner.PlayerTransform; // 임시로 PlayerTransform을 타겟으로 사용 (몬스터 AI의 잔재)
                                                            // TODO: BehaviorTreeRunner에 CurrentEnemyTarget 같은 필드 필요

            if (currentTarget == null)
            {
                // Debug.LogWarning("PlayerAttackAction: 공격할 타겟이 없습니다.");
                return NodeState.Failure; // 타겟이 없으면 실패
            }

            // PlayerCanAttackCondition에서 이미 쿨타임 체크를 했을 것이므로 여기서는 공격 실행에 집중
            // 하지만, 이 Action이 직접 쿨타임을 관리하고 싶다면 여기서도 체크 가능
            // float configuredAttackCooldown = runner.AttackRange; // 임시로 몬스터의 AttackRange를 쿨타임으로 사용. 매우 잘못된 가정.
                                                                // TODO: runner.PlayerAttackCooldown 같은 필드 필요
            // if (Time.time < runner.LastPlayerAttackTime + configuredAttackCooldown) {
            //    return NodeState.Running; // 아직 쿨타임 중
            // }


            // Debug.Log($"PlayerAttackAction: {playerSelfTransform.name}이(가) 타겟 {currentTarget.name}을(를) 공격합니다! (실제 공격 로직 필요)");
            // TODO: 실제 공격 로직 구현 (애니메이션 재생, 데미지 전달)
            // TODO: 공격 후 runner.LastPlayerAttackTime = Time.time; 와 같이 마지막 공격 시간 업데이트

            // 공격 애니메이션이 있다면, 애니메이션 시작 시 Running 반환, 완료 시 Success 반환 구조도 가능.
            // 여기서는 즉시 완료되는 것으로 가정.
            return NodeState.Success;
        }
    }
}
