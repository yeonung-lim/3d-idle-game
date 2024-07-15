using System;
using System.Collections;
using Core.StateMachine;
using UnityEngine;

/// <summary>
///     이 상태가 활성화되어 있는 동안에는 게임 루프가 일시 중지됩니다.
/// </summary>
public class PauseState : AbstractState
{
    private readonly Action m_OnPause;

    /// <param name="onPause">게임 루프가 일시정지되었을 때 호출되는 액션</param>
    public PauseState(Action onPause)
    {
        m_OnPause = onPause;
    }

    public override string Name => $"{nameof(PauseState)}";

    public override void Enter()
    {
        Time.timeScale = 0f;
        m_OnPause?.Invoke();
    }

    public override IEnumerator Execute()
    {
        yield return null;
    }

    public override void Exit()
    {
        Time.timeScale = 1f;
    }
}