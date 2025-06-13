using UnityEngine;

namespace Core.UI
{
    public abstract class UIBase : MonoBehaviour
    {
        public string UIName { get; set; }
        public RectTransform RectTransform { get; private set; }
        public ViewModelBase ViewModel { get; private set; } // Store the ViewModel

        protected virtual void Awake()
        {
            RectTransform = GetComponent<RectTransform>();
        }

        public virtual void Show()
        {
            gameObject.SetActive(true);
            ViewModel?.OnActivated(); // Call OnActivated when shown
        }

        public virtual void Close()
        {
            ViewModel?.OnDeactivated(); // Call OnDeactivated when closed
            gameObject.SetActive(false);
        }

        public virtual void SetViewModel(ViewModelBase viewModel)
        {
            ViewModel = viewModel;
            // Derived classes can override this to perform specific binding
            // to their typed ViewModel if necessary, or to trigger initial data sync.
        }

        // This method can be called by UIManager when the UI GameObject is being destroyed
        // to ensure any Addressable assets tied to this specific instance are released.
        protected virtual void OnDestroy()
        {
            // If UIManager is not null and this instance was managed by it
            // if (UIManager.Instance != null) // Removed problematic line
            // {
            //     // UIManager.Instance.OnUIDestroyed(this);
            // }
        }
    }
}
