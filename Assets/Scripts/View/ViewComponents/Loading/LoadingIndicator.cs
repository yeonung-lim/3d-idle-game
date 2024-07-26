using System;

namespace View.ViewComponents.Loading
{
    /// <summary>
    ///     로딩 퍼센티지를 표시하는 인디케이터입니다.
    /// </summary>
    public class LoadingIndicator
    {
        /// <summary>
        ///     현재 로딩 퍼센티지 (0~1)
        /// </summary>
        private float _loadingPercentage;

        /// <summary>
        ///     로딩 퍼센티지가 변경될 때 호출됩니다.
        /// </summary>
        public Action<float> OnValueChanged { get; set; }

        /// <summary>
        ///     현재 로딩 퍼센티지 (0~1)
        /// </summary>
        public float LoadingPercentage
        {
            get => _loadingPercentage;
            set
            {
                _loadingPercentage = value;
                OnValueChanged?.Invoke(value);
            }
        }
    }
}