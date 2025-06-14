﻿using System;
using UnityEngine;

namespace Sound
{
    /// <summary>
    ///     BGM 오디오 타입입니다.
    /// </summary>
    public enum BGMAudioType
    {
        None = -1,
        TitleMusic // 메뉴 화면 BGM
    }

    /// <summary>
    ///     BGM 오디오 정보입니다.
    /// </summary>
    [Serializable]
    public class BGMAudioInfo
    {
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
    }

    /// <summary>
    ///     BGM 오디오 클립 데이터 클래스
    /// </summary>
    [CreateAssetMenu(fileName = "BGMAudioClips", menuName = "ScriptableObjects/BGMAudioClips")]
    public class BGMAudioClips : ScriptableObject
    {
        /// <summary>
        ///     BGM 오디오 클립들을 저장하는 딕셔너리입니다.
        /// </summary>
        [SerializeField] private SerializableDictionary<BGMAudioType, BGMAudioInfo> audioClipsDict;

        /// <summary>
        ///     BGM 오디오 정보를 반환합니다.
        /// </summary>
        /// <param name="audioType">BGM 오디오 타입</param>
        /// <returns>BGM 오디오 정보</returns>
        public BGMAudioInfo GetAudioInfo(BGMAudioType audioType)
        {
            if (!audioClipsDict.TryGetValue(audioType, out var audioInfo))
                Debug.LogError($"{audioType} 오디오 클립이 누락되었습니다!");

            return audioInfo;
        }
    }
}