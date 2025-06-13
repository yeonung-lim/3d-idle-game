using UnityEngine;

namespace Core.UI
{
    public class Popup : UIBase
    {
        // 현재 Popup은 기본 UIBase 구현을 사용합니다.
        // 향후 여기에 Popup에 특화된 로직을 추가할 수 있습니다.

        public override void Show()
        {
            base.Show();
            // 팝업이 표시될 때 추가적인 로직
            Debug.Log($"팝업 {UIName}이(가) 표시되었습니다.");
        }

        public override void Close()
        {
            base.Close();
            // 팝업이 닫힐 때 추가적인 로직
            Debug.Log($"팝업 {UIName}이(가) 닫혔습니다.");
            // 팝업은 종종 닫힐 때 파괴됩니다. View와 달리 캐시되지 않을 수 있습니다.
            // Destroy(gameObject); // 팝업을 항상 파괴해야 하는지 고려해보세요. 현재는 UIManager가 처리합니다.
        }
    }
}
