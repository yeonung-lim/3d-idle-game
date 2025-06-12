# AI 시스템

## 개요

AI 시스템은 게임 내 몬스터와 플레이어의 자동화된 행동을 관리하는 모듈입니다. 행동 트리(Behavior Tree) 패턴을 사용하여 복잡한 AI 행동을 구현합니다.

## 구조

### 행동 트리 (Behavior Tree)

행동 트리는 AI의 의사결정과 행동을 관리하는 핵심 시스템입니다.

#### 기본 노드 클래스

-   **BTNode**: 모든 행동 트리 노드의 기본 추상 클래스
    -   `Execute()`: 노드의 실행 결과를 반환 (Success, Failure, Running)

#### Leaf 노드

-   **ActionNode**: 실제 행동을 수행하는 노드
    -   생성자: `ActionNode(BehaviorTreeRunner runner, Func<NodeState> action)`
    -   정의된 행동을 실행하고 결과를 반환

#### Decorator 노드

-   **ConditionNode**: 조건이 참일 경우에만 자식 노드를 실행
    -   생성자: `ConditionNode(BehaviorTreeRunner runner, Func<bool> condition, BTNode childNode)`
    -   조건이 거짓이면 즉시 Failure 반환

#### Composite 노드

-   **SelectorNode**: 자식 노드 중 하나가 성공하거나 모두 실패할 때까지 순차적으로 실행

    -   생성자: `SelectorNode(BehaviorTreeRunner runner, List<BTNode> children)`
    -   하나라도 성공하면 즉시 Success 반환

-   **SequenceNode**: 모든 자식 노드를 순차적으로 실행
    -   생성자: `SequenceNode(BehaviorTreeRunner runner, List<BTNode> children)`
    -   하나라도 실패하면 즉시 Failure 반환

#### 상태 관리

-   **NodeState**: 노드의 실행 결과 상태를 나타내는 열거형
    -   `Success`: 노드가 성공적으로 완료됨
    -   `Failure`: 노드가 실패함
    -   `Running`: 노드가 아직 실행 중임

### 몬스터 AI

몬스터의 기본적인 행동 패턴을 구현합니다.

#### 행동 구현

-   **AttackPlayerAction**: 플레이어를 공격하는 행동

    -   공격 범위와 쿨타임 관리
    -   플레이어와의 거리에 따른 공격 판정

-   **MoveToPlayerAction**: 플레이어에게 이동하는 행동

    -   NavMeshAgent를 사용한 경로 탐색
    -   목적지 도달 여부 확인

-   **PatrolAction**: 순찰 행동
    -   지정된 반경 내에서 랜덤한 위치로 이동
    -   대기 시간과 새로운 목적지 설정

### 카메라 컨트롤러

카메라 시스템과 AI의 연동을 관리합니다.

#### 주요 컴포넌트

-   **Actor**: 카메라가 타겟팅할 수 있는 게임 엔티티의 기본 클래스

    -   타입 식별을 위한 기본 기능 제공
    -   향후 확장을 위한 추상 클래스

-   **BehaviorTreeRunner**: 행동 트리를 실행하는 컴포넌트
    -   NavMeshAgent를 통한 AI 이동 관리
    -   플레이어/몬스터 AI 설정
    -   자동/수동 모드 전환 지원

## 사용 예시

```csharp
// 행동 트리 생성 예시
var runner = GetComponent<BehaviorTreeRunner>();
var attackNode = new ActionNode(runner, () => {
    // 공격 로직
    return NodeState.Success;
});
var moveNode = new ActionNode(runner, () => {
    // 이동 로직
    return NodeState.Success;
});

// Selector 노드로 공격과 이동을 조합
var rootNode = new SelectorNode(runner, new List<BTNode> {
    attackNode,
    moveNode
});

runner.SetRootNode(rootNode);
```

## 확장성

AI 시스템은 다음과 같은 방식으로 확장할 수 있습니다:

1. 새로운 ActionNode 구현
2. 복잡한 조건을 가진 ConditionNode 추가
3. 커스텀 Composite 노드 생성
4. Actor 클래스의 확장을 통한 새로운 엔티티 타입 추가
