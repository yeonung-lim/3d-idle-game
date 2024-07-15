using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     기본 이벤트 기능을 제공하는 베이스 클래스
///     이벤트는 게임의 여러 범위 간의 통신을 용이하게 합니다.
///     각 이벤트는 이벤트가 트리거될 때 알림을 받는 리스너 목록이 있는 스크립트 가능한 오브젝트 인스턴스입니다.
/// </summary>
public abstract class AbstractGameEvent : ScriptableObject
{
    private readonly List<IGameEventListener> m_EventListeners = new();

    /// <summary>
    ///     각 이벤트는 트리거된 후 즉시 재설정됩니다.
    ///     이 메서드에는 파생 클래스에 대한 재설정 로직이 포함되어 있습니다.
    /// </summary>
    public abstract void Reset();

    /// <summary>
    ///     현재 이벤트 인스턴스를 트리거하고 구독자에게 알림을 보냅니다.
    /// </summary>
    public void Raise()
    {
        for (var i = m_EventListeners.Count - 1; i >= 0; i--)
            m_EventListeners[i].OnEventRaised();
        Reset();
    }

    /// <summary>
    ///     이 이벤트의 옵저버 목록에 클래스를 추가합니다.
    /// </summary>
    /// <param name="listener">이 이벤트를 관찰하려는 클래스</param>
    public void AddListener(IGameEventListener listener)
    {
        if (!m_EventListeners.Contains(listener)) m_EventListeners.Add(listener);
    }

    /// <summary>
    ///     이 이벤트의 옵저버 목록에서 클래스를 제거합니다.
    /// </summary>
    /// <param name="listener">이 이벤트를 더 이상 관찰하고 싶지 않은 클래스</param>
    public void RemoveListener(IGameEventListener listener)
    {
        if (m_EventListeners.Contains(listener)) m_EventListeners.Remove(listener);
    }
}