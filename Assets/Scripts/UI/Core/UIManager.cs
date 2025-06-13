using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets; // Addressables에 필요
using UnityEngine.ResourceManagement.AsyncOperations; // AsyncOperationHandle에 필요

namespace Core.UI
{
    // AddressableManager를 위한 자리 표시자 - 이 네임스페이스에 있다고 가정하거나 필요에 따라 조정
    // namespace Core.Managers
    // {
    //     public class AddressableManager
    //     {
    //         private static AddressableManager _instance;
    //         public static AddressableManager Instance => _instance ?? (_instance = new AddressableManager()); // 간단한 싱글톤
    //
    //         public async Task<T> LoadAssetAsync<T>(string key) where T : class
    //         {
    //             Debug.Log($"[AddressableManager] 에셋 로드 시도: {key}");
    //             AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(key);
    //             await handle.Task;
    //             if (handle.Status == AsyncOperationStatus.Succeeded)
    //             {
    //                 Debug.Log($"[AddressableManager] 에셋 로드 성공: {key}");
    //                 return handle.Result;
    //             }
    //             Debug.LogError($"[AddressableManager] 에셋 로드 실패: {key} - {handle.OperationException}");
    //             return null;
    //         }
    //
    //         public void ReleaseAsset(GameObject assetInstance) // 인스턴스나 키로 해제한다고 가정
    //         {
    //             if (assetInstance != null)
    //             {
    //                 Addressables.ReleaseInstance(assetInstance);
    //                 Debug.Log($"[AddressableManager] 에셋 인스턴스 해제: {assetInstance.name}");
    //             }
    //         }
    //          public void ReleaseAsset<T>(T asset) where T : class // 인스턴스나 키로 해제한다고 가정
    //         {
    //             if (asset != null)
    //             {
    //                 Addressables.Release(asset);
    //                 Debug.Log($"[AddressableManager] 에셋 해제: {asset.GetType().Name}");
    //             }
    //         }
    //     }
    // }
    // --- 자리 표시자 끝 ---

