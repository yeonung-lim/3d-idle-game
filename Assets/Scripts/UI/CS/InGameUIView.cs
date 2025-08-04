using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UIElements;

namespace UI.Views
{
    public class InGameUIView : MonoBehaviour
    {
        private VisualElement _rootVisualElement;
        private Button _attackLevelUpButton;
        private Button _healthLevelUpButton;
        
        private void Awake()
        {
            // UI Toolkit의 루트 요소를 가져옵니다.
            _rootVisualElement = GetComponent<UIDocument>().rootVisualElement;

            // UI 요소들을 찾습니다.
            _attackLevelUpButton = _rootVisualElement.Q<Button>("attack-level-up-button");
            _healthLevelUpButton = _rootVisualElement.Q<Button>("health-level-up-button");

            // 버튼 클릭 이벤트 핸들러를 등록합니다.
            if (_attackLevelUpButton != null)
            {
                _attackLevelUpButton.clicked += OnAttackLevelUpClicked;
            }

            if (_healthLevelUpButton != null)
            {
                 _healthLevelUpButton.clicked += OnHealthLevelUpClicked;
            }
        }

        private void OnHealthLevelUpClicked()
        {
            // Health 레벨업 로직을 여기에 구현합니다.
            Debug.Log("Health Level Up Clicked");
        }

        private void OnAttackLevelUpClicked()
        {
            // Attack 레벨업 로직을 여기에 구현합니다.
            Debug.Log("Attack Level Up Clicked");
        }
    }
}