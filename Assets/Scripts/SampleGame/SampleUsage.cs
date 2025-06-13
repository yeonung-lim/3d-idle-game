using UnityEngine;
using Core.UI; // UIManager를 위한 임포트
using SampleGame.UI.ViewModels; // SampleViewModel을 위한 임포트
using SampleGame.UI.Views; // SampleView를 위한 임포트
using SampleGame.UI.Popups; // SamplePopup을 위한 임포트
using System.Threading.Tasks; // Task를 위한 임포트

namespace SampleGame
{
    public class SampleUsage : MonoBehaviour
    {
        async void Start()
        {
            // UIManager가 준비되었는지 확인 (싱글톤이므로 자체 초기화되어야 함)
            if (UIManager.Instance == null)
            {
                Debug.LogError("UIManager 인스턴스를 찾을 수 없습니다!");
                return;
            }

            // UIManager가 필요에 따라 생성되는 경우 초기화를 위한 잠시 대기
            // 실제 게임에서는 초기화 시퀀스나 스플래시 스크린이 있을 수 있음
            await Task.Delay(100); // UIManager.Awake가 실행되도록 짧은 지연

            ShowSampleView();
        }

        public async void ShowSampleView()
        {
            Debug.Log("SampleUsage: SampleView 표시 시도 중...");
            var sampleViewModel = new SampleViewModel();
            // UIManager가 이 뷰 모델을 뷰 인스턴스에 설정할 것임
            View view = await UIManager.Instance.ShowView<SampleView>(sampleViewModel);
            if (view != null)
            {
                Debug.Log("SampleUsage: SampleView가 성공적으로 표시되었습니다.");
            }
            else
            {
                Debug.LogError("SampleUsage: SampleView 표시에 실패했습니다.");
            }
        }

        // 이 메서드는 SampleView의 버튼에 의해 호출될 수 있지만,
        // 이 예제에서는 SampleView의 버튼이 직접 UIManager를 호출합니다.
        // 이것은 팝업을 표시하는 또 다른 방법입니다.
        public async void ShowSamplePopupFromExternal()
        {
            Debug.Log("SampleUsage: 외부에서 SamplePopup 표시 시도 중...");
            var popupViewModel = new SamplePopupViewModel("외부에서 열린 팝업");
            Popup popup = await UIManager.Instance.ShowPopup<SamplePopup>(popupViewModel);
            if (popup != null)
            {
                Debug.Log("SampleUsage: 외부 호출로 SamplePopup이 성공적으로 표시되었습니다.");
            }
            else
            {
                Debug.LogError("SampleUsage: 외부 호출로 SamplePopup 표시에 실패했습니다.");
            }
        }

        void Update()
        {
            // 예시: 'P' 키를 눌러 팝업 표시
            if (Input.GetKeyDown(KeyCode.P))
            {
                ShowSamplePopupFromExternal();
            }

            // 예시: 'C' 키를 눌러 현재 뷰 닫기
             if (Input.GetKeyDown(KeyCode.C))
            {
                UIManager.Instance.CloseCurrentView();
            }
        }
    }
}
