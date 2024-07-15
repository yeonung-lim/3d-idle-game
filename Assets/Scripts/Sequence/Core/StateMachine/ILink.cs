/// <summary>
///     상태 머신의 상태를 서로 연결하는 트랜지션 링크용 인터페이스
/// </summary>
public interface ILink
{
    /// <summary>
    ///     state-machine은 이 메서드를 호출하여 링크를 검사하고 전환을 위해 열려 있는지 확인합니다.
    ///     현재 상태의 첫 번째 열린 링크에 의해 결정된 다음 상태로 전환합니다.
    ///     상태의 모든 링크가 거짓을 반환하면 상태-머신은 현재 상태를 유지합니다.
    /// </summary>
    /// <param name="nextState">이 링크가 가리키는 다음 상태</param>
    /// <returns>true: 전환을 위해 링크가 열려 있음.</returns>
    bool Validate(out IState nextState);

    /// <summary>
    ///     링크를 활성화합니다.
    /// </summary>
    void Enable()
    {
    }

    /// <summary>
    ///     링크를 비활성화합니다.
    /// </summary>
    void Disable()
    {
    }
}