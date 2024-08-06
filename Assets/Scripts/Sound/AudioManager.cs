using System;
using Ads;
using AsyncInitialize;
using Hellmade.Sound;
using UnityCommunity.UnitySingleton;
using UnityEngine;

namespace Sound
{
    /// <summary>
    ///     사운드를 관리하는 클래스입니다.
    /// </summary>
    public class AudioManager : PersistentMonoSingleton<AudioManager>, IResettableAudioController
    {
        /// <summary>
        ///     UI 사운드 재생 간격 정보입니다.
        /// </summary>
        [SerializeField] private SoundIntervalInfo uiSoundInterval = new();

        /// <summary>
        ///     BGM 사운드 클립 정보입니다.
        /// </summary>
        private BGMAudioClips _bgmAudioClips;

        /// <summary>
        ///     현재 재생 중인 BGM의 타입입니다.
        /// </summary>
        private BGMAudioType _bgmAudioType;

        /// <summary>
        ///     현재 재생 중인 BGM의 오디오 인덱스
        /// </summary>
        private int _bgmSource = -1;

        /// <summary>
        ///     재생중이던 BGM의 시간입니다.
        /// </summary>
        private float _bgmTime;

        /// <summary>
        ///     초기화 여부입니다.
        /// </summary>
        private bool _isInitialized;

        /// <summary>
        ///     UI 사운드 클립 정보입니다.
        /// </summary>
        private UIAudioClips _uiAudioClips;

        internal float GlobalBGMVolume
        {
            get => PlayerPrefs.GetFloat("GlobalBGMVolume", 1f);
            set => PlayerPrefs.SetFloat("GlobalBGMVolume", value);
        }

        internal float GlobalSFXVolume
        {
            get => PlayerPrefs.GetFloat("GlobalSFXVolume", 1f);
            set => PlayerPrefs.SetFloat("GlobalSFXVolume", value);
        }

        /// <summary>
        ///     초기화를 재설정합니다.
        /// </summary>
        public void Reset()
        {
            _isInitialized = false;
        }

        private void Start()
        {
            _uiAudioClips = Resources.Load<UIAudioClips>("UIAudioClips");
            _bgmAudioClips = Resources.Load<BGMAudioClips>("BGMAudioClips");
        }

        /// <summary>
        ///     리셋을 하기전 준비합니다.
        /// </summary>
        public void ResetPrepare()
        {
            if (_isInitialized == false) return;
            if (_bgmSource == -1) return;

            var musicAudio = EazySoundManager.GetMusicAudio(_bgmSource);
            _bgmTime = musicAudio.AudioSource.time;
        }

        /// <summary>
        ///     사운드를 초기화합니다.
        /// </summary>
        public void ResetAudio()
        {
            if (_isInitialized == false) return;

            var audioType = BGMAudioType.TitleMusic;
            var audioTime = 0f;

            if (_bgmSource != -1)
            {
                audioType = _bgmAudioType;
                audioTime = _bgmTime;
            }

            StopMusic();

            AudioSettings.Reset(AudioSettings.GetConfiguration());

            var audioID = PlayMusic(audioType, true);
            var musicAudio = EazySoundManager.GetMusicAudio(audioID);
            musicAudio.AudioSource.time = audioTime;
        }

        /// <summary>
        ///     비동기 작업을 반환합니다.
        /// </summary>
        /// <returns></returns>
        public CustomizableAsyncOperation GetAsyncOperation()
        {
            return CustomizableAsyncOperation.Create(() => _isInitialized);
        }

        /// <summary>
        ///     사운드 관련 초기화 작업을 시작합니다.
        /// </summary>
        public void StartProcess()
        {
            EazySoundManager.GlobalMusicVolume = GlobalBGMVolume;
            EazySoundManager.GlobalUISoundsVolume = GlobalSFXVolume;

            PlayMusic(BGMAudioType.TitleMusic, true);

            AdManager.Instance.SetAudioController(this);

            _isInitialized = true;
        }

        /// <summary>
        ///     BGM과 SFX 전역 볼륨을 동시에 설정합니다.
        /// </summary>
        /// <param name="volume"></param>
        public void SetGlobalVolume(float volume)
        {
            SetGlobalBGMVolume(volume);
            SetGlobalSfxVolume(volume);
        }

