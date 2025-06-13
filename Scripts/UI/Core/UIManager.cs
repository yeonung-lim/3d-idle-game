using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets; // Required for Addressables
using UnityEngine.ResourceManagement.AsyncOperations; // Required for AsyncOperationHandle

namespace Core.UI
{
    // Placeholder for AddressableManager - assume it exists in this namespace or adjust as needed
    // namespace Core.Managers
    // {
    //     public class AddressableManager
    //     {
    //         private static AddressableManager _instance;
    //         public static AddressableManager Instance => _instance ?? (_instance = new AddressableManager()); // Simple Singleton
    //
    //         public async Task<T> LoadAssetAsync<T>(string key) where T : class
    //         {
    //             Debug.Log($"[AddressableManager] Attempting to load asset: {key}");
    //             AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(key);
    //             await handle.Task;
    //             if (handle.Status == AsyncOperationStatus.Succeeded)
    //             {
    //                 Debug.Log($"[AddressableManager] Asset loaded successfully: {key}");
    //                 return handle.Result;
    //             }
    //             Debug.LogError($"[AddressableManager] Failed to load asset: {key} - {handle.OperationException}");
    //             return null;
    //         }
    //
    //         public void ReleaseAsset(GameObject assetInstance) // Assuming we release by instance or key
    //         {
    //             if (assetInstance != null)
    //             {
    //                 Addressables.ReleaseInstance(assetInstance);
    //                 Debug.Log($"[AddressableManager] Released asset instance: {assetInstance.name}");
    //             }
    //         }
    //          public void ReleaseAsset<T>(T asset) where T : class // Assuming we release by instance or key
    //         {
    //             if (asset != null)
    //             {
    //                 Addressables.Release(asset);
    //                 Debug.Log($"[AddressableManager] Released asset: {asset.GetType().Name}");
    //             }
    //         }
    //     }
    // }
    // --- End of Placeholder ---

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

        [Header("UI Configuration")]
        [SerializeField] private Transform _viewLayer; // Parent for Views
        [SerializeField] private Transform _popupLayer; // Parent for Popups
        [SerializeField] private Canvas _mainCanvas; // Reference to the main canvas

