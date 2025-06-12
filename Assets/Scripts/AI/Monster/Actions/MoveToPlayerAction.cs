// C#
// Assets/Scripts/AI/Monster/Actions/MoveToPlayerAction.cs
using UnityEngine;
using UnityEngine.AI;
using AI.BehaviorTree;

namespace AI.Monster.Actions
{
    /// <summary>
    /// 플레이어에게 이동하는 행동입니다. NavMeshAgent를 사용합니다.
    /// </summary>
    public class MoveToPlayerAction
    {
        private BehaviorTreeRunner runner;
        private NavMeshAgent agent;
        private Transform playerTransform;
        private Transform monsterTransform;

        private float stoppingDistance; // NavMeshAgent의 기본 정지 거리 또는 공격 범위와 연동

        /// <summary>
        /// MoveToPlayerAction 생성자입니다.
        /// </summary>
        /// <param name="runner">행동을 실행하는 BehaviorTreeRunner 인스턴스입니다.</param>
        public MoveToPlayerAction(BehaviorTreeRunner runner)
        {
            this.runner = runner;
            this.agent = runner.Agent;
            this.playerTransform = runner.PlayerTransform;
            this.monsterTransform = runner.transform;
            this.stoppingDistance = runner.AttackRange * 0.8f; // 공격 범위의 80% 지점에서 멈추도록 설정 (약간의 여유)
        }

        /// <summary>
        /// 플레이어를 향해 이동합니다.
        /// </summary>
        /// <returns>
        /// 플레이어에게 성공적으로 도달했거나 계속 이동 중이면 Running,
        /// 플레이어 참조가 없거나 NavMeshAgent가 없으면 Failure를 반환합니다.
        /// </returns>
        public NodeState Execute()
        {
            if (playerTransform == null || agent == null || !agent.enabled || !agent.isOnNavMesh)
            {
                Debug.LogWarning("MoveToPlayerAction: PlayerTransform, NavMeshAgent가 유효하지 않거나 NavMesh에 없습니다.");
                return NodeState.Failure;
            }

            agent.SetDestination(playerTransform.position);

            // 플레이어와의 남은 거리가 정지 거리보다 작거나 같으면 성공
            if (agent.remainingDistance <= stoppingDistance && !agent.pathPending)
            {
                // Debug.Log("MoveToPlayerAction: 목표 지점 도달 (Success)");
                agent.ResetPath(); // 다음 행동을 위해 경로 초기화
                return NodeState.Success;
            }

            // Debug.Log($"MoveToPlayerAction: 이동 중... (Remaining: {agent.remainingDistance}, Stopping: {stoppingDistance})");
            return NodeState.Running; // 아직 이동 중
        }
    }
}
