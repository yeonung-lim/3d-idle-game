using UnityEngine;

namespace Share
{
    /// <summary>
    ///     플레이어가 레벨을 완료하면 이벤트가 트리거됩니다.
    /// </summary>
    [CreateAssetMenu(fileName = nameof(CompletedEvent),
        menuName = "Events/Common/" + nameof(CompletedEvent))]
    public class CompletedEvent : AbstractGameEvent
    {
        public override void Reset()
        {
        }
    }
}