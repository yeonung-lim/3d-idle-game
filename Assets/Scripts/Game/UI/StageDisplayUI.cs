using UnityEngine;
using TMPro; // Using TextMeshPro
using Game.Core; // For StageManager

namespace Game.UI
{
    public class StageDisplayUI : MonoBehaviour
    {
        public TextMeshProUGUI stageText; // Assign in Inspector

        void Start()
        {
            if (stageText == null)
            {
                Debug.LogError("StageDisplayUI: stageText (TextMeshProUGUI) is not assigned!", this);
                enabled = false; // Disable script if text component is missing
                return;
            }

            if (StageManager.Instance != null)
            {
                StageManager.Instance.OnStageChanged += UpdateStageText;
                // Set initial text
                UpdateStageText(StageManager.Instance.currentStage);
            }
            else
            {
                Debug.LogError("StageDisplayUI: StageManager.Instance is null. Cannot subscribe to OnStageChanged event or get initial stage.", this);
                stageText.text = "Stage: Error"; // Display error on UI
                enabled = false;
            }
        }

        void UpdateStageText(int newStage)
        {
            if (stageText != null)
            {
                stageText.text = "Stage: " + newStage;
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
