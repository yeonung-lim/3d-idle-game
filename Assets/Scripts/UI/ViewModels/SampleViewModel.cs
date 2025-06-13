using Core.UI; // ViewModelBase를 위한 임포트
using System.Windows.Input; // ICommand를 위한 임포트 (선택사항, 간단한 메서드도 사용 가능)

// 간단한 ICommand 구현 예시
public class RelayCommand : ICommand
{
    private readonly System.Action _execute;
    private readonly System.Func<bool> _canExecute;

    public event System.EventHandler CanExecuteChanged;

    public RelayCommand(System.Action execute, System.Func<bool> canExecute = null)
    {
        _execute = execute ?? throw new System.ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public bool CanExecute(object parameter) => _canExecute == null || _canExecute();
    public void Execute(object parameter) => _execute();

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, System.EventArgs.Empty);
}

namespace SampleGame.UI.ViewModels
{
    public class SampleViewModel : ViewModelBase
    {
        private string _displayText = "Initial Text from ViewModel";
        public string DisplayText
        {
            get => _displayText;
            set => SetProperty(ref _displayText, value);
        }

        private int _updateCount = 0;

        public ICommand UpdateTextCommand { get; }

        public SampleViewModel()
        {
            UpdateTextCommand = new RelayCommand(UpdateText);
            Initialize();
        }

        public override void Initialize(object model = null)
        {
            base.Initialize(model);
            DisplayText = "ViewModel Initialized!";
            UnityEngine.Debug.Log("샘플 뷰모델이 초기화되었습니다.");
        }

        private void UpdateText()
        {
            _updateCount++;
            DisplayText = $"Text updated: {_updateCount} times!";
            UnityEngine.Debug.Log("샘플 뷰모델: 텍스트 업데이트 명령이 실행되었습니다.");
        }

        public override void OnActivated()
        {
            base.OnActivated();
            UnityEngine.Debug.Log("샘플 뷰모델이 활성화되었습니다.");
        }

        public override void OnDeactivated()
        {
            base.OnDeactivated();
            UnityEngine.Debug.Log("샘플 뷰모델이 비활성화되었습니다.");
        }
    }
}
