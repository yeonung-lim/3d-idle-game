// C#
// Assets/Scripts/AI/Monster/Conditions/IsAliveCondition.cs
using UnityEngine;
using AI.BehaviorTree; // BehaviorTreeRunner에 접근하기 위함

namespace AI.Monster.Conditions
{
    /// <summary>
    /// 몬스터가 살아있는지 확인하는 조건입니다.
    /// 실제 Health 컴포넌트와 연동해야 합니다.
    /// </summary>
    public class IsAliveCondition
    {
        private BehaviorTreeRunner runner;
        // private Health monsterHealth; // 실제 Health 컴포넌트 참조

        /// <summary>
        /// IsAliveCondition 생성자입니다.
        /// </summary>
        /// <param name="runner">조건을 실행하는 BehaviorTreeRunner 인스턴스입니다.</param>
        public IsAliveCondition(BehaviorTreeRunner runner)
        {
            this.runner = runner;
            // this.monsterHealth = runner.MonsterHealth; // Runner로부터 Health 컴포넌트 참조를 가져옵니다.
                                                        // BehaviorTreeRunner에 MonsterHealth 프로퍼티 추가 필요
        }

        /// <summary>
        /// 몬스터가 살아있는지 여부를 반환합니다.
        /// </summary>
        /// <returns>살아있으면 true, 아니면 false를 반환합니다.</returns>
        public bool Evaluate()
        {
            // TODO: 실제 Health 컴포넌트의 IsAlive 또는 CurrentHealth > 0 과 같은 로직으로 대체해야 합니다.
            // 예시: return monsterHealth != null && monsterHealth.IsAlive;
            // 현재는 항상 살아있는 것으로 간주합니다.
            if (runner == null)
            {
                Debug.LogError("BehaviorTreeRunner가 IsAliveCondition에 제대로 전달되지 않았습니다.");
                return false;
            }
            // Debug.Log("IsAliveCondition: 현재는 항상 true 반환 (Health 시스템 연동 필요)");
            return true; // 임시로 항상 true 반환
        }
    }
}
