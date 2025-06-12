// C#
// Assets/Scripts/AI/Monster/Conditions/IsPlayerInAggroRangeCondition.cs
using UnityEngine;
using AI.BehaviorTree; // BehaviorTreeRunner에 접근하기 위함

namespace AI.Monster.Conditions
{
    /// <summary>
    /// 플레이어가 몬스터의 어그로 범위 내에 있는지 확인하는 조건입니다.
    /// </summary>
    public class IsPlayerInAggroRangeCondition
    {
        private BehaviorTreeRunner runner;
        private Transform monsterTransform;

        /// <summary>
        /// IsPlayerInAggroRangeCondition 생성자입니다.
        /// </summary>
        /// <param name="runner">조건을 실행하는 BehaviorTreeRunner 인스턴스입니다.</param>
        public IsPlayerInAggroRangeCondition(BehaviorTreeRunner runner)
        {
            this.runner = runner;
            this.monsterTransform = runner.transform;
        }

        /// <summary>
        /// 플레이어가 어그로 범위 내에 있는지 여부를 반환합니다.
        /// </summary>
        /// <returns>플레이어가 범위 내에 있으면 true, 아니면 false를 반환합니다.</returns>
        public bool Evaluate()
        {
            if (runner.PlayerTransform == null)
            {
                // 플레이어 참조가 없으면 감지할 수 없음
                return false;
            }

            float distanceToPlayer = Vector3.Distance(monsterTransform.position, runner.PlayerTransform.position);
            bool inRange = distanceToPlayer <= runner.AggroRange;
            // if (inRange) Debug.Log($"IsPlayerInAggroRange: Player in range ({distanceToPlayer}m / {runner.AggroRange}m)");
            // else Debug.Log($"IsPlayerInAggroRange: Player NOT in range ({distanceToPlayer}m / {runner.AggroRange}m)");
            return inRange;
        }
    }
}
