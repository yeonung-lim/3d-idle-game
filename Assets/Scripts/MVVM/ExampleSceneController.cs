using UnityEngine;
using MVVM.ViewModels; // Required for ExampleViewModel
using UnityEngine.UI; // Required for Button (if you add UI button handlers here)

namespace MVVM.Examples
{
    public class ExampleSceneController : MonoBehaviour
    {
        [Header("ViewModel Reference")]
        [Tooltip("ExampleViewModel 컴포넌트가 있는 GameObject를 할당하세요.")]
        public ExampleViewModel exampleViewModel;

        [Header("Player Name Input (Optional)")]
        [Tooltip("선택사항: 플레이어 이름을 변경할 InputField를 할당하세요.")]
        public InputField playerNameInputField; // Assuming Unity UI InputField

        void Start()
        {
            if (exampleViewModel == null)
            {
                Debug.LogError("ExampleSceneController: ExampleViewModel이 할당되지 않았습니다!", this);
                // Try to find it if not assigned, as a fallback
                exampleViewModel = FindObjectOfType<ExampleViewModel>();
                if (exampleViewModel == null)
                {
                    Debug.LogError("ExampleSceneController: 씬에서 ExampleViewModel을 찾을 수 없습니다!", this);
                    enabled = false; // Disable component if ViewModel is missing
                    return;
                }
                 Debug.LogWarning("ExampleSceneController: ExampleViewModel이 동적으로 찾아졌습니다. 인스펙터에서 할당하는 것이 더 좋습니다.", this);
            }

            if (playerNameInputField != null)
            {
                playerNameInputField.onEndEdit.AddListener(OnPlayerNameChanged);
                // Initialize input field with current ViewModel name
                playerNameInputField.text = exampleViewModel.PlayerName;
            }
        }

        // --- Public methods to be called by UI Buttons ---

        public void ApplyDamageToViewModel()
        {
            if (exampleViewModel != null)
            {
                exampleViewModel.TakeDamage(10);
            }
            else
            {
                Debug.LogError("ExampleSceneController에 ViewModel이 할당되지 않았습니다.", this);
            }
        }

        public void HealViewModel()
        {
            if (exampleViewModel != null)
            {
                exampleViewModel.Heal(15);
            }
            else
            {
                Debug.LogError("ExampleSceneController에 ViewModel이 할당되지 않았습니다.", this);
            }
        }

        public void ChangePlayerNameToRandom()
        {
            if (exampleViewModel != null)
            {
                string[] names = { "Gandalf", "Frodo", "Aragorn", "Legolas", "Gimli" };
                exampleViewModel.PlayerName = names[Random.Range(0, names.Length)];
                if (playerNameInputField != null)
                {
                    playerNameInputField.text = exampleViewModel.PlayerName; // Update input field if linked
                }
            }
            else
            {
                Debug.LogError("ExampleSceneController에 ViewModel이 할당되지 않았습니다.", this);
            }
        }

        public void SetManaToRandom()
        {
            if (exampleViewModel != null)
            {
                exampleViewModel.SetMana(Random.Range(0f, 100f));
            }
            else
            {
                Debug.LogError("ExampleSceneController에 ViewModel이 할당되지 않았습니다.", this);
            }
        }

        public void ResetViewModelState()
        {
            if (exampleViewModel != null)
            {
                exampleViewModel.ResetState();
                 if (playerNameInputField != null)
                {
                    playerNameInputField.text = exampleViewModel.PlayerName; // Update input field if linked
                }
            }
            else
            {
                Debug.LogError("ExampleSceneController에 ViewModel이 할당되지 않았습니다.", this);
            }
        }

        // --- InputField Listener ---
        private void OnPlayerNameChanged(string newName)
        {
            if (exampleViewModel != null)
            {
                exampleViewModel.PlayerName = newName;
            }
        }

        void OnDestroy()
        {
            if (playerNameInputField != null)
            {
                playerNameInputField.onEndEdit.RemoveListener(OnPlayerNameChanged);
            }
        }
    }
}
