using MVVM.Core; // Required for ViewBase
using MVVM.ViewModels; // Required for ExampleViewModel
using UnityEngine;
using System.ComponentModel;

namespace MVVM.Views
{
    public class ExampleView : ViewBase<ExampleViewModel>
    {
        // 이 뷰는 주로 ViewBase 기능을 보여주는 예제입니다.
        // 대부분의 UI 업데이트는 간단한 속성-속성 바인딩을 위해
        // DataBinder 컴포넌트에 의해 직접 처리됩니다.

        protected override void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (viewModel == null) return;

            // ViewModel에서 속성이 변경되었음을 로그로 기록합니다.
            // 더 복잡한 뷰에서는 DataBinder로 쉽게 처리할 수 없는
            // 특정 UI 요소를 업데이트하거나 애니메이션을 트리거할 수 있습니다.
            Debug.Log($"ExampleView가 ViewModel 속성 변경을 수신했습니다: {e.PropertyName}. 현재 상태: {viewModel.StatusMessage}", this);

            // DataBinder를 사용하지 않을 경우 수동으로 업데이트하는 예시:
            // if (e.PropertyName == nameof(viewModel.PlayerName))
            // {
            //     // someTextComponent가 이 View의 Text 필드라고 가정
            //     // someTextComponent.text = viewModel.PlayerName;
            // }
        }

        protected override void InitialUpdate()
        {
            base.InitialUpdate();
            if (viewModel != null)
            {
                Debug.Log($"ExampleView 초기 업데이트가 호출되었습니다. ViewModel 플레이어 이름: {viewModel.PlayerName}", this);
                // ViewModel의 상태를 기반으로 초기 설정을 수행합니다
                // ViewModel이 처음 할당될 때 뷰가 한 번만 설정해야 하는 경우에 유용합니다.
            }
        }

        // 뷰가 ViewModel과 상호작용하는 방법을 보여주는 예시 메서드
        public void TriggerViewModelDamage()
        {
            if (viewModel != null)
            {
                viewModel.TakeDamage(10);
            }
        }
    }
}
