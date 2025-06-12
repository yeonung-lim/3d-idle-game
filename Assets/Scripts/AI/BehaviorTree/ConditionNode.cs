// C#
// Assets/Scripts/AI/BehaviorTree/ConditionNode.cs
using System;
using UnityEngine;

namespace AI.BehaviorTree
{
    /// <summary>
    /// 주어진 조건이 참일 경우에만 자식 노드를 실행하는 Decorator 노드입니다.
    /// </summary>
    public class ConditionNode : BTNode
    {
        private readonly Func<bool> condition;
        private readonly BTNode childNode;

        /// <summary>
        /// ConditionNode 생성자입니다.
        /// </summary>
        /// <param name="runner">이 노드를 실행하는 BehaviorTreeRunner 인스턴스입니다.</param>
        /// <param name="condition">실행 전에 평가할 조건입니다.</param>
        /// <param name="childNode">조건이 참일 경우 실행할 자식 노드입니다.</param>
        public ConditionNode(BehaviorTreeRunner runner, Func<bool> condition, BTNode childNode) : base(runner)
        {
            this.condition = condition;
            this.childNode = childNode;
        }

        /// <summary>
        /// 조건을 평가하고, 참이면 자식 노드를 실행합니다.
        /// </summary>
        /// <returns>조건이 거짓이면 Failure를 반환하고, 참이면 자식 노드의 실행 결과를 반환합니다.</returns>
        public override NodeState Execute()
        {
            if (condition())
            {
                return childNode.Execute();
            }
            else
            {
                return NodeState.Failure;
            }
        }
    }
}
