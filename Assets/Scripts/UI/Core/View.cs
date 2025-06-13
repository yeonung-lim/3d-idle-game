using UnityEngine;

namespace Core.UI
{
    public class View : UIBase
    {
        // 현재 View는 기본 UIBase 구현을 사용합니다.
        // 향후 View에 특화된 로직을 여기에 추가할 수 있습니다.

        public override void Show()
        {
            base.Show();
            // View가 표시될 때의 추가 로직
            Debug.Log($"뷰 {UIName}가 표시되었습니다.");
        }

        public override void Close()
        {
            base.Close();
            // View가 닫힐 때의 추가 로직
            Debug.Log($"뷰 {UIName}가 닫혔습니다.");
        }
    }
}
