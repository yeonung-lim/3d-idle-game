using UnityEngine;

namespace Share
{
    /// <summary>
    ///     이벤트를 발생시키는 클래스입니다.
    /// </summary>
    public class EventRaise : MonoBehaviour
    {
        [SerializeField] private AbstractGameEvent raiseEvent;

        public void Raise()
        {
            if (raiseEvent == null) return;

            raiseEvent.Raise();
        }
    }
}