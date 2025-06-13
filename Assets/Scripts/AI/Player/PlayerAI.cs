// C#
// Assets/Scripts/AI/Player/PlayerAI.cs
using UnityEngine;
using AI.BehaviorTree;
using AI.Player.Conditions; // Player Condition classes
using AI.Player.Actions;   // Player Action classes
using System.Collections.Generic;

namespace AI.Player
{
    /// <summary>
    /// 플레이어의 행동 트리를 설정하고 BehaviorTreeRunner에 연결합니다.
    /// 플레이어 게임 오브젝트에 BehaviorTreeRunner와 함께 추가되어야 합니다.
    /// </summary>
    [RequireComponent(typeof(BehaviorTreeRunner))]
    public class PlayerAI : MonoBehaviour
    {
        private BehaviorTreeRunner btRunner;

        void Start()
        {
            btRunner = GetComponent<BehaviorTreeRunner>();
            if (btRunner == null)
            {
                Debug.LogError("PlayerAI: BehaviorTreeRunner 컴포넌트를 찾을 수 없습니다. 이 스크립트는 비활성화됩니다.");
                enabled = false;
                return;
            }

            ConstructPlayerBehaviorTree();
        }

        /// <summary>
        /// 플레이어의 행동 트리를 구성합니다.
        /// </summary>
        private void ConstructPlayerBehaviorTree()
        {
            // --- 조건 인스턴스 생성 ---
            // 이 조건들은 BehaviorTreeRunner에 정의된 상태와 파라미터를 사용합니다.
            PlayerIsAliveCondition playerIsAlive = new PlayerIsAliveCondition(btRunner);
            IsManualModeCondition isManualMode = new IsManualModeCondition(btRunner);
            PlayerHasTargetCondition playerHasTarget = new PlayerHasTargetCondition(btRunner);
            PlayerTargetIsAliveCondition playerTargetIsAlive = new PlayerTargetIsAliveCondition(btRunner);
            PlayerIsInAttackRangeCondition playerIsInAttackRange = new PlayerIsInAttackRangeCondition(btRunner);
            PlayerCanAttackCondition playerCanAttack = new PlayerCanAttackCondition(btRunner);

            // --- 행동 인스턴스 생성 ---
            PlayerWaitForInputAction waitForInput = new PlayerWaitForInputAction(btRunner);
            PlayerAttackAction attackAction = new PlayerAttackAction(btRunner);
            PlayerMoveToTargetAction moveToTargetAction = new PlayerMoveToTargetAction(btRunner);

            // --- 행동 트리 구성 ---
            // Root (IsAlive? Condition)
            // └── SelectorNode // If alive, then choose between manual or auto
            //     ├── ConditionNode (IsManualMode?)
            //     │   └── ActionNode (PlayerWaitForInput)
            //     └── SequenceNode (AutoMode)
            //         ├── ConditionNode (PlayerHasTarget?)
            //         ├── ConditionNode (PlayerTargetIsAlive?)
            //         ├── SelectorNode // Decide: Attack or Move
            //         │   ├── SequenceNode // Attack sequence
            //         │   │   ├── ConditionNode (PlayerIsInAttackRange?)
            //         │   │   ├── ConditionNode (PlayerCanAttack?)
            //         │   │   └── ActionNode (PlayerAttack)
            //         │   └── ActionNode (PlayerMoveToTarget)

            BTNode rootNode = new ConditionNode(btRunner, playerIsAlive.Evaluate, // IsAlive?
                new SelectorNode(btRunner, new List<BTNode> // Selector (Manual or Auto)
                {
                    // Manual Mode Branch
                    new ConditionNode(btRunner, isManualMode.Evaluate, // IsManualMode?
                        new ActionNode(btRunner, waitForInput.Execute)   // PlayerWaitForInput
                    ),
                    // Auto Mode Branch
                    new SequenceNode(btRunner, new List<BTNode> // AutoMode Sequence
                    {
                        new ConditionNode(btRunner, playerHasTarget.Evaluate, new ActionNode(btRunner, () => NodeState.Success)),       // PlayerHasTarget?
                        new ConditionNode(btRunner, playerTargetIsAlive.Evaluate, new ActionNode(btRunner, () => NodeState.Success)),   // PlayerTargetIsAlive?
                        new SelectorNode(btRunner, new List<BTNode> // Selector (Attack or Move)
                        {
                            // Attack Sequence
                            new SequenceNode(btRunner, new List<BTNode>
                            {
                                new ConditionNode(btRunner, playerIsInAttackRange.Evaluate, new ActionNode(btRunner, () => NodeState.Success)), // PlayerIsInAttackRange?
                                new ConditionNode(btRunner, playerCanAttack.Evaluate, new ActionNode(btRunner, () => NodeState.Success)),       // PlayerCanAttack?
                                new ActionNode(btRunner, attackAction.Execute)               // PlayerAttack
                            }),
                            // Move Action (if attack conditions not met or attack sequence fails)
                            new ActionNode(btRunner, moveToTargetAction.Execute)         // PlayerMoveToTarget
                        })
                    })
                })
            );

            btRunner.SetRootNode(rootNode);
            Debug.Log("PlayerAI: 플레이어 행동 트리가 성공적으로 구성되어 BehaviorTreeRunner에 설정되었습니다.");
        }
    }
}
