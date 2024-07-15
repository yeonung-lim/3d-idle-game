using UnityEngine;

namespace AsyncInitialize
{
    public abstract class MonoAsyncInit : MonoBehaviour, IAsyncInit
    {
        public bool IsInitialized { get; protected set; }
        
        public abstract CustomizableAsyncOperation GetAsyncOperation();
        public abstract void StartProcess();
        public abstract void Reset();
    }
}