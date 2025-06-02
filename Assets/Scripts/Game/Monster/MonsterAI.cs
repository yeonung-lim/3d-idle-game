using UnityEngine;

namespace Game.Monster
{
    public class MonsterAI : MonoBehaviour
    {
        void Start()
        {
            Debug.Log(gameObject.name + " AI가 초기화되었습니다. 현재 대기 상태입니다.");
        }

        // 향후 AI 로직이 여기에 추가될 예정입니다 (예: 행동을 위한 Update 메서드)
    }
}
