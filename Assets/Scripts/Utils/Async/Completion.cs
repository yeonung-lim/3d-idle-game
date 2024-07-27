using System.Threading;
using Cysharp.Threading.Tasks;

namespace Utils.Async
{
    public static class Completion
    {
        public static UniTaskCompletionSource<T> CreateWithDefaultCancelToken<T>()
        {
            var tcs = new UniTaskCompletionSource<T>();
            var cancelToken = Token.DefaultCancelToken();
            cancelToken.Register(() => tcs.TrySetCanceled());
            return tcs;
        }

        public static UniTaskCompletionSource<T> CreateWithCancelToken<T>(CancellationToken token)
        {
            var tcs = new UniTaskCompletionSource<T>();
            token.Register(() => tcs.TrySetCanceled());
            return tcs;
        }
    }

    public static class CompletedTasks
    {
        public static UniTask<bool> True => UniTask.FromResult(true);
        public static UniTask<bool> False => UniTask.FromResult(false);
    }
}