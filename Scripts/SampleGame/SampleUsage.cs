using UnityEngine;
using Core.UI; // For UIManager
using SampleGame.UI.ViewModels; // For SampleViewModel
using SampleGame.UI.Views; // For SampleView
using SampleGame.UI.Popups; // For SamplePopup
using System.Threading.Tasks; // For Task

namespace SampleGame
{
    public class SampleUsage : MonoBehaviour
    {
        async void Start()
        {
            // Ensure UIManager is ready (it's a Singleton, should initialize itself)
            if (UIManager.Instance == null)
            {
                Debug.LogError("UIManager instance not found!");
                return;
            }

            // Give UIManager a moment to initialize if it's being created on demand.
            // In a real game, you might have an initialization sequence or splash screen.
            await Task.Delay(100); // Small delay to ensure UIManager.Awake runs.

            ShowSampleView();
        }

        public async void ShowSampleView()
        {
            Debug.Log("SampleUsage: Attempting to show SampleView...");
            var sampleViewModel = new SampleViewModel();
            // The UIManager will handle setting this view model to the view instance
            View view = await UIManager.Instance.ShowView<SampleView>(sampleViewModel);
            if (view != null)
            {
                Debug.Log("SampleUsage: SampleView shown successfully.");
            }
            else
            {
                Debug.LogError("SampleUsage: Failed to show SampleView.");
            }
        }

        // This method could be called by a button in the SampleView,
        // but for this example, SampleView's button directly calls UIManager.
        // This is just another way to show a popup.
        public async void ShowSamplePopupFromExternal()
        {
            Debug.Log("SampleUsage: Attempting to show SamplePopup externally...");
            var popupViewModel = new SamplePopupViewModel("Externally Opened Popup");
            Popup popup = await UIManager.Instance.ShowPopup<SamplePopup>(popupViewModel);
            if (popup != null)
            {
                Debug.Log("SampleUsage: SamplePopup shown successfully from external call.");
            }
            else
            {
                Debug.LogError("SampleUsage: Failed to show SamplePopup from external call.");
            }
        }

        void Update()
        {
            // Example: Press 'P' to show a popup
            if (Input.GetKeyDown(KeyCode.P))
            {
                ShowSamplePopupFromExternal();
            }

            // Example: Press 'C' to close current view
             if (Input.GetKeyDown(KeyCode.C))
            {
                UIManager.Instance.CloseCurrentView();
            }
        }
    }
}
