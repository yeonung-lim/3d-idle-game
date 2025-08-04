using UnityEngine;

namespace ProjectIdle
{
    /// <summary>
    ///     UserCache는 사용자 데이터를 저장하는 스크립터블 오브젝트입니다
    ///     이 클래스는 사용자 설정이나 상태를 저장하는 데 사용됩니다.
    /// </summary>
    [CreateAssetMenu(fileName = "UserCache", menuName = "ProjectIdle/UserCache", order = 1)]
    public class UserCache : ScriptableObject
    {
        public long gold; // 사용자의 골드
        public long diamond; // 사용자의 다이아몬드
        public int level; // 사용자의 레벨
        public int exp; // 사용자의 경험치
    }
}