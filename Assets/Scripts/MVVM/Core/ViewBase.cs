using UnityEngine;
using System.ComponentModel;

namespace MVVM.Core
{
    public abstract class ViewBase<TViewModel> : MonoBehaviour where TViewModel : ViewModelBase
    {
        [SerializeField]
        protected TViewModel viewModel;

        protected virtual void OnEnable()
        {
            if (viewModel != null)
            {
                viewModel.PropertyChanged += OnViewModelPropertyChanged;
            }
            else
            {
                Debug.LogError(GetType().Name + "에서 ViewModel이 할당되지 않았습니다.", this);
            }
        }

        protected virtual void OnDisable()
        {
            if (viewModel != null)
            {
                viewModel.PropertyChanged -= OnViewModelPropertyChanged;
            }
        }

        protected abstract void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e);

        /// <summary>
        /// 선택사항: OnEnable 이후에 런타임 중 ViewModel을 할당하거나 변경할 경우 이 메서드를 호출하세요.
        /// </summary>
        public virtual void SetViewModel(TViewModel newViewModel)
        {
            if (viewModel != null)
            {
                viewModel.PropertyChanged -= OnViewModelPropertyChanged;
            }

            viewModel = newViewModel;

            if (viewModel != null)
            {
                viewModel.PropertyChanged += OnViewModelPropertyChanged;
                // 선택사항: 모든 바인딩을 새로고침하거나 초기 UI 설정을 트리거
                InitialUpdate();
            }
        }

        /// <summary>
        /// 새로운 ViewModel이 설정되거나 UI를 수동으로 새로고침할 때 호출됩니다.
        /// ViewModel 속성을 기반으로 초기 상태를 설정하려면 이 메서드를 오버라이드하세요.
        /// </summary>
        protected virtual void InitialUpdate()
        {
            // 예시: 관련된 모든 속성에 대한 업데이트를 수동으로 트리거
            // if (viewModel != null)
            // {
            //     OnViewModelPropertyChanged(viewModel, new PropertyChangedEventArgs(null)); // 또는 특정 속성들
            // }
        }
    }
}
