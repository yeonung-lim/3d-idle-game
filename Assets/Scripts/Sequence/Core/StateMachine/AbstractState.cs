using System.Collections;
using System.Collections.Generic;

namespace Core.StateMachine
{
    /// <summary>
    ///     상태 머신의 상태에 대한 공통 기능을 제공하는 추상 클래스입니다.
    /// </summary>
    public abstract class AbstractState : IState
    {
        private readonly List<ILink> m_Links = new();

        /// <summary>
        ///     디버깅 목적으로 사용되는 상태의 이름입니다.
        /// </summary>
        public virtual string Name { get; set; }

        public virtual void Enter()
        {
        }

        public abstract IEnumerator Execute();

        public virtual void Exit()
        {
        }

        public virtual void AddLink(ILink link)
        {
            if (!m_Links.Contains(link)) m_Links.Add(link);
        }

        public virtual void RemoveLink(ILink link)
        {
            if (m_Links.Contains(link)) m_Links.Remove(link);
        }

        public virtual void RemoveAllLinks()
        {
            m_Links.Clear();
        }

        public virtual bool ValidateLinks(out IState nextState)
        {
            if (m_Links != null && m_Links.Count > 0)
                foreach (var link in m_Links)
                {
                    var result = link.Validate(out nextState);
                    if (result) return true;
                }

            //default
            nextState = null;
            return false;
        }

        public void EnableLinks()
        {
            foreach (var link in m_Links) link.Enable();
        }

        public void DisableLinks()
        {
            foreach (var link in m_Links) link.Disable();
        }
    }
}