        private readonly Stack<View> _viewStack = new Stack<View>();
        private readonly Stack<Popup> _popupStack = new Stack<Popup>();
        private readonly Dictionary<Type, UIBase> _loadedUIs = new Dictionary<Type, UIBase>(); // Cache for loaded UI instances (prefabs are released)
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
                    Debug.LogError("UIManager: Main Canvas not found! Please assign it or ensure one exists in the scene.");
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
            Debug.Log("UIManager Initialized.");
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
            // Consider scanning only specific assemblies if performance is an issue
            var uiTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.IsSubclassOf(typeof(UIBase)) && !t.IsAbstract);

            foreach (var type in uiTypes)
            {
                var attr = type.GetCustomAttribute<UIAttribute>();
                if (attr != null)
                {
                    if (_uiAddressableKeys.ContainsKey(type))
                    {
                        Debug.LogWarning($"UIManager: Duplicate UI type found for key scan: {type.Name}. Check UIAttributes.");
                        continue;
                    }
                    _uiAddressableKeys.Add(type, attr.AddressableKey);
                    Debug.Log($"UIManager: Registered UI {type.Name} with key {attr.AddressableKey}");
                }
                else
                {
                    Debug.LogWarning($"UIManager: UI type {type.Name} is missing UIAttribute. It cannot be loaded by type.");
                }
            }
        }

        void Update()
        {
            // Handle Back Key (Escape)
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
                CloseCurrentView(); // Or implement more sophisticated back navigation for views
            }
            else
            {
                Debug.Log("UIManager: No UI open to close with back button.");
                // Optionally, Application.Quit(); or other app-specific behavior
            }
        }

        #region View Management
        public async Task<TView> ShowView<TView>(ViewModelBase viewModel = null) where TView : View
        {
            if (!_isInitialized) Initialize();

            if (!_uiAddressableKeys.TryGetValue(typeof(TView), out var key))
            {
                Debug.LogError($"UIManager: No Addressable key found for View type {typeof(TView).Name}. Did you forget [UIAttribute]?");
                return null;
            }

            // Close current view if one is open
            if (_viewStack.Count > 0)
            {
                View currentView = _viewStack.Peek();
                // Optimization: if it's the same view type, maybe just update ViewModel?
                // For now, always close and open.
                CloseView(currentView);
            }

            TView viewInstance = await GetOrCreateUI<TView>(key, _viewLayer);
            if (viewInstance != null)
            {
                viewInstance.SetViewModel(viewModel); // Bind ViewModel
                // viewModel?.OnActivated(); // Notify ViewModel -- Handled by UIBase.Show()
                viewInstance.Show();
                _viewStack.Push(viewInstance);
                Debug.Log($"UIManager: View {typeof(TView).Name} shown.");
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

            if (_viewStack.Count > 0 && _viewStack.Peek() == view) // Ensure it's the top view
            {
                _viewStack.Pop();
            }
            else if (_viewStack.Contains(view)) // Should not happen with current logic but good for robustness
            {
                // This case implies trying to close a view that is not the current top one,
                // which the current design (only one view at a time) prevents.
                // Rebuild stack if necessary, though ideally this path isn't hit.
                var tempStack = new Stack<View>(_viewStack.Reverse().Where(v => v != view));
                _viewStack.Clear();
                //foreach(var v in tempStack) _viewStack.Push(v); // This is wrong, need to reverse again.
                                                                // Simpler: just clear and repopulate from a filtered list
                var list = _viewStack.ToList();
                list.Remove(view);
                _viewStack.Clear();
                foreach(var v_item in ((IEnumerable<View>)list).Reverse()) _viewStack.Push(v_item);

                Debug.LogWarning($"UIManager: Closed a view ({view.UIName}) that was not at the top of the stack. This is unusual.");
            }

            // (viewModelProperty.GetValue(view) as ViewModelBase)?.OnDeactivated(); -- Handled by UIBase.Close()

            view.Close();
            // We don't destroy Views, just deactivate them. They are cached in _loadedUIs.
            Debug.Log($"UIManager: View {view.UIName} closed.");

            // Optional: Show previous view if maintaining a history stack deeper than current _viewStack
        }
        #endregion

        #region Popup Management
        public async Task<TPopup> ShowPopup<TPopup>(ViewModelBase viewModel = null) where TPopup : Popup
        {
            if (!_isInitialized) Initialize();

            if (!_uiAddressableKeys.TryGetValue(typeof(TPopup), out var key))
            {
                Debug.LogError($"UIManager: No Addressable key found for Popup type {typeof(TPopup).Name}. Did you forget [UIAttribute]?");
                return null;
            }

            TPopup popupInstance = await GetOrCreateUI<TPopup>(key, _popupLayer);
            if (popupInstance != null)
            {
                popupInstance.transform.SetAsLastSibling(); // Ensure it's on top
                popupInstance.SetViewModel(viewModel); // Bind ViewModel
                // viewModel?.OnActivated(); // Notify ViewModel -- Handled by UIBase.Show()
                popupInstance.Show();
                _popupStack.Push(popupInstance);
                Debug.Log($"UIManager: Popup {typeof(TPopup).Name} shown.");
            }
            return popupInstance;
        }

        public void CloseTopPopup()
        {
            if (_popupStack.Count > 0)
            {
                Popup popup = _popupStack.Pop();

                // (viewModelProperty.GetValue(popup) as ViewModelBase)?.OnDeactivated(); -- Handled by UIBase.Close()

                popup.Close();
                // Unlike Views, Popups are typically destroyed when closed.
                // AddressableManager.Instance.ReleaseAsset(popup.gameObject); // Release the instance
                // _loadedUIs.Remove(popup.GetType()); // Remove from cache
                // Destroy(popup.gameObject); // Destroy the GameObject
                // For now, let's also cache popups like views. This can be changed.
                Debug.Log($"UIManager: Popup {popup.UIName} closed.");
            }
        }

        public void ClosePopup(Popup popupToClose)
        {
            if (popupToClose == null || !_popupStack.Contains(popupToClose)) return;

            // Rebuild the stack without the closed popup
            var currentPopups = _popupStack.ToList();
            _popupStack.Clear();
            foreach (var p in currentPopups.Where(p => p != popupToClose).Reverse())
            {
                _popupStack.Push(p);
            }

            // (viewModelProperty.GetValue(popupToClose) as ViewModelBase)?.OnDeactivated(); -- Handled by UIBase.Close()

            popupToClose.Close();
            // AddressableManager.Instance.ReleaseAsset(popupToClose.gameObject);
            // _loadedUIs.Remove(popupToClose.GetType());
            // Destroy(popupToClose.gameObject);
            Debug.Log($"UIManager: Popup {popupToClose.UIName} specifically closed.");
        }

        public void CloseAllPopups()
        {
            while (_popupStack.Count > 0)
            {
                CloseTopPopup();
            }
        }
        #endregion

        #region UI Loading and Caching
        private async Task<T> GetOrCreateUI<T>(string key, Transform parentLayer) where T : UIBase
        {
            if (_loadedUIs.TryGetValue(typeof(T), out UIBase cachedUI) && cachedUI != null)
            {
                Debug.Log($"UIManager: Returning cached UI for {typeof(T).Name}");
                cachedUI.transform.SetParent(parentLayer, false); // Ensure it's under the correct layer
                // Reset scale and rect transform properties if necessary
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
                Debug.LogError($"UIManager: Failed to load UI prefab with key: {key} for type {typeof(T).Name}");
                return null;
            }

            GameObject uiInstanceGo = Instantiate(prefab, parentLayer);
            T uiInstance = uiInstanceGo.GetComponent<T>();

            if (uiInstance == null)
            {
                Debug.LogError($"UIManager: Prefab for key {key} does not have a component of type {typeof(T).Name}. Destroying instance.");
                Destroy(uiInstanceGo);
                ReleasePrefabFromAddressables(prefab); // Release loaded prefab if instance creation failed
                return null;
            }

            uiInstance.UIName = key; // Or use typeof(T).Name
            _loadedUIs[typeof(T)] = uiInstance; // Cache the instance
            Debug.Log($"UIManager: Instantiated and cached UI {typeof(T).Name} from key {key}");

            // Release the prefab asset itself if Addressables uses reference counting.
            // Instantiated objects (uiInstanceGo) are separate and need their own Addressables.ReleaseInstance when destroyed.
            ReleasePrefabFromAddressables(prefab);


            return uiInstance;
        }

        // Placeholder for actual Addressable loading
        private async Task<T> LoadPrefabFromAddressables<T>(string key) where T : class
        {
            Debug.Log($"[UIManager/Addressables] Attempting to load asset: {key}");
            AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(key);
            await handle.Task;
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                Debug.Log($"[UIManager/Addressables] Asset loaded successfully: {key}");
                return handle.Result;
            }
            Debug.LogError($"[UIManager/Addressables] Failed to load asset: {key} - {handle.OperationException}");
            return null;
        }

        private void ReleasePrefabFromAddressables<T>(T asset) where T : class
        {
            if (asset != null)
            {
                Addressables.Release(asset);
                Debug.Log($"[UIManager/Addressables] Released addressable asset (e.g. prefab): {asset.GetType().Name} associated with a key.");
            }
        }
         private void ReleaseInstanceFromAddressables(GameObject instance)
        {
            if (instance != null)
            {
                Addressables.ReleaseInstance(instance);
                Debug.Log($"[UIManager/Addressables] Released addressable instance: {instance.name}");
            }
        }

        public void OnUIDestroyed(UIBase uiElement)
        {
            if (uiElement == null) return;

            var type = uiElement.GetType();
            if (_loadedUIs.ContainsKey(type) && _loadedUIs[type] == uiElement)
            {
                _loadedUIs.Remove(type);
                Debug.Log($"UIManager: UI element {type.Name} removed from cache upon its GameObject destruction.");
            }
            // The GameObject instance itself should be released if it was created by Addressables.
            // This is typically handled by calling Addressables.ReleaseInstance(gameObject) in the UIBase's OnDestroy method.
            // So, UIManager doesn't need to explicitly call ReleaseInstanceFromAddressables here IF UIBase does it.
            // For now, let's assume UIBase will handle its own instance release.
        }
        #endregion

        protected void OnDestroy()
        {
            // Cleanup: Close all UIs and release assets if necessary
            CloseAllPopups(); // These are cached, so their GameObjects are just deactivated.
            if (_viewStack.Count > 0) CloseView(_viewStack.Peek()); // Close current view

            foreach (var pair in _loadedUIs)
            {
                if (pair.Value != null)
                {
                    // If instances are managed by Addressables (e.g. instantiated from an Addressable prefab)
                    // they need to be released.
                    ReleaseInstanceFromAddressables(pair.Value.gameObject);
                    // Destroying the GameObject might be redundant if ReleaseInstance handles it,
                    // or necessary if ReleaseInstance only decrements a counter.
                    // For safety, let's ensure it's destroyed.
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
