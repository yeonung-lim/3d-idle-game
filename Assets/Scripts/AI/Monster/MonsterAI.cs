// C#
// Assets/Scripts/AI/Monster/MonsterAI.cs
using UnityEngine;
using AI.BehaviorTree;
using AI.Monster.Conditions; // Condition 클래스들을 사용하기 위함
using AI.Monster.Actions;   // Action 클래스들을 사용하기 위함
using System.Collections.Generic;

namespace AI.Monster
{
    /// <summary>
    /// 몬스터의 행동 트리를 설정하고 BehaviorTreeRunner에 연결합니다.
    /// </summary>
    [RequireComponent(typeof(BehaviorTreeRunner))]
    public class MonsterAI : MonoBehaviour
    {
        private BehaviorTreeRunner btRunner;

        void Start()
        {
            btRunner = GetComponent<BehaviorTreeRunner>();
            if (btRunner == null)
            {
                Debug.LogError("MonsterAI: BehaviorTreeRunner 컴포넌트를 찾을 수 없습니다.");
                enabled = false; // BehaviorTreeRunner가 없으면 이 스크립트 비활성화
                return;
            }

            ConstructBehaviorTree();
        }

        /// <summary>
        /// 몬스터의 행동 트리를 구성합니다.
        /// </summary>
        private void ConstructBehaviorTree()
        {
            // 조건 인스턴스 생성
            IsAliveCondition isAliveCondition = new IsAliveCondition(btRunner);
            IsPlayerInAggroRangeCondition isPlayerInAggroRangeCondition = new IsPlayerInAggroRangeCondition(btRunner);

            // 행동 인스턴스 생성
            MoveToPlayerAction moveToPlayerAction = new MoveToPlayerAction(btRunner);
            AttackPlayerAction attackPlayerAction = new AttackPlayerAction(btRunner);
            PatrolAction patrolAction = new PatrolAction(btRunner);

            // 행동 트리 구성
            // 루트 노드
            //  - IsAlive? (ConditionNode)
            //    - Selector
            //      - IsPlayerInAggroRange? (ConditionNode)
            //        - Sequence
            //          - MoveToPlayer (ActionNode)
            //          - AttackPlayer (ActionNode)
            //      - Patrol (ActionNode)

            BTNode rootNode = new ConditionNode(btRunner, isAliveCondition.Evaluate, // IsAlive?
                new SelectorNode(btRunner, new List<BTNode> // Selector
                {
                    new ConditionNode(btRunner, isPlayerInAggroRangeCondition.Evaluate, // IsPlayerInAggroRange?
                        new SequenceNode(btRunner, new List<BTNode> // Sequence
                        {
                            new ActionNode(btRunner, moveToPlayerAction.Execute),   // MoveToPlayer
                            new ActionNode(btRunner, attackPlayerAction.Execute)    // AttackPlayer
                        })
                    ),
                    new ActionNode(btRunner, patrolAction.Execute) // Patrol
                })
            );

            btRunner.SetRootNode(rootNode);
            // Debug.Log("MonsterAI: 행동 트리가 성공적으로 구성되고 BehaviorTreeRunner에 설정되었습니다.");
        }
    }
}
