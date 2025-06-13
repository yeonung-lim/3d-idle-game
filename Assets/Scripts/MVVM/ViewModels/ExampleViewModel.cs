using MVVM.Core; // Ensures ViewModelBase is recognized
using UnityEngine; // For Debug.Log or other Unity-specific functionalities if needed

namespace MVVM.ViewModels
{
    public class ExampleViewModel : ViewModelBase
    {
        private string _playerName = "PlayerOne";
        public string PlayerName
        {
            get => _playerName;
            set
            {
                if (_playerName == value) return;
                _playerName = value;
                RaisePropertyChanged(); // Automatically uses "PlayerName" due to [CallerMemberName]
                Debug.Log("ExampleViewModel: PlayerName changed to " + _playerName);
            }
        }

        private int _health = 100;
        public int Health
        {
            get => _health;
            set
            {
                if (_health == value) return;
                _health = value;
                RaisePropertyChanged(nameof(Health)); // Explicitly passing property name also works
                Debug.Log("ExampleViewModel: Health changed to " + _health);
                // Example of a dependent property
                RaisePropertyChanged(nameof(StatusMessage));
            }
        }

        private bool _isAlive = true;
        public bool IsAlive
        {
            get => _isAlive;
            set
            {
                if (_isAlive == value) return;
                _isAlive = value;
                RaisePropertyChanged();
                Debug.Log("ExampleViewModel: IsAlive changed to " + _isAlive);
                RaisePropertyChanged(nameof(StatusMessage));
            }
        }

        // Example of a property that depends on other properties
        public string StatusMessage => $"Player: {PlayerName}, Health: {Health}, Alive: {IsAlive}";

        // Example of a public field (less common for MVVM properties, but supported by DataBinder)
        public float Mana = 75f;

        public void TakeDamage(int amount)
        {
            Health -= amount;
            if (Health <= 0)
            {
                Health = 0;
                IsAlive = false;
            }
        }

        public void Heal(int amount)
        {
            Health += amount;
            if (Health > 100) Health = 100;
            if (Health > 0 && !IsAlive) IsAlive = true;
        }

        public void SetMana(float newMana)
        {
            if (Mathf.Approximately(Mana, newMana)) return;
            Mana = newMana;
            RaisePropertyChanged(nameof(Mana)); // Important: Manually call for public fields if they need to notify
            Debug.Log("ExampleViewModel: Mana changed to " + Mana);
        }

        // It's good practice to call RaisePropertyChanged for all properties if an external event significantly changes state
        public void ResetState()
        {
            PlayerName = "DefaultPlayer";
            Health = 100;
            IsAlive = true;
            SetMana(50f);
            // No need to call RaisePropertyChanged(null) or for each, as setters do it.
            // However, if you were directly changing backing fields without going through setters:
            // _playerName = "DefaultPlayer"; _health = 100; _isAlive = true; Mana = 50f;
            // RaisePropertyChanged(null); // This would indicate all properties might have changed.
        }
    }
}
