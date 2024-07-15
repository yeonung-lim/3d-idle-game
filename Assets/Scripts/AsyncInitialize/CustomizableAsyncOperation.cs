using System;
using UnityEngine;

namespace AsyncInitialize
{
    /// <summary>
    ///     비동기 작업을 커스터마이징할 수 있는 클래스
    /// </summary>
    public class CustomizableAsyncOperation : CustomYieldInstruction
    {
        /// <summary>
        ///     완료 여부를 반환하는 함수
        /// </summary>
        private Func<bool> _donePredicate;

        /// <summary>
        ///     작업의 진행률을 반환
        /// </summary>
        private Func<float> _progressPredicate;

        /// <summary>
        ///     작업이 완료되었는지 여부를 반환
        /// </summary>
        public bool IsDone => _donePredicate();


        /// <summary>
        ///     작업의 진행률을 반환합니다.
        /// </summary>
        public float Progress => Mathf.Clamp01(_progressPredicate());

        /// <summary>
        ///     작업이 완료될 때까지 대기합니다.
        /// </summary>
        public override bool keepWaiting => !IsDone;

        /// <summary>
        ///     IAsyncInit를 상속받은 클래스가 구현해야 하는 메서드입니다.
        ///     진행률과 완료 여부를 판별할 수 있도록 해야합니다.
        /// </summary>
        /// <param name="donePredicate">완료 여부를 반환하는 함수입니다.</param>
        /// <returns>CustomizableAsyncOperation</returns>
        public static CustomizableAsyncOperation Create(Func<bool> donePredicate)
        {
            return new CustomizableAsyncOperation
            {
                _donePredicate = donePredicate,
                _progressPredicate = () => donePredicate() ? 1f : 0f
            };
        }

        /// <summary>
        ///     IAsyncInit를 상속받은 클래스가 구현해야 하는 메서드입니다.
        ///     진행률과 완료 여부를 판별할 수 있도록 해야합니다.
        /// </summary>
        /// <param name="donePredicate">완료 여부를 반환하는 함수입니다.</param>
        /// <param name="progressPredicate">진행률을 반환하는 함수입니다.</param>
        /// <returns>CustomizableAsyncOperation</returns>
        public static CustomizableAsyncOperation Create(Func<bool> donePredicate, Func<float> progressPredicate)
        {
            return new CustomizableAsyncOperation
            {
                _donePredicate = donePredicate,
                _progressPredicate = progressPredicate
            };
        }
    }
}