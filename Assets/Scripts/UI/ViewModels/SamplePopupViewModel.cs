using Core.UI;

namespace SampleGame.UI.ViewModels
{
    public class SamplePopupViewModel : ViewModelBase
    {
        private string _title;
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public SamplePopupViewModel(string title = "Default Popup Title")
        {
            Title = title;
            UnityEngine.Debug.Log($"샘플 팝업 뷰모델이 초기화되었습니다. 제목: {Title}");
        }

        public override void OnActivated()
        {
            base.OnActivated();
            UnityEngine.Debug.Log("샘플 팝업 뷰모델이 활성화되었습니다.");
        }

        public override void OnDeactivated()
        {
            base.OnDeactivated();
            UnityEngine.Debug.Log("샘플 팝업 뷰모델이 비활성화되었습니다.");
        }
    }
}
