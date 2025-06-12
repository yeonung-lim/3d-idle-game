// C#
// Assets/Scripts/AI/BehaviorTree/ActionNode.cs
using System;
using UnityEngine;

namespace AI.BehaviorTree
{
    /// <summary>
    /// 실제 행동을 수행하는 Leaf 노드입니다.
    /// </summary>
    public class ActionNode : BTNode
    {
        private readonly Func<NodeState> action;

        /// <summary>
        /// ActionNode 생성자입니다.
        /// </summary>
        /// <param name="runner">이 노드를 실행하는 BehaviorTreeRunner 인스턴스입니다.</param>
        /// <param name="action">실행할 행동을 정의하는 함수입니다. 이 함수는 NodeState를 반환해야 합니다.</param>
        public ActionNode(BehaviorTreeRunner runner, Func<NodeState> action) : base(runner)
        {
            this.action = action;
        }

        /// <summary>
        /// 정의된 행동을 실행합니다.
        /// </summary>
        /// <returns>행동의 실행 결과 (Success, Failure, Running)를 반환합니다.</returns>
        public override NodeState Execute()
        {
            return action();
        }
    }
}
