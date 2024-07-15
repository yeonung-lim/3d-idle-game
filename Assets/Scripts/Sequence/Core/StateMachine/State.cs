using System;
using System.Collections;

namespace Core.StateMachine
{
    /// <summary>
    ///     일반적인 비어 있는 상태
    /// </summary>
    public class State : AbstractState
    {
        private readonly Action m_OnExecute;

        /// <param name="onExecute">상태가 실행될 때 호출되는 이벤트입니다.</param>
        public State(Action onExecute)
        {
            m_OnExecute = onExecute;
        }

        public override IEnumerator Execute()
        {
            yield return null;
            m_OnExecute?.Invoke();
        }
    }
}