using UnityEngine;
#if UNITY_EDITOR
using JD.EditorAudioUtils;
#endif

namespace Sound.Utils
{
    /// <summary>
    ///     에디터에서 사운드를 재생하는 클래스입니다.
    /// </summary>
    public class PlaySoundInEditor : MonoBehaviour
    {
#if UNITY_EDITOR
        [SerializeField] private AudioClip _audioClip;
#endif

        /// <summary>
        ///     _audioClip 변수에 할당된 사운드를 재생합니다.
        /// </summary>
        public void Play()
        {
#if UNITY_EDITOR
            if (Application.isPlaying) return;
            EditorAudioUtility.PlayPreviewClip(_audioClip);
#endif
        }
    }
}