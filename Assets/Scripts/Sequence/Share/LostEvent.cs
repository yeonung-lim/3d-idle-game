using UnityEngine;

namespace Share
{
    /// <summary>
    ///     플레이어가 레벨을 실패하면 이벤트가 트리거됩니다.
    /// </summary>
    [CreateAssetMenu(fileName = nameof(LostEvent),
        menuName = "Events/Common/" + nameof(LostEvent))]
    public class LostEvent : AbstractGameEvent
    {
        public override void Reset()
        {
        }
    }
}