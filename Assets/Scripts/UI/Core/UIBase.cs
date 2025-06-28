using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Core.UI
{
    public abstract class UIBase : MonoBehaviour
    {
        public string UIName { get; set; }
        public VisualElement RootVisualElement { get; private set; }
        public UIDocument UIDocument { get; private set; }

        protected virtual void Awake()
        {
            UIDocument = GetComponent<UIDocument>();
            if (UIDocument != null)
            {
                RootVisualElement = UIDocument.rootVisualElement;
                InitializeVisualElements();
                SetupDataBinding();
            }
            else
            {
                Debug.LogError($"UIDocument component not found on {gameObject.name}");
            }
        }

        /// <summary>
        /// UI Toolkit의 VisualElement들을 초기화하는 메서드
        /// 파생 클래스에서 오버라이드하여 UI 요소들을 찾고 초기 설정
        /// </summary>
        protected virtual void InitializeVisualElements()
        {
            // 파생 클래스에서 구현
            // 예: button = RootVisualElement.Q<Button>("MyButton");
        }

        /// <summary>
        /// UI Toolkit의 데이터 바인딩을 설정하는 메서드
        /// 파생 클래스에서 오버라이드하여 바인딩 로직 구현
        /// </summary>
        protected virtual void SetupDataBinding()
        {
            // 파생 클래스에서 구현
            // 예: RootVisualElement.dataSource = myDataObject;
            //     RootVisualElement.SetBinding("text", new DataBinding 
            //     {
            //         dataSourcePath = new PropertyPath("playerName"),
            //         bindingMode = BindingMode.ToTarget
            //     });
        }

        public virtual void Show()
        {
            gameObject.SetActive(true);
            if (RootVisualElement != null)
            {
                RootVisualElement.style.display = DisplayStyle.Flex;
            }
            OnShown();
        }

        public virtual void Close()
        {
            OnClosed();
            if (RootVisualElement != null)
            {
                RootVisualElement.style.display = DisplayStyle.None;
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// UI가 표시된 후 호출되는 메서드
        /// 파생 클래스에서 오버라이드하여 표시 후 로직 구현
        /// </summary>
        protected virtual void OnShown()
        {
            // 파생 클래스에서 구현
        }

        /// <summary>
        /// UI가 닫히기 전 호출되는 메서드
        /// 파생 클래스에서 오버라이드하여 닫기 전 로직 구현
        /// </summary>
        protected virtual void OnClosed()
        {
            // 파생 클래스에서 구현
        }

        /// <summary>
        /// 데이터 소스를 설정하는 메서드
        /// UI Toolkit의 네이티브 데이터 바인딩 사용
        /// </summary>
        /// <param name="dataSource">바인딩할 데이터 객체</param>
        public virtual void SetDataSource(object dataSource)
        {
            if (RootVisualElement != null)
            {
                RootVisualElement.dataSource = dataSource;
                OnDataSourceChanged(dataSource);
            }
        }

        /// <summary>
        /// 데이터 소스가 변경되었을 때 호출되는 메서드
        /// 파생 클래스에서 오버라이드하여 추가 처리 구현
        /// </summary>
        /// <param name="dataSource">새로운 데이터 소스</param>
        protected virtual void OnDataSourceChanged(object dataSource)
        {
            // 파생 클래스에서 구현
        }

        /// <summary>
        /// UI Toolkit의 Q 메서드를 래핑한 헬퍼 메서드
        /// </summary>
        protected T QueryElement<T>(string name = null, string className = null) where T : VisualElement
        {
            if (RootVisualElement == null) return null;

            if (!string.IsNullOrEmpty(name))
                return RootVisualElement.Q<T>(name);
            else if (!string.IsNullOrEmpty(className))
                return RootVisualElement.Q<T>(className: className);
            else
                return RootVisualElement.Q<T>();
        }

        /// <summary>
        /// 여러 UI 요소를 한번에 찾는 헬퍼 메서드
        /// </summary>
        protected UQueryBuilder<T> QueryAllElements<T>(string className = null) where T : VisualElement
        {
            if (RootVisualElement == null) return new UQueryBuilder<T>(null);

            if (!string.IsNullOrEmpty(className))
                return RootVisualElement.Query<T>(className: className);
            else
                return RootVisualElement.Query<T>();
        }

        /// <summary>
        /// 바인딩 헬퍼 메서드 - 간단한 프로퍼티 바인딩
        /// </summary>
        protected void BindProperty(VisualElement element, string bindingPath, string dataSourcePath, BindingMode mode = BindingMode.ToTarget)
        {
            if (element == null) return;

            var binding = new DataBinding
            {
                dataSourcePath = new PropertyPath(dataSourcePath),
                bindingMode = mode
            };

            element.SetBinding(bindingPath, binding);
        }

        /// <summary>
        /// 이벤트 바인딩 헬퍼 메서드
        /// </summary>
        protected void BindEvent<T>(T element, System.Action callback) where T : VisualElement
        {
            if (element == null || callback == null) return;

            switch (element)
            {
                case Button button:
                    button.clicked += callback;
                    break;
                case Toggle toggle:
                    toggle.RegisterValueChangedCallback(evt => callback());
                    break;
                case TextField textField:
                    textField.RegisterValueChangedCallback(evt => callback());
                    break;
                // 필요에 따라 다른 컨트롤 타입 추가
            }
        }

        protected virtual void OnDestroy()
        {
            // UI Toolkit에서 이벤트 구독 해제
            UnbindEvents();
        }

        /// <summary>
        /// UI 이벤트 구독을 해제하는 메서드
        /// 파생 클래스에서 오버라이드하여 메모리 누수 방지
        /// </summary>
        protected virtual void UnbindEvents()
        {
            // 파생 클래스에서 구현
            // 예: button.clicked -= OnButtonClicked;
        }
    }
}