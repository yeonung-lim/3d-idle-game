// C#
// Assets/Scripts/AI/BehaviorTree/SelectorNode.cs
using System.Collections.Generic;
using UnityEngine;

namespace AI.BehaviorTree
{
    /// <summary>
    /// 자식 노드 중 하나가 성공하거나 모두 실패할 때까지 순차적으로 실행하는 Composite 노드입니다.
    /// 자식 노드 중 하나라도 Success를 반환하면 즉시 Success를 반환합니다.
    /// 모든 자식 노드가 Failure를 반환하면 Failure를 반환합니다.
    /// 자식 노드가 Running을 반환하면 즉시 Running을 반환합니다.
    /// </summary>
    public class SelectorNode : BTNode
    {
        protected List<BTNode> children = new List<BTNode>();

        /// <summary>
        /// SelectorNode 생성자입니다.
        /// </summary>
        /// <param name="runner">이 노드를 실행하는 BehaviorTreeRunner 인스턴스입니다.</param>
        /// <param name="children">이 Selector의 자식 노드들입니다.</param>
        public SelectorNode(BehaviorTreeRunner runner, List<BTNode> children) : base(runner)
        {
            this.children = children;
        }

        /// <summary>
        /// Selector 로직을 실행합니다.
        /// </summary>
        /// <returns>노드의 실행 결과 상태 (Success, Failure, Running)를 반환합니다.</returns>
        public override NodeState Execute()
        {
            foreach (BTNode node in children)
            {
                switch (node.Execute())
                {
                    case NodeState.Success:
                        return NodeState.Success; // 자식 중 하나라도 성공하면 즉시 성공 반환
                    case NodeState.Failure:
                        continue; // 다음 자식으로 넘어감
                    case NodeState.Running:
                        return NodeState.Running; // 자식 중 하나라도 실행 중이면 즉시 실행 중 반환
                    default:
                        continue;
                }
            }
            return NodeState.Failure; // 모든 자식이 실패하면 실패 반환
        }
    }
}
