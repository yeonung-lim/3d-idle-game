using Core.UI;
using SampleGame.UI.ViewModels; // SampleViewModel을 위한 임포트
using UnityEngine;
using UnityEngine.UI; // Text와 Button을 위한 임포트

namespace SampleGame.UI.Views
{
    [UIAttribute("UI/SampleView")] // 어드레서블 키
    public class SampleView : View
    {
        [Header("UI Elements")]
        [SerializeField] private Text _infoText; // 인스펙터에서 할당
        [SerializeField] private Button _updateButton; // 인스펙터에서 할당
        [SerializeField] private Button _openPopupButton; // 인스펙터에서 할당

        private SampleViewModel _viewModel;

        protected override void Awake()
        {
            base.Awake(); // 중요: UIBase.Awake()는 RectTransform을 초기화합니다

            // 인스펙터에서 할당되지 않은 경우를 위한 대체 코드
            if (_infoText == null) _infoText = GetComponentInChildren<Text>(); // 간단한 검색
            if (_updateButton == null) Debug.LogWarning("SampleView: 업데이트 버튼이 할당되지 않았습니다.");
            if (_openPopupButton == null) Debug.LogWarning("SampleView: 팝업 열기 버튼이 할당되지 않았습니다.");

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
                // 데이터 바인딩을 위한 PropertyChanged 이벤트 구독
                _viewModel.PropertyChanged += ViewModel_PropertyChanged;
                // 초기 UI 업데이트
                UpdateInfoText(_viewModel.DisplayText);
            }
            else if (viewModel != null)
            {
                Debug.LogError("SampleView가 잘못된 ViewModel 타입을 받았습니다.");
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
                Debug.Log($"SampleView: 정보 텍스트가 '{text}'로 업데이트되었습니다");
            }
            else
            {
                Debug.LogWarning("SampleView: InfoText 컴포넌트가 null입니다.");
            }
        }

        private void OnUpdateButtonClick()
        {
            // ViewModel의 명령 실행
            _viewModel?.UpdateTextCommand?.Execute(null);
        }

        private async void OnOpenPopupButtonClick()
        {
            Debug.Log("SampleView: 팝업 열기 버튼이 클릭되었습니다. SamplePopup을 표시하려고 시도합니다.");
            // 뷰에서 팝업을 여는 예시
            var popupVM = new SamplePopupViewModel("SampleView에서 열린 팝업");
            await UIManager.Instance.ShowPopup<SampleGame.UI.Popups.SamplePopup>(popupVM); // SamplePopup의 전체 네임스페이스 추가
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (_viewModel != null)
            {
                // 구독 해제
                _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
            }
            if (_updateButton != null) _updateButton.onClick.RemoveListener(OnUpdateButtonClick);
            if (_openPopupButton != null) _openPopupButton.onClick.RemoveListener(OnOpenPopupButtonClick);
        }
    }
}
