using UnityEngine;
using UnityEngine.Events;

namespace Effects.Pooling
{
    [RequireComponent(typeof(ParticleSystem))]
    public class ParticleSystemWrapper : MonoBehaviour
    {
        public UnityEvent onStopped;
        private ParticleSystem _particleSystem;

        public ParticleSystem ParticleSystem
        {
            get
            {
                if (_particleSystem == null) _particleSystem = GetComponent<ParticleSystem>();

                return _particleSystem;
            }
        }

        private void OnParticleSystemStopped()
        {
            onStopped?.Invoke();
        }
    }
}