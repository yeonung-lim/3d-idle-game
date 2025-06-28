using Core.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Views
{
    /// <summary>
    /// 프리로딩 화면을 표시하는 View
    /// </summary>
    [UI("PreLoadingView")]
    public class PreLoadingView : View
    {
        private Label _loadingText;
        private ProgressBar _loadingProgress;
        private Label _loadingDescription;

        private PreLoadingData _data;

        protected override void InitializeVisualElements()
        {
            base.InitializeVisualElements();

            // UI 요소들 찾기
            _loadingText = QueryElement<Label>("loading-text");
            _loadingProgress = QueryElement<ProgressBar>("loading-progress");
            _loadingDescription = QueryElement<Label>("loading-description");

            if (_loadingProgress != null)
            {
                _loadingProgress.lowValue = 0f;
                _loadingProgress.highValue = 100f;
                _loadingProgress.value = 0f;
            }
        }

        protected override void SetupDataBinding()
        {
            base.SetupDataBinding();

            if (RootVisualElement != null)
            {
                // UI Toolkit 데이터 바인딩 설정
                if (_loadingText != null)
                {
                    BindProperty(_loadingText, "text", nameof(PreLoadingData.LoadingText));
                }

                if (_loadingProgress != null)
                {
                    BindProperty(_loadingProgress, "value", nameof(PreLoadingData.Progress));
                }

                if (_loadingDescription != null)
                {
                    BindProperty(_loadingDescription, "text", nameof(PreLoadingData.Description));
                }
            }
        }

        protected override void OnDataSourceChanged(object dataSource)
        {
            base.OnDataSourceChanged(dataSource);
            _data = dataSource as PreLoadingData;
        }

        /// <summary>
        /// 로딩 진행률을 업데이트합니다
        /// </summary>
        /// <param name="progress">0~100 사이의 진행률</param>
        public void UpdateProgress(float progress)
        {
            if (_data != null)
            {
                _data.Progress = Mathf.Clamp(progress * 100f, 0f, 100f);
            }

            // 직접 업데이트 (데이터 바인딩이 작동하지 않는 경우를 위한 폴백)
            if (_loadingProgress != null)
            {
                _loadingProgress.value = Mathf.Clamp(progress * 100f, 0f, 100f);
            }
        }

        /// <summary>
        /// 로딩 텍스트를 업데이트합니다
        /// </summary>
        /// <param name="text">표시할 텍스트</param>
        public void UpdateLoadingText(string text)
        {
            if (_data != null)
            {
                _data.LoadingText = text;
            }

            // 직접 업데이트 (데이터 바인딩이 작동하지 않는 경우를 위한 폴백)
            if (_loadingText != null)
            {
                _loadingText.text = text;
            }
        }

        /// <summary>
        /// 설명 텍스트를 업데이트합니다
        /// </summary>
        /// <param name="description">표시할 설명</param>
        public void UpdateDescription(string description)
        {
            if (_data != null)
            {
                _data.Description = description;
            }

            // 직접 업데이트 (데이터 바인딩이 작동하지 않는 경우를 위한 폴백)
            if (_loadingDescription != null)
            {
                _loadingDescription.text = description;
            }
        }
    }

    /// <summary>
    /// PreLoadingView의 데이터 모델
    /// </summary>
    [System.Serializable]
    public class PreLoadingData
    {
        public string LoadingText { get; set; } = "로딩중...";
        public float Progress { get; set; } = 0f;
        public string Description { get; set; } = "게임을 준비하는 중입니다.";
    }
}