using Effects.Pooling;
using UnityEngine;
using UnityEngine.Events;

namespace Effects
{
    public class BaseParticle : IEffect
    {
        public BaseParticle(ParticleSystemWrapper effectObject, bool isAutoRelease)
        {
            EffectObject = effectObject;

            if (isAutoRelease)
                EffectObject.onStopped.AddListener(Release);
        }

        public ParticleSystemWrapper EffectObject { get; private set; }

        public void Play()
        {
            EffectObject.ParticleSystem.Play();
        }

        public void Stop()
        {
            EffectObject.ParticleSystem.Stop();
        }

        public void Release()
        {
            Object.Destroy(EffectObject.gameObject);
            EffectObject = null;
        }

        public Transform Transform => EffectObject.transform;
        public UnityEvent OnFinished => EffectObject.onStopped;
    }
}