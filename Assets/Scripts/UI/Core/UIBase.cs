using UnityEngine;

namespace Core.UI
{
    public abstract class UIBase : MonoBehaviour
    {
        public string UIName { get; set; }
        public RectTransform RectTransform { get; private set; }
        public ViewModelBase ViewModel { get; private set; } // ViewModel 저장

        protected virtual void Awake()
        {
            RectTransform = GetComponent<RectTransform>();
        }

        public virtual void Show()
        {
            gameObject.SetActive(true);
            ViewModel?.OnActivated(); // 표시될 때 OnActivated 호출
        }

        public virtual void Close()
        {
            ViewModel?.OnDeactivated(); // 닫힐 때 OnDeactivated 호출
            gameObject.SetActive(false);
        }

        public virtual void SetViewModel(ViewModelBase viewModel)
        {
            ViewModel = viewModel;
            // 파생 클래스들은 필요한 경우 특정 타입의 ViewModel에 대한 바인딩을 수행하거나
            // 초기 데이터 동기화를 트리거하기 위해 이 메서드를 재정의할 수 있습니다.
        }

        // 이 메서드는 UI GameObject가 파괴될 때 UIManager에 의해 호출될 수 있으며,
        // 이 특정 인스턴스에 연결된 Addressable 에셋들이 해제되도록 보장합니다.
        protected virtual void OnDestroy()
        {
            // UIManager가 null이 아니고 이 인스턴스가 UIManager에 의해 관리되고 있었다면
            // if (UIManager.Instance != null) // 문제가 있는 라인 제거됨
            // {
            //     // UIManager.Instance.OnUIDestroyed(this);
            // }
        }
    }
}
