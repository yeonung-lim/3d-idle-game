/// <summary>
///     특정 이벤트를 수신 대기하고 이벤트가 발생하면 전환을 위해 열려 있는 링크
///     이 링크 유형에 의해 현재 상태가 다음 단계로 연결되면
///     상태 머신은 이벤트가 트리거될 때까지 기다린 후 다음 단계로 이동합니다.
/// </summary>
public class EventLink : ILink, IGameEventListener
{
    private readonly AbstractGameEvent m_GameEvent;
    private readonly IState m_NextState;
    private bool m_EventRaised;

    /// <param name="gameEvent">이 링크가 수신하는 이벤트</param>
    /// <param name="nextState">다음 상태</param>
    public EventLink(AbstractGameEvent gameEvent, IState nextState)
    {
        m_GameEvent = gameEvent;
        m_NextState = nextState;
    }

    public void OnEventRaised()
    {
        m_EventRaised = true;
    }

    /// <summary>
    ///     이벤트가 발생하면 다음 상태로 전환합니다.
    /// </summary>
    /// <param name="nextState">다음 상태</param>
    /// <returns>이벤트가 발생하면 true를 반환합니다.</returns>
    public bool Validate(out IState nextState)
    {
        nextState = null;
        var result = false;

        if (m_EventRaised)
        {
            nextState = m_NextState;
            result = true;
        }

        return result;
    }

    public void Enable()
    {
        m_GameEvent.AddListener(this);
        m_EventRaised = false;
    }

    public void Disable()
    {
        m_GameEvent.RemoveListener(this);
        m_EventRaised = false;
    }
}