using UnityEngine;

namespace Core.UI
{
    public class Popup : UIBase
    {
        // Currently, Popup uses the base UIBase implementation.
        // Specific Popup logic can be added here in the future.

        public override void Show()
        {
            base.Show();
            // Additional logic specific to Popups when they are shown
            Debug.Log($"Popup {UIName} shown.");
        }

        public override void Close()
        {
            base.Close();
            // Additional logic specific to Popups when they are closed
            Debug.Log($"Popup {UIName} closed.");
            // Popups are often destroyed when closed, unlike Views that might be cached.
            // Destroy(gameObject); // Consider if popups should always be destroyed. For now, let UIManager handle it.
        }
    }
}
