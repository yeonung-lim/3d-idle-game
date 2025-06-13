using UnityEngine;
using MVVM.ViewModels; // Required for ExampleViewModel
using UnityEngine.UI; // Required for Button (if you add UI button handlers here)

namespace MVVM.Examples
{
    public class ExampleSceneController : MonoBehaviour
    {
        [Header("ViewModel Reference")]
        [Tooltip("Assign the GameObject that has the ExampleViewModel component.")]
        public ExampleViewModel exampleViewModel;

        [Header("Player Name Input (Optional)")]
        [Tooltip("Optional: Assign an InputField to change the player's name.")]
        public InputField playerNameInputField; // Assuming Unity UI InputField

        void Start()
        {
            if (exampleViewModel == null)
            {
                Debug.LogError("ExampleSceneController: ExampleViewModel is not assigned!", this);
                // Try to find it if not assigned, as a fallback
                exampleViewModel = FindObjectOfType<ExampleViewModel>();
                if (exampleViewModel == null)
                {
                    Debug.LogError("ExampleSceneController: Could not find ExampleViewModel in the scene!", this);
                    enabled = false; // Disable component if ViewModel is missing
                    return;
                }
                 Debug.LogWarning("ExampleSceneController: ExampleViewModel was found dynamically. It's better to assign it in the Inspector.", this);
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
                Debug.LogError("ViewModel not assigned to ExampleSceneController.", this);
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
                Debug.LogError("ViewModel not assigned to ExampleSceneController.", this);
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
                Debug.LogError("ViewModel not assigned to ExampleSceneController.", this);
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
                Debug.LogError("ViewModel not assigned to ExampleSceneController.", this);
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
                Debug.LogError("ViewModel not assigned to ExampleSceneController.", this);
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
