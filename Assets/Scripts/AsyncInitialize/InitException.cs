using System;

namespace AsyncInitialize
{
    /// <summary>
    ///     IAsyncInit 비동기 작업을 초기화하는 과정에서 발생하는 예외입니다.
    /// </summary>
    public class AsyncInitException : Exception
    {
        public AsyncInitException(string message) : base(message)
        {
        }
    }
}