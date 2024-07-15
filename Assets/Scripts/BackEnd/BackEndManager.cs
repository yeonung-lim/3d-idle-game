using AsyncInitialize;
using BackEnd;
using UnityEngine;

namespace Back
{
    /// <summary>
    ///     뒤끝 관리자
    /// </summary>
    public class BackEndManager : MonoAsyncInit
    {
        public override void Reset()
        {
            IsInitialized = false;
        }

        public override CustomizableAsyncOperation GetAsyncOperation()
        {
            return CustomizableAsyncOperation.Create(() => IsInitialized);
        }

        public override void StartProcess()
        {
            var bro = Backend.Initialize(true); // 뒤끝 초기화

            // 뒤끝 초기화에 대한 응답값
            if (bro.IsSuccess())
                Debug.Log("초기화 성공 : " + bro); // 성공일 경우 statusCode 204 Success
            else
                Debug.LogError("초기화 실패 : " + bro); // 실패일 경우 statusCode 400대 에러 발생

            IsInitialized = true;
        }
    }
}