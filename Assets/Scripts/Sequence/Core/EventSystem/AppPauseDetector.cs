using UnityEngine;

/// <summary>
///     외부(OS) 소스에 의해 게임이 일시 중지되거나 포커스를 잃었는지
///     확인하기 위해 유니티 이벤트 메서드를 재정의하고 이벤트를 트리거합니다.
/// </summary>
public class AppPauseDetector : MonoBehaviour
{
    [SerializeField] private AbstractGameEvent m_PauseEvent;

    /// <summary>
    ///     애플리케이션의 현재 일시 중지 상태를 반환합니다.
    /// </summary>
    public bool IsPaused { get; private set; }

    private void OnApplicationFocus(bool hasFocus)
    {
        IsPaused = !hasFocus;

        if (IsPaused)
            m_PauseEvent.Raise();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        IsPaused = pauseStatus;

        if (IsPaused)
            m_PauseEvent.Raise();
    }
}