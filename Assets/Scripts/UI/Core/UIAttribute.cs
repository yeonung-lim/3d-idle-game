using System;

namespace Core.UI
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class UIAttribute : Attribute
    {
        public string AddressableKey { get; }
        public int Order { get; } // 선택사항: UI 레이어나 스택에서의 순서를 제어하기 위함

        public UIAttribute(string addressableKey, int order = 0)
        {
            if (string.IsNullOrWhiteSpace(addressableKey))
            {
                throw new ArgumentNullException(nameof(addressableKey), "Addressable 키는 null이거나 공백일 수 없습니다.");
            }
            AddressableKey = addressableKey;
            Order = order;
        }
    }
}
