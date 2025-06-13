using UnityEngine;

namespace Core.UI
{
    public class View : UIBase
    {
        // Currently, View uses the base UIBase implementation.
        // Specific View logic can be added here in the future.

        public override void Show()
        {
            base.Show();
            // Additional logic specific to Views when they are shown
            Debug.Log($"View {UIName} shown.");
        }

        public override void Close()
        {
            base.Close();
            // Additional logic specific to Views when they are closed
            Debug.Log($"View {UIName} closed.");
        }
    }
}
