/// <summary>
///     전환을 위해 항상 열려 있는 링크입니다.
///     이 링크 유형으로 현재 상태가 다음 단계에 연결되어 있으면
///     현재 단계의 실행이 완료되면 상태 머신은 다음 단계로 이동합니다.
/// </summary>
public class Link : ILink
{
    private readonly IState m_NextState;

    /// <param name="nextState">다음 상태</param>
    public Link(IState nextState)
    {
        m_NextState = nextState;
    }

    public bool Validate(out IState nextState)
    {
        nextState = m_NextState;
        return true;
    }
}