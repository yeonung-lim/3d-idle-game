using UnityEngine;

namespace Game.Core
{
    public class CurrencyManager : MonoBehaviour
    {
        public static CurrencyManager Instance { get; private set; }

        private int currentGold;
        public event System.Action<int> OnGoldChanged;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                // DontDestroyOnLoad(gameObject); // 선택사항: 씬 전환시 유지하려면 활성화하세요
                InitializeCurrency();
            }
            else
            {
                Debug.LogWarning("CurrencyManager: 이미 인스턴스가 존재합니다. 현재 인스턴스를 제거합니다.");
                Destroy(gameObject);
            }
        }

        private void InitializeCurrency()
        {
            currentGold = 0; // 시작 골드
            // 향후 여기에 세이브 데이터로부터 로드하는 기능 추가 예정
            Debug.Log("CurrencyManager 초기화 완료. 현재 골드: " + currentGold);
        }

        public int GetCurrentGold()
        {
            return currentGold;
        }

        public void AddGold(int amount)
        {
            if (amount < 0)
            {
                Debug.LogWarning("CurrencyManager: 음수 값의 골드를 추가할 수 없습니다. 대신 SpendGold를 사용하세요.");
                return;
            }
            currentGold += amount;
            OnGoldChanged?.Invoke(currentGold);
            Debug.Log(amount + " 골드가 추가되었습니다. 현재 골드: " + currentGold);
        }

        public bool SpendGold(int amount)
        {
            if (amount < 0)
            {
                Debug.LogWarning("CurrencyManager: 음수 값의 골드를 소비할 수 없습니다.");
                return false;
            }

            if (currentGold >= amount)
            {
                currentGold -= amount;
                OnGoldChanged?.Invoke(currentGold);
                Debug.Log(amount + " 골드를 사용했습니다. 현재 골드: " + currentGold);
                return true;
            }
            else
            {
                Debug.Log(amount + " 골드를 사용하기에 골드가 부족합니다. 현재 골드: " + currentGold);
                return false;
            }
        }
    }
}
