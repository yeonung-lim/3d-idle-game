using System;
using UnityEngine;

namespace Sound
{
    /// <summary>
    ///     UI 오디오 타입입니다.
    /// </summary>
    public enum UIAudioType
    {
        None = -1,
        ButtonClick = 0, // 기본 버튼 효과음
        BeginStickerDrag = 1, // 스티커 드래그 시작
        AnswerCorrect = 2, // 정답
        AnswerWrong = 3, // 오답,
        BeginStick = 4, // 스티커 붙이기 시작,
        FillColorBg = 5, // 배경 색칠하기
        Congratulation = 6, // 축하
        MiniGameFail = 7 // 미니게임 실패
    }

    /// <summary>
    ///     UI 오디오 정보입니다.
    /// </summary>
    [Serializable]
    public class UIAudioInfo
    {
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
    }

    /// <summary>
    ///     UI 오디오 클립 데이터 클래스
    /// </summary>
    [CreateAssetMenu(fileName = "UIAudioClips", menuName = "ScriptableObjects/UIAudioClips")]
    public class UIAudioClips : ScriptableObject
    {
        /// <summary>
        ///     UI 오디오 클립들을 저장하는 딕셔너리입니다.
        /// </summary>
        [SerializeField] private SerializableDictionary<UIAudioType, UIAudioInfo> audioClipsDict;

        /// <summary>
        ///     UI 오디오 정보를 반환합니다.
        /// </summary>
        /// <param name="audioType">UI 오디오 타입</param>
        /// <returns>UI 오디오 정보</returns>
        public UIAudioInfo GetAudioInfo(UIAudioType audioType)
        {
            if (audioType == UIAudioType.None)
                return null;
            if (!audioClipsDict.TryGetValue(audioType, out var audioInfo))
                Debug.LogError($"{audioType} 오디오 클립이 누락되었습니다!");

            return audioInfo;
        }
    }
}