using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Utils.Async
{
    public static class Token
    {
        private static readonly Lazy<GameObject> ObservableObject = new(() =>
        {
            var go = new GameObject("ObservableObject");
            Object.DontDestroyOnLoad(go);
            go.hideFlags = HideFlags.HideAndDontSave;
            return go;
        });

        public static CancellationToken DefaultCancelToken()
        {
            return ObservableObject.Value.GetCancellationTokenOnDestroy();
        }
    }
}