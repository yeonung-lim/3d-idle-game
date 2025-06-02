using UnityEngine;
using TMPro;
using Game.Core;

namespace Game.UI
{
    public class StageDisplayUI : MonoBehaviour
    {
        public TextMeshProUGUI stageText;

        void Start()
        {
            if (stageText == null)
            {
                Debug.LogError("StageDisplayUI: stageText (TextMeshProUGUI) 컴포넌트가 할당되지 않았습니다!", this);
                enabled = false; // 텍스트 컴포넌트가 없으면 스크립트 비활성화
                return;
            }

            if (StageManager.Instance != null)
            {
                StageManager.Instance.OnStageChanged += UpdateStageText;
                // 초기 텍스트 설정
                UpdateStageText(StageManager.Instance.currentStage);
            }
            else
            {
                Debug.LogError("StageDisplayUI: StageManager.Instance가 null입니다. OnStageChanged 이벤트 구독 및 초기 스테이지 설정이 불가능합니다.", this);
                stageText.text = "스테이지: 오류"; // UI에 오류 표시
                enabled = false;
            }
        }

        void UpdateStageText(int newStage)
        {
            if (stageText != null)
            {
                stageText.text = "스테이지: " + newStage;
            }
        }

        void OnDestroy()
        {
            if (StageManager.Instance != null)
            {
                StageManager.Instance.OnStageChanged -= UpdateStageText;
            }
        }
    }
}
