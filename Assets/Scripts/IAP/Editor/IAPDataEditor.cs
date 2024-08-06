using UnityEditor;

namespace IAP
{
    public class IAPDataEditor
    {
        [CustomEditor(typeof(IAPSettings))]
        public class AdSettingsEditor : Editor
        {
            //hide settings in inspector
            public override void OnInspectorGUI()
            {
                // base.OnInspectorGUI();
            }
        }
    }
}