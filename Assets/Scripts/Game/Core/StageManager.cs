using UnityEngine;

namespace Game.Core
{
    public class StageManager : MonoBehaviour
    {
        public static StageManager Instance { get; private set; }

        public int currentStage { get; private set; }
        public event System.Action<int> OnStageChanged;

        public int monstersKilledThisStage { get; private set; }
        public int monstersPerStage = 10;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                // DontDestroyOnLoad(gameObject); // 선택사항: 씬 전환시 유지해야 하는 경우
            }
            else
            {
                Debug.LogWarning("스테이지 매니저: 이미 인스턴스가 존재합니다. 현재 인스턴스를 제거합니다.");
                Destroy(gameObject);
            }
        }

        void Start()
        {
            InitializeStage();
        }

        private void InitializeStage()
        {
            currentStage = 1;
            monstersKilledThisStage = 0;
            OnStageChanged?.Invoke(currentStage);
            Debug.Log("스테이지 매니저 초기화 완료. 현재 스테이지: " + currentStage);
        }

        public void AdvanceToNextStage()
        {
            currentStage++;
            monstersKilledThisStage = 0;
            OnStageChanged?.Invoke(currentStage);
            Debug.Log("다음 스테이지로 진행: " + currentStage);
            // 향후 개선사항:
            // - MonsterSpawner에 난이도 조정 알림 (예: 더 강한 몬스터 생성, 생성 간격 감소)
            // - 환경, 음악 등 변경
        }

        public void MonsterKilled()
        {
            if (currentStage == 0) // 초기화되지 않았거나 게임오버 상태인 경우
            {
                Debug.LogWarning("스테이지 매니저: 활성화된 스테이지가 없는 상태에서 MonsterKilled가 호출됨 (currentStage가 0).");
                return;
            }

            monstersKilledThisStage++;
            Debug.Log("몬스터 처치 진행상황. 현재 스테이지 처치수: " + monstersKilledThisStage + "/" + monstersPerStage);

            if (monstersKilledThisStage >= monstersPerStage)
            {
                AdvanceToNextStage();
            }
        }

        void Update()
        {
            // 디버그용 다음 스테이지 진행 키
            if (Input.GetKeyDown(KeyCode.N))
            {
                Debug.Log("디버그: 키 입력으로 다음 스테이지 진행.");
                AdvanceToNextStage();
            }
        }
    }
}
