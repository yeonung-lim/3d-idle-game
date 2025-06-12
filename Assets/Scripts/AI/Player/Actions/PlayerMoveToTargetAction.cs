// C#
// Assets/Scripts/AI/Player/Actions/PlayerMoveToTargetAction.cs
using UnityEngine;
using UnityEngine.AI; // NavMeshAgent 사용
using AI.BehaviorTree;

namespace AI.Player.Actions
{
    /// <summary>
    /// 플레이어가 현재 타겟에게 이동하는 행동입니다. NavMeshAgent를 사용합니다.
    /// </summary>
    public class PlayerMoveToTargetAction
    {
        private BehaviorTreeRunner runner;
        private NavMeshAgent agent;
        private Transform playerSelfTransform;
        // private float targetStoppingDistance; // 타겟과의 정지 거리 (공격 범위 등과 연동)

        public PlayerMoveToTargetAction(BehaviorTreeRunner runner)
        {
            this.runner = runner;
            this.agent = runner.Agent; // BehaviorTreeRunner는 NavMeshAgent를 가지고 있음
            this.playerSelfTransform = runner.transform;
            // this.targetStoppingDistance = runner.PlayerAttackRange * 0.9f; // 공격 범위의 90% 지점에서 멈추도록 설정
            // TODO: runner.PlayerAttackRange 같은 필드 필요
        }

        public NodeState Execute()
        {
            if (runner == null || agent == null || !agent.enabled || !agent.isOnNavMesh)
            {
                Debug.LogError("PlayerMoveToTargetAction: Runner 또는 NavMeshAgent가 유효하지 않거나 NavMesh 위에 없습니다.");
                return NodeState.Failure;
            }

            // Transform currentTarget = runner.CurrentTarget; // Runner에 CurrentTarget 필드 필요
            Transform currentTarget = runner.PlayerTransform; // 임시로 PlayerTransform을 타겟으로 사용 (몬스터 AI의 잔재)
                                                            // TODO: BehaviorTreeRunner에 CurrentEnemyTarget 같은 필드 필요

            if (currentTarget == null)
            {
                // Debug.LogWarning("PlayerMoveToTargetAction: 이동할 타겟이 없습니다.");
                return NodeState.Failure; // 타겟이 없으면 실패
            }

            // NavMeshAgent의 정지 거리를 플레이어의 공격 범위에 맞춰 설정할 수 있습니다.
            // agent.stoppingDistance = runner.PlayerAttackRange * 0.9f; // 매번 설정하거나 초기화 시 설정
            // 현재는 BehaviorTreeRunner의 AttackRange를 임시 사용 (몬스터용 AggroRange/AttackRange 중 AttackRange)
            float currentStoppingDistance = runner.AttackRange * 0.9f; // 임시 설정
            agent.stoppingDistance = currentStoppingDistance;


            agent.SetDestination(currentTarget.position);

            // 목표 지점에 거의 도달했는지 확인 (NavMeshAgent의 기본 정지 거리 또는 설정된 정지 거리 고려)
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                // Debug.Log($"PlayerMoveToTargetAction: 타겟 ({currentTarget.name}) 근처 도착 (Success)");
                // agent.ResetPath(); // 경로 초기화는 상황에 따라 필요 없을 수 있음 (계속 타겟을 따라갈 것이라면)
                return NodeState.Success; // 목표 도달
            }

            // Debug.Log($"PlayerMoveToTargetAction: 타겟 ({currentTarget.name})으로 이동 중... 남은 거리: {agent.remainingDistance:F1}m");
            return NodeState.Running; // 아직 이동 중
        }
    }
}
