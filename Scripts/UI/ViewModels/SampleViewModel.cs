using Core.UI; // For ViewModelBase
using System.Windows.Input; // For ICommand (optional, can use simple methods too)

// Simple ICommand implementation for demonstration
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
            UnityEngine.Debug.Log("SampleViewModel Initialized.");
        }

        private void UpdateText()
        {
            _updateCount++;
            DisplayText = $"Text updated: {_updateCount} times!";
            UnityEngine.Debug.Log("SampleViewModel: UpdateTextCommand executed.");
        }

        public override void OnActivated()
        {
            base.OnActivated();
            UnityEngine.Debug.Log("SampleViewModel Activated.");
        }

        public override void OnDeactivated()
        {
            base.OnDeactivated();
            UnityEngine.Debug.Log("SampleViewModel Deactivated.");
        }
    }
}
