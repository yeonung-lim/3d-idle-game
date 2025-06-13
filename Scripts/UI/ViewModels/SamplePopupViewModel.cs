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
            UnityEngine.Debug.Log($"SamplePopupViewModel Initialized with title: {Title}");
        }

        public override void OnActivated()
        {
            base.OnActivated();
            UnityEngine.Debug.Log("SamplePopupViewModel Activated.");
        }

        public override void OnDeactivated()
        {
            base.OnDeactivated();
            UnityEngine.Debug.Log("SamplePopupViewModel Deactivated.");
        }
    }
}
