// C#
// Assets/Scripts/AI/BehaviorTree/SequenceNode.cs
using System.Collections.Generic;
using UnityEngine;

namespace AI.BehaviorTree
{
    /// <summary>
    /// 모든 자식 노드가 성공하거나 하나라도 실패할 때까지 순차적으로 실행하는 Composite 노드입니다.
    /// 자식 노드 중 하나라도 Failure를 반환하면 즉시 Failure를 반환합니다.
    /// 모든 자식 노드가 Success를 반환하면 Success를 반환합니다.
    /// 자식 노드가 Running을 반환하면 즉시 Running을 반환합니다.
    /// </summary>
    public class SequenceNode : BTNode
    {
        protected List<BTNode> children = new List<BTNode>();

        /// <summary>
        /// SequenceNode 생성자입니다.
        /// </summary>
        /// <param name="runner">이 노드를 실행하는 BehaviorTreeRunner 인스턴스입니다.</param>
        /// <param name="children">이 Sequence의 자식 노드들입니다.</param>
        public SequenceNode(BehaviorTreeRunner runner, List<BTNode> children) : base(runner)
        {
            this.children = children;
        }

        /// <summary>
        /// Sequence 로직을 실행합니다.
        /// </summary>
        /// <returns>노드의 실행 결과 상태 (Success, Failure, Running)를 반환합니다.</returns>
        public override NodeState Execute()
        {
            foreach (BTNode node in children)
            {
                switch (node.Execute())
                {
                    case NodeState.Success:
                        continue; // 다음 자식으로 넘어감
                    case NodeState.Failure:
                        return NodeState.Failure; // 자식 중 하나라도 실패하면 즉시 실패 반환
                    case NodeState.Running:
                        return NodeState.Running; // 자식 중 하나라도 실행 중이면 즉시 실행 중 반환
                    default:
                        continue;
                }
            }
            return NodeState.Success; // 모든 자식이 성공하면 성공 반환
        }
    }
}
