namespace AsyncInitialize
{
    /// <summary>
    ///     StartProcess()를 호출하여 비동기 작업을 시작할 수 있는 인터페이스입니다.
    ///     GetAsyncOperation()로 반환되는 인스턴스로 비동기 작업 완료 여부를 확인할 수 있습니다.
    /// </summary>
    public interface IAsyncInit
    {
        public bool IsInitialized { get; }

        /// <summary>
        ///     비동기 작업의 진행 상황을 반환합니다.
        /// </summary>
        /// <returns></returns>
        CustomizableAsyncOperation GetAsyncOperation();

        public void StartInitialize();
        public void Reset();
    }
}