    public class UIManager : MonoBehaviour
    {
        #region Singleton
        private static UIManager _instance;
        public static UIManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<UIManager>();
                    if (_instance == null)
                    {
                        GameObject singletonObject = new GameObject(nameof(UIManager));
                        _instance = singletonObject.AddComponent<UIManager>();
                        DontDestroyOnLoad(singletonObject);
                    }
                }
                return _instance;
            }
        }
        #endregion

        [Header("UI 설정")]
        [SerializeField] private Transform _viewLayer; // View를 위한 부모
        [SerializeField] private Transform _popupLayer; // Popup을 위한 부모
        [SerializeField] private Canvas _mainCanvas; // 메인 캔버스 참조

        private readonly Stack<View> _viewStack = new Stack<View>();
        private readonly Stack<Popup> _popupStack = new Stack<Popup>();
        private readonly Dictionary<Type, UIBase> _loadedUIs = new Dictionary<Type, UIBase>(); // 로드된 UI 인스턴스 캐시 (프리팹은 해제됨)
        private readonly Dictionary<Type, string> _uiAddressableKeys = new Dictionary<Type, string>();

        private bool _isInitialized = false;

        protected void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            Initialize();
        }

        private void Initialize()
        {
            if (_isInitialized) return;

            if (_mainCanvas == null)
            {
                _mainCanvas = FindObjectOfType<Canvas>();
                if (_mainCanvas == null)
                {
                    Debug.LogError("UIManager: 메인 캔버스를 찾을 수 없습니다! 할당하거나 씬에 하나가 있는지 확인해주세요.");
                    return;
                }
            }

            if (_viewLayer == null)
            {
                _viewLayer = CreateLayer("ViewLayer", _mainCanvas.transform);
            }
            if (_popupLayer == null)
            {
                _popupLayer = CreateLayer("PopupLayer", _mainCanvas.transform);
            }

            ScanUIAttributes();
            _isInitialized = true;
            Debug.Log("UIManager 초기화 완료.");
        }

        private Transform CreateLayer(string name, Transform parent)
        {
            GameObject layerObject = new GameObject(name);
            RectTransform rectTransform = layerObject.AddComponent<RectTransform>();
            rectTransform.SetParent(parent, false);
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.localScale = Vector3.one;
            return layerObject.transform;
        }

        private void ScanUIAttributes()
        {
            _uiAddressableKeys.Clear();
            // 성능 문제가 있다면 특정 어셈블리만 스캔하는 것을 고려
            var uiTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.IsSubclassOf(typeof(UIBase)) && !t.IsAbstract);

            foreach (var type in uiTypes)
            {
                var attr = type.GetCustomAttribute<UIAttribute>();
                if (attr != null)
                {
                    if (_uiAddressableKeys.ContainsKey(type))
                    {
                        Debug.LogWarning($"UIManager: 키 스캔 중 중복된 UI 타입 발견: {type.Name}. UIAttributes를 확인해주세요.");
                        continue;
                    }
                    _uiAddressableKeys.Add(type, attr.AddressableKey);
                    Debug.Log($"UIManager: UI {type.Name}을(를) 키 {attr.AddressableKey}로 등록했습니다.");
                }
                else
                {
                    Debug.LogWarning($"UIManager: UI 타입 {type.Name}에 UIAttribute가 없습니다. 타입으로 로드할 수 없습니다.");
                }
            }
        }

        void Update()
        {
            // 뒤로가기 키 (Escape) 처리
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                HandleBackButton();
            }
        }

        public void HandleBackButton()
        {
            if (_popupStack.Count > 0)
            {
                CloseTopPopup();
            }
            else if (_viewStack.Count > 0)
            {
                CloseCurrentView(); // 또는 View에 대한 더 정교한 뒤로가기 네비게이션 구현
            }
            else
            {
                Debug.Log("UIManager: 뒤로가기 버튼으로 닫을 UI가 없습니다.");
                // 선택적으로 Application.Quit(); 또는 다른 앱별 동작
            }
        }

        #region View 관리
        public async Task<TView> ShowView<TView>(ViewModelBase viewModel = null) where TView : View
        {
            if (!_isInitialized) Initialize();

            if (!_uiAddressableKeys.TryGetValue(typeof(TView), out var key))
            {
                Debug.LogError($"UIManager: View 타입 {typeof(TView).Name}에 대한 Addressable 키를 찾을 수 없습니다. [UIAttribute]를 잊으셨나요?");
                return null;
            }

            // 현재 열려있는 View가 있다면 닫기
            if (_viewStack.Count > 0)
            {
                View currentView = _viewStack.Peek();
                // 최적화: 같은 View 타입이라면 ViewModel만 업데이트할 수 있음
                // 지금은 항상 닫고 열기로 구현
                CloseView(currentView);
            }

            TView viewInstance = await GetOrCreateUI<TView>(key, _viewLayer);
            if (viewInstance != null)
            {
                viewInstance.SetViewModel(viewModel); // ViewModel 바인딩
                // viewModel?.OnActivated(); // ViewModel에 알림 -- UIBase.Show()에서 처리됨
                viewInstance.Show();
                _viewStack.Push(viewInstance);
                Debug.Log($"UIManager: View {typeof(TView).Name} 표시됨.");
            }
            return viewInstance;
        }

        public void CloseCurrentView()
        {
            if (_viewStack.Count > 0)
            {
                CloseView(_viewStack.Peek());
            }
        }

        private void CloseView(View view)
        {
            if (view == null) return;

            if (_viewStack.Count > 0 && _viewStack.Peek() == view) // 최상위 View인지 확인
            {
                _viewStack.Pop();
            }
            else if (_viewStack.Contains(view)) // 현재 로직으로는 발생하지 않아야 하지만 견고성을 위해 유지
            {
                // 이 경우는 최상위가 아닌 View를 닫으려고 시도하는 것
                // 현재 설계(한 번에 하나의 View만)에서는 방지됨
                // 필요한 경우 스택을 재구성
                var tempStack = new Stack<View>(_viewStack.Reverse().Where(v => v != view));
                _viewStack.Clear();
                //foreach(var v in tempStack) _viewStack.Push(v); // 잘못됨, 다시 역순으로 해야 함
                                                                // 더 간단하게: 필터링된 목록에서 다시 채우기
                var list = _viewStack.ToList();
                list.Remove(view);
                _viewStack.Clear();
                foreach(var v_item in ((IEnumerable<View>)list).Reverse()) _viewStack.Push(v_item);

                Debug.LogWarning($"UIManager: 스택의 맨 위가 아닌 View({view.UIName})를 닫았습니다. 이는 비정상적인 상황입니다.");
            }

            // (viewModelProperty.GetValue(view) as ViewModelBase)?.OnDeactivated(); -- UIBase.Close()에서 처리됨

            view.Close();
            // View는 파괴하지 않고 비활성화만 합니다. _loadedUIs에 캐시됩니다.
            Debug.Log($"UIManager: View {view.UIName} 닫힘.");

            // 선택사항: 현재 _viewStack보다 더 깊은 히스토리 스택을 유지한다면 이전 View 표시
        }
        #endregion

        #region Popup 관리
        public async Task<TPopup> ShowPopup<TPopup>(ViewModelBase viewModel = null) where TPopup : Popup
        {
            if (!_isInitialized) Initialize();

            if (!_uiAddressableKeys.TryGetValue(typeof(TPopup), out var key))
            {
                Debug.LogError($"UIManager: Popup 타입 {typeof(TPopup).Name}에 대한 Addressable 키를 찾을 수 없습니다. [UIAttribute]를 잊으셨나요?");
                return null;
            }

            TPopup popupInstance = await GetOrCreateUI<TPopup>(key, _popupLayer);
            if (popupInstance != null)
            {
                popupInstance.transform.SetAsLastSibling(); // 최상위에 표시
                popupInstance.SetViewModel(viewModel); // ViewModel 바인딩
                // viewModel?.OnActivated(); // ViewModel에 알림 -- UIBase.Show()에서 처리됨
                popupInstance.Show();
                _popupStack.Push(popupInstance);
                Debug.Log($"UIManager: Popup {typeof(TPopup).Name} 표시됨.");
            }
            return popupInstance;
        }

        public void CloseTopPopup()
        {
            if (_popupStack.Count > 0)
            {
                Popup popup = _popupStack.Pop();

                // (viewModelProperty.GetValue(popup) as ViewModelBase)?.OnDeactivated(); -- UIBase.Close()에서 처리됨

                popup.Close();
                // View와 달리 Popup은 일반적으로 닫을 때 파괴됩니다.
                // AddressableManager.Instance.ReleaseAsset(popup.gameObject); // 인스턴스 해제
                // _loadedUIs.Remove(popup.GetType()); // 캐시에서 제거
                // Destroy(popup.gameObject); // GameObject 파괴
                // 지금은 View처럼 Popup도 캐시하도록 구현. 변경 가능.
                Debug.Log($"UIManager: Popup {popup.UIName} 닫힘.");
            }
        }

        public void ClosePopup(Popup popupToClose)
        {
            if (popupToClose == null || !_popupStack.Contains(popupToClose)) return;

            // 닫을 Popup을 제외하고 스택 재구성
            var currentPopups = _popupStack.ToList();
            _popupStack.Clear();
            foreach (var p in currentPopups.Where(p => p != popupToClose).Reverse())
            {
                _popupStack.Push(p);
            }

            // (viewModelProperty.GetValue(popupToClose) as ViewModelBase)?.OnDeactivated(); -- UIBase.Close()에서 처리됨

            popupToClose.Close();
            // AddressableManager.Instance.ReleaseAsset(popupToClose.gameObject);
            // _loadedUIs.Remove(popupToClose.GetType());
            // Destroy(popupToClose.gameObject);
            Debug.Log($"UIManager: Popup {popupToClose.UIName} 특정 닫힘.");
        }

        public void CloseAllPopups()
        {
            while (_popupStack.Count > 0)
            {
                CloseTopPopup();
            }
        }
        #endregion

        #region UI 로딩 및 캐싱
        private async Task<T> GetOrCreateUI<T>(string key, Transform parentLayer) where T : UIBase
        {
            if (_loadedUIs.TryGetValue(typeof(T), out UIBase cachedUI) && cachedUI != null)
            {
                Debug.Log($"UIManager: {typeof(T).Name}에 대한 캐시된 UI 반환");
                cachedUI.transform.SetParent(parentLayer, false); // 올바른 레이어 아래에 있는지 확인
                // 필요한 경우 스케일과 rect transform 속성 초기화
                RectTransform rt = cachedUI.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchorMin = Vector2.zero;
                    rt.anchorMax = Vector2.one;
                    rt.offsetMin = Vector2.zero;
                    rt.offsetMax = Vector2.zero;
                    rt.localScale = Vector3.one;
                }
                return cachedUI as T;
            }

            GameObject prefab = await LoadPrefabFromAddressables<GameObject>(key);

            if (prefab == null)
            {
                Debug.LogError($"UIManager: 키 {key}로 UI 프리팹 로드 실패: {typeof(T).Name} 타입");
                return null;
            }

            GameObject uiInstanceGo = Instantiate(prefab, parentLayer);
            T uiInstance = uiInstanceGo.GetComponent<T>();

            if (uiInstance == null)
            {
                Debug.LogError($"UIManager: 키 {key}의 프리팹이 {typeof(T).Name} 타입의 컴포넌트를 가지고 있지 않습니다. 인스턴스를 파괴합니다.");
                Destroy(uiInstanceGo);
                ReleasePrefabFromAddressables(prefab); // 인스턴스 생성 실패 시 로드된 프리팹 해제
                return null;
            }

            uiInstance.UIName = key; // 또는 typeof(T).Name 사용
            _loadedUIs[typeof(T)] = uiInstance; // 인스턴스 캐시
            Debug.Log($"UIManager: 키 {key}에서 UI {typeof(T).Name} 인스턴스화 및 캐시됨");

            // Addressables가 참조 카운팅을 사용한다면 프리팹 에셋 자체를 해제
            // 인스턴스화된 객체(uiInstanceGo)는 별개이며 파괴될 때 자체적인 Addressables.ReleaseInstance가 필요
            ReleasePrefabFromAddressables(prefab);

            return uiInstance;
        }

        // 실제 Addressable 로딩을 위한 자리 표시자
        private async Task<T> LoadPrefabFromAddressables<T>(string key) where T : class
        {
            Debug.Log($"[UIManager/Addressables] 에셋 로드 시도: {key}");
            AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(key);
            await handle.Task;
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                Debug.Log($"[UIManager/Addressables] 에셋 로드 성공: {key}");
                return handle.Result;
            }
            Debug.LogError($"[UIManager/Addressables] 에셋 로드 실패: {key} - {handle.OperationException}");
            return null;
        }

        private void ReleasePrefabFromAddressables<T>(T asset) where T : class
        {
            if (asset != null)
            {
                Addressables.Release(asset);
                Debug.Log($"[UIManager/Addressables] 주소 지정 가능한 에셋(예: 프리팹) 해제: {asset.GetType().Name} 키와 연관됨");
            }
        }
         private void ReleaseInstanceFromAddressables(GameObject instance)
        {
            if (instance != null)
            {
                Addressables.ReleaseInstance(instance);
                Debug.Log($"[UIManager/Addressables] 주소 지정 가능한 인스턴스 해제: {instance.name}");
            }
        }

        public void OnUIDestroyed(UIBase uiElement)
        {
            if (uiElement == null) return;

            var type = uiElement.GetType();
            if (_loadedUIs.ContainsKey(type) && _loadedUIs[type] == uiElement)
            {
                _loadedUIs.Remove(type);
                Debug.Log($"UIManager: UI 요소 {type.Name}이(가) GameObject 파괴 시 캐시에서 제거됨");
            }
            // GameObject 인스턴스 자체는 Addressables에 의해 생성된 경우 해제되어야 함
            // 이는 일반적으로 UIBase의 OnDestroy 메서드에서 Addressables.ReleaseInstance(gameObject)를 호출하여 처리됨
            // 따라서 UIBase가 이를 처리한다면 UIManager는 여기서 명시적으로 ReleaseInstanceFromAddressables를 호출할 필요가 없음
            // 지금은 UIBase가 자체 인스턴스 해제를 처리한다고 가정
        }
        #endregion

        protected void OnDestroy()
        {
            // 정리: 모든 UI를 닫고 필요한 경우 에셋 해제
            CloseAllPopups(); // 캐시되어 있으므로 GameObject는 비활성화만 됨
            if (_viewStack.Count > 0) CloseView(_viewStack.Peek()); // 현재 View 닫기

            foreach (var pair in _loadedUIs)
            {
                if (pair.Value != null)
                {
                    // 인스턴스가 Addressables에 의해 관리되는 경우(예: Addressable 프리팹에서 인스턴스화)
                    // 해제가 필요함
                    ReleaseInstanceFromAddressables(pair.Value.gameObject);
                    // ReleaseInstance가 GameObject를 처리한다면 Destroy는 중복될 수 있음
                    // 또는 ReleaseInstance가 카운터만 감소시킨다면 Destroy가 필요할 수 있음
                    // 안전을 위해 파괴 확인
                    if(pair.Value.gameObject != null) Destroy(pair.Value.gameObject);
                }
            }
            _loadedUIs.Clear();
            _uiAddressableKeys.Clear();

            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}
