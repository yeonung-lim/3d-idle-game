using System;
using System.Collections;
using UniRx;
using UnityEngine;

/// <summary>
///     일반 상태 머신
/// </summary>
public class StateMachine
{
    private readonly ReactiveProperty<IState> _currentStateRP = new();
    private Coroutine m_CurrentPlayCoroutine;

    private Coroutine m_LoopCoroutine;
    private bool m_PlayLock;
    public IState CurrentState { get; private set; }

    public bool IsRunning => m_LoopCoroutine != null;

    /// <summary>
    ///     이전 상태를 마무리한 다음 새 상태를 실행합니다.
    /// </summary>
    /// <param name="state"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public virtual void SetCurrentState(IState state)
    {
        if (state == null)
            throw new ArgumentNullException(nameof(state));

        if (CurrentState != null && m_CurrentPlayCoroutine != null)
            //interrupt currently executing state
            Skip();

        _currentStateRP.Value = CurrentState = state;
        Coroutines.StartCoroutine(Play());
    }

    /// <summary>
    ///     현재 상태의 수명 주기 메서드를 실행합니다.
    /// </summary>
    /// <returns></returns>
    private IEnumerator Play()
    {
        if (!m_PlayLock)
        {
            m_PlayLock = true;

            CurrentState.Enter();

            //현재 상태의 코루틴을 실행하기 위한
            //참조를 유지하여 나중에 중지할 수 있도록 지원합니다.
            m_CurrentPlayCoroutine = Coroutines.StartCoroutine(CurrentState.Execute());
            yield return m_CurrentPlayCoroutine;

            m_CurrentPlayCoroutine = null;
        }
    }

    public IDisposable SubscribeStateChanged(Action<IState> onNext)
    {
        return _currentStateRP.Subscribe(onNext);
    }

    /// <summary>
    ///     현재 상태의 실행을 중단하고 완료합니다.
    /// </summary>
    /// <exception cref="Exception"></exception>
    private void Skip()
    {
        if (CurrentState == null)
            throw new Exception($"{nameof(CurrentState)} is null!");

        if (m_CurrentPlayCoroutine != null)
        {
            Coroutines.StopCoroutine(ref m_CurrentPlayCoroutine);
            //finalize current state
            CurrentState.Exit();
            m_CurrentPlayCoroutine = null;
            m_PlayLock = false;
        }
    }

    public virtual void Run(IState state)
    {
        SetCurrentState(state);
        Run();
    }

    /// <summary>
    ///     이 메서드는 Stop()
    ///     이후에 호출되면 이전 상태를 재개하지 않으며 클라이언트가 수동으로 상태를 설정해야 합니다.
    /// </summary>
    public virtual void Run()
    {
        if (m_LoopCoroutine != null) //already running
            return;

        m_LoopCoroutine = Coroutines.StartCoroutine(Loop());
    }

    /// <summary>
    ///     스테이트 머신의 메인 루프를 끕니다.
    /// </summary>
    public void Stop()
    {
        if (m_LoopCoroutine == null) // 이미 중지됨
            return;

        if (CurrentState != null && m_CurrentPlayCoroutine != null)
            // 현재 실행 중인 상태 인터럽트
            Skip();

        Coroutines.StopCoroutine(ref m_LoopCoroutine);
        CurrentState = null;
    }

    /// <summary>
    ///     스테이트 머신의 메인 업데이트 루프.
    ///     현재 스테이트의 상태와 링크를 확인하여 스테이트 시퀀싱을 제공합니다.
    /// </summary>
    /// <returns></returns>
    protected virtual IEnumerator Loop()
    {
        while (true)
        {
            if (CurrentState != null && m_CurrentPlayCoroutine == null) // 현재 상태 재생 완료
                if (CurrentState.ValidateLinks(out var nextState))
                {
                    if (m_PlayLock)
                    {
                        // 현재 상태 마무리
                        CurrentState.Exit();
                        m_PlayLock = false;
                    }

                    CurrentState.DisableLinks();
                    SetCurrentState(nextState);
                    CurrentState.EnableLinks();
                }

            yield return null;
        }
    }
}