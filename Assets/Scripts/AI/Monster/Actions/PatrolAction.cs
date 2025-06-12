// C#
// Assets/Scripts/AI/Monster/Actions/PatrolAction.cs
using UnityEngine;
using UnityEngine.AI;
using AI.BehaviorTree;

namespace AI.Monster.Actions
{
    /// <summary>
    /// 지정된 범위 내에서 랜덤한 지점으로 이동하며 순찰하는 행동입니다.
    /// </summary>
    public class PatrolAction
    {
        private BehaviorTreeRunner runner;
        private NavMeshAgent agent;
        private Transform monsterTransform;

        private float patrolRadius = 10f; // 순찰 반경
        private float patrolWaitTime = 3f;  // 목적지 도착 후 대기 시간
        private float currentWaitTime = 0f;
        private bool waiting = false;
        private Vector3 startPosition; // 순찰 시작 위치 (몬스터의 초기 위치 기준)

        /// <summary>
        /// PatrolAction 생성자입니다.
        /// </summary>
        /// <param name="runner">행동을 실행하는 BehaviorTreeRunner 인스턴스입니다.</param>
        public PatrolAction(BehaviorTreeRunner runner)
        {
            this.runner = runner;
            this.agent = runner.Agent;
            this.monsterTransform = runner.transform;
            this.startPosition = monsterTransform.position; // 초기 위치를 순찰 기준점으로 사용
        }

        /// <summary>
        /// 순찰 행동을 실행합니다.
        /// </summary>
        /// <returns>항상 Running을 반환하거나, 특정 조건에서 Success/Failure를 반환할 수 있습니다.</returns>
        public NodeState Execute()
        {
            if (agent == null || !agent.enabled || !agent.isOnNavMesh)
            {
                Debug.LogWarning("PatrolAction: NavMeshAgent가 유효하지 않거나 NavMesh에 없습니다.");
                return NodeState.Failure;
            }

            if (waiting)
            {
                currentWaitTime += Time.deltaTime;
                if (currentWaitTime >= patrolWaitTime)
                {
                    waiting = false;
                    // Debug.Log("PatrolAction: 대기 완료, 새 목적지 설정");
                }
                else
                {
                    // Debug.Log($"PatrolAction: 대기 중... ({currentWaitTime:F1}/{patrolWaitTime}s)");
                    return NodeState.Running; // 대기 중일 때는 Running
                }
            }

            // NavMeshAgent가 현재 경로를 계산 중이 아니고, 목적지에 도달했거나 아직 목적지가 설정되지 않았다면 새 목적지 설정
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                if (!waiting) // 이전 상태가 대기가 아니었을 때만 (즉, 방금 도착했거나 처음 시작할 때)
                {
                    // Debug.Log("PatrolAction: 목적지 도달 또는 첫 실행. 대기 시작.");
                    waiting = true;
                    currentWaitTime = 0f;
                    return NodeState.Running; // 목적지 도착 후 대기 시작
                }

                // 대기가 끝났으면 새 목적지 설정
                Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
                randomDirection += startPosition; // 순찰 중심점 기준으로 랜덤 위치
                NavMeshHit hit;
                if (NavMesh.SamplePosition(randomDirection, out hit, patrolRadius, NavMesh.AllAreas))
                {
                    // Debug.Log($"PatrolAction: 새 순찰 지점 설정: {hit.position}");
                    agent.SetDestination(hit.position);
                }
                else
                {
                    // Debug.LogWarning("PatrolAction: 유효한 NavMesh 위치를 찾지 못했습니다. 순찰 반경을 확인하세요.");
                    // 유효한 위치를 못찾으면 잠시 후 다시 시도하도록 Failure 대신 Running 유지 가능
                    return NodeState.Failure; // 또는 Running으로 해서 계속 시도하게 할 수도 있음
                }
            }
            // else if (agent.pathPending)
            // {
            //    Debug.Log("PatrolAction: 경로 계산 중...");
            // }
            // else
            // {
            //    Debug.Log($"PatrolAction: 순찰 지점으로 이동 중... (남은 거리: {agent.remainingDistance})");
            // }

            return NodeState.Running; // 항상 실행 중인 상태로 간주 (순찰은 계속됨)
        }
    }
}
