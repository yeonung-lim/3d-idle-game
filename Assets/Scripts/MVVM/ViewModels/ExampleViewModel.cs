using MVVM.Core; // ViewModelBase 인식 보장
using UnityEngine; // Debug.Log나 다른 Unity 관련 기능이 필요한 경우 사용

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
                RaisePropertyChanged(); // [CallerMemberName] 덕분에 자동으로 "PlayerName" 사용
                Debug.Log("ExampleViewModel: 플레이어 이름이 " + _playerName + "로 변경됨");
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
                RaisePropertyChanged(nameof(Health)); // 명시적으로 프로퍼티 이름을 전달하는 것도 가능
                Debug.Log("ExampleViewModel: 체력이 " + _health + "로 변경됨");
                // 종속 프로퍼티 예시
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
                Debug.Log("ExampleViewModel: 생존 상태가 " + _isAlive + "로 변경됨");
                RaisePropertyChanged(nameof(StatusMessage));
            }
        }

        // 다른 프로퍼티들에 의존하는 프로퍼티 예시
        public string StatusMessage => $"Player: {PlayerName}, Health: {Health}, Alive: {IsAlive}";

        // 공개 필드 예시 (MVVM 프로퍼티로는 덜 일반적이지만 DataBinder에서 지원)
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
            RaisePropertyChanged(nameof(Mana)); // 중요: 공개 필드의 경우 알림이 필요하면 수동으로 호출해야 함
            Debug.Log("ExampleViewModel: 마나가 " + Mana + "로 변경됨");
        }

        // 외부 이벤트로 인해 상태가 크게 변경될 때는 모든 프로퍼티에 대해 RaisePropertyChanged를 호출하는 것이 좋은 관행
        public void ResetState()
        {
            PlayerName = "DefaultPlayer";
            Health = 100;
            IsAlive = true;
            SetMana(50f);
            // 각각의 setter가 이미 호출하므로 RaisePropertyChanged(null)이나 각각에 대해 호출할 필요가 없음
            // 하지만 backing 필드를 직접 변경하는 경우:
            // _playerName = "DefaultPlayer"; _health = 100; _isAlive = true; Mana = 50f;
            // RaisePropertyChanged(null); // 이는 모든 프로퍼티가 변경되었을 수 있음을 나타냄
        }
    }
}
