using System;

namespace Share
{
    /// <summary>
    ///     일반 이벤트 옵저버 클래스
    /// </summary>
    [Serializable]
    public class GenericGameEventListener : IGameEventListener
    {
        /// <summary>
        ///     이 클래스가 관찰하는 이벤트
        /// </summary>
        public AbstractGameEvent m_Event;

        /// <summary>
        ///     이벤트가 트리거되면 호출되는 이벤트 핸들러
        /// </summary>
        public Action EventHandler;

        /// <summary>
        ///     구독된 이벤트가 트리거될 때 호출되는 이벤트 핸들러입니다.
        /// </summary>
        public void OnEventRaised()
        {
            EventHandler?.Invoke();
        }

        /// <summary>
        ///     이벤트 구독
        /// </summary>
        public void Subscribe()
        {
            m_Event.AddListener(this);
        }

        /// <summary>
        ///     이벤트 구독 해제
        /// </summary>
        public void Unsubscribe()
        {
            m_Event.RemoveListener(this);
        }
    }
}