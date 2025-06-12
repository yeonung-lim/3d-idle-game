// C#
// Assets/Scripts/AI/BehaviorTree/NodeState.cs
namespace AI.BehaviorTree
{
    /// <summary>
    /// 노드의 실행 결과 상태를 정의합니다.
    /// </summary>
    public enum NodeState
    {
        Success, // 성공
        Failure, // 실패
        Running  // 실행 중
    }
}
