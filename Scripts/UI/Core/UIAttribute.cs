using System;

namespace Core.UI
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class UIAttribute : Attribute
    {
        public string AddressableKey { get; }
        public int Order { get; } // Optional: For controlling order in UI layers or stacks

        public UIAttribute(string addressableKey, int order = 0)
        {
            if (string.IsNullOrWhiteSpace(addressableKey))
            {
                throw new ArgumentNullException(nameof(addressableKey), "Addressable key cannot be null or whitespace.");
            }
            AddressableKey = addressableKey;
            Order = order;
        }
    }
}
