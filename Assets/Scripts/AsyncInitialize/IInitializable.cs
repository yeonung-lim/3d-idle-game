namespace AsyncInitialize
{
    public interface IInitializable
    {
        public bool IsInitialized { get; }

        public void Initialize();
        public void Reset();
    }
}