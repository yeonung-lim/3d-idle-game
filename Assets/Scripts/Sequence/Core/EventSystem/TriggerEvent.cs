using UnityEngine;

/// <summary>
///     트리거 콜리전 시 이벤트를 발생시킵니다.
/// </summary>
[RequireComponent(typeof(Collider))]
public class TriggerEvent : MonoBehaviour
{
    private const string k_PlayerTag = "Player";

    [SerializeField] private AbstractGameEvent m_Event;

    private void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag(k_PlayerTag))
            if (m_Event != null)
                m_Event.Raise();
    }
}