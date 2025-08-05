using UnityEngine;

namespace ProjectIdle
{
    /// <summary>
    ///     UserCache는 사용자 데이터를 저장하는 스크립터블 오브젝트입니다
    /// </summary>
    [CreateAssetMenu(fileName = "UserData", menuName = "ProjectIdle/UserData", order = 1)]
    public class UserData : ScriptableObject
    {
        /// <summary>
        /// 뒤끝 데이터 ID
        /// </summary>
        public string inDate;
        
        /// <summary>
        /// 사용자의 골드
        /// </summary>
        public long gold;
        
        /// <summary>
        /// 사용자의 젬
        /// </summary>
        public long gem;

        /// <summary>
        /// 사용자의 공격력
        /// </summary>
        public int attack;
        
        /// <summary>
        /// 사용자의 공격력 레벨업 횟수
        /// </summary>
        public int attackLevelUpCount;
        
        /// <summary>
        /// 사용자의 최대체력
        /// </summary>
        public int maxHp;
        
        /// <summary>
        /// 사용자의 최대체력 레벨업 횟수
        /// </summary>
        public int maxHpLevelUpCount;
    }
}