        /// <summary>
        ///     전역 BGM 볼륨을 설정합니다.
        /// </summary>
        /// <param name="volume"></param>
        public void SetGlobalBGMVolume(float volume)
        {
            GlobalBGMVolume = volume;
            EazySoundManager.GlobalMusicVolume = volume;
        }

        /// <summary>
        ///     전역 SFX 볼륨을 설정합니다.
        /// </summary>
        /// <param name="volume"></param>
        public void SetGlobalSfxVolume(float volume)
        {
            GlobalSFXVolume = volume;
            EazySoundManager.GlobalUISoundsVolume = volume;
        }

        /// <summary>
        ///     효과음을 재생합니다.
        /// </summary>
        /// <param name="soundType">재생할 효과음 타입</param>
        /// <param name="minSoundInterval">최소 사운드 재생 간격</param>
        /// <returns>사운드 인덱스</returns>
        public int PlaySfx(UIAudioType soundType, float? minSoundInterval = null)
        {
            var interval = minSoundInterval ?? uiSoundInterval.minSoundInterval;
            if (Time.unscaledTime - uiSoundInterval.lastSoundPlayTime < interval)
            {
                Debug.LogWarning($"[SFX] {soundType}이 너무 빠르게 재생되었습니다.");
                return -1;
            }

            if (soundType == UIAudioType.None)
                return -1;

            var audioInfo = _uiAudioClips.GetAudioInfo(soundType);
            if (audioInfo == null) return -1;

            uiSoundInterval.lastSoundPlayTime = Time.time;

            Debug.Log($"Play SFX: {soundType}");
            return EazySoundManager.PlayUISound(audioInfo.clip, audioInfo.volume);
        }

        /// <summary>
        ///     효과음의 재생 시간을 반환합니다.
        /// </summary>
        /// <param name="type">효과음 타입</param>
        /// <returns>재생 시간</returns>
        public float GetSfxAudioDuration(UIAudioType type)
        {
            var clip = _uiAudioClips.GetAudioInfo(type)?.clip;
            if (clip == null) return 0f;

            return clip.length;
        }

        /// <summary>
        ///     효과음을 중지합니다.
        /// </summary>
        /// <param name="soundId">사운드 인덱스</param>
        public void StopSfx(int soundId)
        {
            EazySoundManager.GetUISoundAudio(soundId).Stop();
        }

        /// <summary>
        ///     효과음을 중지합니다.
        /// </summary>
        /// <param name="type">효과음 타입</param>
        public void StopSfx(UIAudioType type)
        {
            var clip = _uiAudioClips.GetAudioInfo(type)?.clip;
            if (clip == null) return;

            EazySoundManager.GetUISoundAudio(clip).Stop();
        }

        /// <summary>
        ///     BGM을 중지합니다.
        /// </summary>
        public void StopMusic()
        {
            if (_bgmSource == -1) return;

            EazySoundManager.StopAllMusic();
            _bgmSource = -1;
            _bgmTime = 0f;
            _bgmAudioType = BGMAudioType.None;
        }

        /// <summary>
        ///     BGM을 재생합니다.
        /// </summary>
        /// <param name="soundType">BGM 타입</param>
        /// <param name="persist">씬 전환 시 계속 재생할지 여부</param>
        /// <returns>사운드 인덱스</returns>
        public int PlayMusic(BGMAudioType soundType, bool persist)
        {
            var audioInfo = _bgmAudioClips.GetAudioInfo(soundType);
            if (audioInfo == null) return -1;

            if (_bgmSource != -1)
            {
                var musicAudio = EazySoundManager.GetMusicAudio(_bgmSource);
                if (musicAudio.Clip == audioInfo.clip) return _bgmSource;
            }

            _bgmSource = EazySoundManager.PlayMusic(audioInfo.clip, audioInfo.volume, true, persist);
            _bgmAudioType = soundType;

            return _bgmSource;
        }

        /// <summary>
        ///     효과음의 최소 재생 간격 정보 클래스입니다.
        /// </summary>
        [Serializable]
        private class SoundIntervalInfo
        {
            [SerializeField] [Min(0f)] public float minSoundInterval = 0.075f;
            public float lastSoundPlayTime;
        }
    }
}