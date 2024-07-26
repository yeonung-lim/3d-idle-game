using UnityCommunity.UnitySingleton;
using UnityEngine;

namespace View
{
    public class UIManager : PersistentMonoSingleton<UIManager>
    {
        private BaseView[] _views;

        protected override void OnInitializing()
        {
            base.OnInitializing();

            _views = GetComponentsInChildren<BaseView>(true);
        }

        public static void ShowView<T>() where T : MonoBehaviour
        {
        }
    }
}