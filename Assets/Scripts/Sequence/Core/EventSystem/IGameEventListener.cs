/// <summary>
///     이벤트를 구독하려는 모든 클래스는 이 인터페이스를 구현해야 합니다.
/// </summary>
public interface IGameEventListener
{
    /// <summary>
    ///     구독된 이벤트가 트리거될 때 호출되는 이벤트 핸들러입니다.
    /// </summary>
    void OnEventRaised();
}