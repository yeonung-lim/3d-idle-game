using Core.UI;
using SampleGame.UI.ViewModels; // For SampleViewModel
using UnityEngine;
using UnityEngine.UI; // For Text and Button

namespace SampleGame.UI.Views
{
    [UIAttribute("UI/SampleView")] // Addressable Key
    public class SampleView : View
    {
        [Header("UI Elements")]
        [SerializeField] private Text _infoText; // Assign in Inspector
        [SerializeField] private Button _updateButton; // Assign in Inspector
        [SerializeField] private Button _openPopupButton; // Assign in Inspector

        private SampleViewModel _viewModel;

        protected override void Awake()
        {
            base.Awake(); // Important: UIBase.Awake() initializes RectTransform

            // Fallback if not assigned in inspector
            if (_infoText == null) _infoText = GetComponentInChildren<Text>(); // Simple search
            if (_updateButton == null) Debug.LogWarning("SampleView: UpdateButton not assigned.");
            if (_openPopupButton == null) Debug.LogWarning("SampleView: OpenPopupButton not assigned.");

            if (_updateButton != null)
            {
                _updateButton.onClick.AddListener(OnUpdateButtonClick);
            }
            if (_openPopupButton != null)
            {
                _openPopupButton.onClick.AddListener(OnOpenPopupButtonClick);
            }
        }

        public override void SetViewModel(ViewModelBase viewModel)
        {
            base.SetViewModel(viewModel);
            if (viewModel is SampleViewModel sampleVM)
            {
                _viewModel = sampleVM;
                // Subscribe to PropertyChanged event for data binding
                _viewModel.PropertyChanged += ViewModel_PropertyChanged;
                // Initial UI update
                UpdateInfoText(_viewModel.DisplayText);
            }
            else if (viewModel != null)
            {
                Debug.LogError("SampleView received incorrect ViewModel type.");
            }
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SampleViewModel.DisplayText))
            {
                UpdateInfoText(_viewModel.DisplayText);
            }
        }

        private void UpdateInfoText(string text)
        {
            if (_infoText != null)
            {
                _infoText.text = text;
                Debug.Log($"SampleView: InfoText updated to '{text}'");
            }
            else
            {
                Debug.LogWarning("SampleView: InfoText component is null.");
            }
        }

        private void OnUpdateButtonClick()
        {
            // Execute command on ViewModel
            _viewModel?.UpdateTextCommand?.Execute(null);
        }

        private async void OnOpenPopupButtonClick()
        {
            Debug.Log("SampleView: OpenPopupButton clicked. Attempting to show SamplePopup.");
            // Example of opening a popup from a view
            var popupVM = new SamplePopupViewModel("Popup from SampleView");
            await UIManager.Instance.ShowPopup<SampleGame.UI.Popups.SamplePopup>(popupVM); // Added full namespace for SamplePopup
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (_viewModel != null)
            {
                // Unsubscribe
                _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
            }
            if (_updateButton != null) _updateButton.onClick.RemoveListener(OnUpdateButtonClick);
            if (_openPopupButton != null) _openPopupButton.onClick.RemoveListener(OnOpenPopupButtonClick);
        }
    }
}
