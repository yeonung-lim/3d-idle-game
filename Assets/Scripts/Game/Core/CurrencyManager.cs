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
                // DontDestroyOnLoad(gameObject); // Optional: if it should persist across scenes
                InitializeCurrency();
            }
            else
            {
                Debug.LogWarning("CurrencyManager: Another instance already exists. Destroying this one.");
                Destroy(gameObject);
            }
        }

        private void InitializeCurrency()
        {
            currentGold = 0; // Starting gold
            // Potentially load from save data here in the future
            Debug.Log("CurrencyManager initialized. Current gold: " + currentGold);
        }

        public int GetCurrentGold()
        {
            return currentGold;
        }

        public void AddGold(int amount)
        {
            if (amount < 0)
            {
                Debug.LogWarning("CurrencyManager: Cannot add a negative amount of gold. Use SpendGold instead.");
                return;
            }
            currentGold += amount;
            OnGoldChanged?.Invoke(currentGold);
            Debug.Log(amount + " gold added. Current gold: " + currentGold);
        }

        public bool SpendGold(int amount)
        {
            if (amount < 0)
            {
                Debug.LogWarning("CurrencyManager: Cannot spend a negative amount of gold.");
                return false;
            }

            if (currentGold >= amount)
            {
                currentGold -= amount;
                OnGoldChanged?.Invoke(currentGold);
                Debug.Log(amount + " gold spent. Current gold: " + currentGold);
                return true;
            }
            else
            {
                Debug.Log("Not enough gold to spend " + amount + ". Current gold: " + currentGold);
                return false;
            }
        }
    }
}
