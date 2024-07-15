using System.Collections;
using Core.StateMachine;
using UnityEngine;

/// <summary>
///     설정된 양만큼 상태 머신을 지연시킵니다.
/// </summary>
public class DelayState : AbstractState
{
    private readonly float m_DelayInSeconds;

    /// <param name="delayInSeconds">지연 시간(초)</param>
    public DelayState(float delayInSeconds)
    {
        m_DelayInSeconds = delayInSeconds;
    }

    public override string Name => nameof(DelayState);

    public override IEnumerator Execute()
    {
        var startTime = Time.time;
        while (Time.time - startTime < m_DelayInSeconds) yield return null;
    }
}