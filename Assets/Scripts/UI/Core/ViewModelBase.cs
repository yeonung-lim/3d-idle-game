using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Core.UI
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
            {
                return false;
            }

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        // 선택사항: 모델에서 데이터를 받아 ViewModel을 초기화하는 메서드 추가
        public virtual void Initialize(object model = null)
        {
            // 기본 구현은 비어있거나 공통 초기화를 처리할 수 있음
        }

        // 선택사항: ViewModel이 활성화될 때 호출되는 메서드 (예: View가 표시될 때)
        public virtual void OnActivated()
        {
            // 연결된 View가 활성화될 때 호출됨
        }

        // 선택사항: ViewModel이 비활성화될 때 호출되는 메서드 (예: View가 닫히거나 숨겨질 때)
        public virtual void OnDeactivated()
        {
            // 연결된 View가 닫히거나 숨겨질 때 호출됨
        }
    }
}
