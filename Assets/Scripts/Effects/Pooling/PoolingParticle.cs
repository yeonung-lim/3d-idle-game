using UnityEngine;
using UnityEngine.Events;

namespace Effects.Pooling
{
    /// <summary>
    ///     풀링 파티클
    /// </summary>
    public class PoolingParticle : IPoolingEffect
    {
        /// <summary>
        ///     생성자
        /// </summary>
        /// <param name="effectWrapper">파티클 시스템 래퍼</param>
        public PoolingParticle(ParticleSystemWrapper effectWrapper)
        {
            EffectWrapper = effectWrapper;
        }

        /// <summary>
        ///     파티클 이미지
        /// </summary>
        public ParticleSystemWrapper EffectWrapper { get; }

        /// <summary>
        ///     이펙트 풀
        /// </summary>
        public IEffectPool EffectPool { get; set; }

        /// <summary>
        ///     이펙트 트랜스폼
        /// </summary>
        public Transform Transform => EffectWrapper.transform;

        /// <summary>
        ///     이펙트 종료 이벤트
        /// </summary>
        public UnityEvent OnFinished =>
            EffectWrapper.ParticleSystem.main.stopAction == ParticleSystemStopAction.Callback
                ? EffectWrapper.onStopped
                : null;

        /// <summary>
        ///     이펙트 재생
        /// </summary>
        public void Play()
        {
            EffectWrapper.ParticleSystem.Play();
        }

        /// <summary>
        ///     이펙트 정지
        /// </summary>
        public void Stop()
        {
            EffectWrapper.ParticleSystem.Stop();
        }

        /// <summary>
        ///     이펙트 해제
        /// </summary>
        public void Release()
        {
            EffectPool.Release(this);
        }
    }
}