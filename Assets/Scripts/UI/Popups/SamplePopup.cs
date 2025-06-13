using Core.UI;
using SampleGame.UI.ViewModels; // SamplePopupViewModel을 위한 임포트
using UnityEngine;
using UnityEngine.UI; // Text와 Button을 위한 임포트

namespace SampleGame.UI.Popups
{
    [UIAttribute("UI/SamplePopup")] // 어드레서블 키
    public class SamplePopup : Popup
    {
        [Header("UI Elements")]
        [SerializeField] private Text _titleText; // Inspector에서 할당
        [SerializeField] private Button _closeButton; // Inspector에서 할당

        private SamplePopupViewModel _viewModel;

        protected override void Awake()
        {
            base.Awake();
            if (_titleText == null) _titleText = GetComponentInChildren<Text>(); // 간단한 검색
            if (_closeButton == null) Debug.LogWarning("SamplePopup: 닫기 버튼이 할당되지 않았습니다.");

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
            // 이 특정 팝업 인스턴스를 닫음
            UIManager.Instance.ClosePopup(this);
            // 또는, 이 버튼이 항상 최상위 팝업을 닫는 경우 UIManager.Instance.CloseTopPopup(); 사용
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
