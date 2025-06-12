// C#
// Assets/Scripts/AI/BehaviorTree/BTNode.cs
using UnityEngine;

namespace AI.BehaviorTree
{
    /// <summary>
    /// 모든 행동 트리 노드의 기본 추상 클래스입니다.
    /// </summary>
    public abstract class BTNode
    {
        protected BehaviorTreeRunner runner;

        /// <summary>
        /// BTNode 생성자입니다.
        /// </summary>
        /// <param name="runner">이 노드를 실행하는 BehaviorTreeRunner 인스턴스입니다.</param>
        public BTNode(BehaviorTreeRunner runner)
        {
            this.runner = runner;
        }

        /// <summary>
        /// 이 노드의 로직을 실행합니다.
        /// </summary>
        /// <returns>노드의 실행 결과 상태 (Success, Failure, Running)를 반환합니다.</returns>
        public abstract NodeState Execute();
    }
}
