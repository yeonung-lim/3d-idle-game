using Core.UI;
using SampleGame.UI.ViewModels; // For SamplePopupViewModel
using UnityEngine;
using UnityEngine.UI; // For Text and Button

namespace SampleGame.UI.Popups
{
    [UIAttribute("UI/SamplePopup")] // Addressable Key
    public class SamplePopup : Popup
    {
        [Header("UI Elements")]
        [SerializeField] private Text _titleText; // Assign in Inspector
        [SerializeField] private Button _closeButton; // Assign in Inspector

        private SamplePopupViewModel _viewModel;

        protected override void Awake()
        {
            base.Awake();
            if (_titleText == null) _titleText = GetComponentInChildren<Text>(); // Simple search
            if (_closeButton == null) Debug.LogWarning("SamplePopup: CloseButton not assigned.");

            if (_closeButton != null)
            {
                _closeButton.onClick.AddListener(OnCloseButtonClick);
            }
        }

        public override void SetViewModel(ViewModelBase viewModel)
        {
            base.SetViewModel(viewModel);
            if (viewModel is SamplePopupViewModel popupVM)
            {
                _viewModel = popupVM;
                _viewModel.PropertyChanged += ViewModel_PropertyChanged;
                UpdateTitleText(_viewModel.Title);
            }
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SamplePopupViewModel.Title))
            {
                UpdateTitleText(_viewModel.Title);
            }
        }

        private void UpdateTitleText(string title)
        {
            if (_titleText != null)
            {
                _titleText.text = title;
            }
        }

        private void OnCloseButtonClick()
        {
            // Close this specific popup instance
            UIManager.Instance.ClosePopup(this);
            // Alternatively, UIManager.Instance.CloseTopPopup(); if this is always the top one to be closed by its own button
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (_viewModel != null)
            {
                _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
            }
            if (_closeButton != null) _closeButton.onClick.RemoveListener(OnCloseButtonClick);
        }
    }
}
