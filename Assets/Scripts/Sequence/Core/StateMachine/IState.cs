using System.Collections;

/// <summary>
///     상태 머신의 상태를 위한 인터페이스
/// </summary>
public interface IState
{
    /// <summary>
    ///     이 상태가 "현재 상태"가 될 때 소유자 state-machine이 호출합니다
    ///     이 메서드는 다른 상태 메서드보다 먼저 호출되며 상태 요구 사항을 설정하는 데 사용됩니다.
    /// </summary>
    void Enter();

    /// <summary>
    ///     이 상태가 "현재 상태"가 되면 소유자 state-machine이 호출합니다
    ///     이 코루틴은 Enter() 이후에 시작되며 상태의 주요 로직을 포함합니다.
    /// </summary>
    /// <returns></returns>
    IEnumerator Execute();

    /// <summary>
    ///     이 상태가 더 이상 "현재 상태"가 아니며 다음
    ///     상태로 이동할 때 소유자 state-machine이 호출합니다.
    ///     이 메서드는 정리에 사용됩니다.
    /// </summary>
    void Exit();

    /// <summary>
    ///     상태에 전환 링크를 추가합니다. 각 상태에는 여러 개의 링크가 있을 수 있습니다.
    /// </summary>
    /// <param name="link">추가할 링크</param>
    void AddLink(ILink link);

    /// <summary>
    ///     상태에서 전환 링크를 제거합니다.
    /// </summary>
    /// <param name="link">제거할 링크</param>
    void RemoveLink(ILink link);

    /// <summary>
    ///     상태의 모든 링크를 제거합니다.
    /// </summary>
    void RemoveAllLinks();

    /// <summary>
    ///     state-machine은 이 메서드를 호출하여 상태의 모든 링크를 검사하고 다음 상태로 전환할지 여부를 결정합니다.
    ///     state-machine은 현재 상태의 첫 번째 열린 링크에 의해 결정된 다음 상태로 전환합니다.
    ///     상태의 모든 링크가 거짓을 반환하면 상태 머신은 현재 상태를 유지합니다.
    /// </summary>
    /// <param name="nextState">첫 번째 열린 링크가 가리키는 다음 상태는 다음과 같습니다.</param>
    /// <returns>true: 상태 머신이 다음 단계로 넘어가야 합니다.</returns>
    bool ValidateLinks(out IState nextState);

    /// <summary>
    ///     상태의 모든 링크를 활성화합니다.
    /// </summary>
    void EnableLinks();

    /// <summary>
    ///     상태의 모든 링크를 비활성화합니다.
    /// </summary>
    void DisableLinks();
}