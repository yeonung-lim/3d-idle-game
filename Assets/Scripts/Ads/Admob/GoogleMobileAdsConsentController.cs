using System;
using GoogleMobileAds.Ump.Api;
using UnityEngine;
using UnityEngine.UI;

namespace GoogleMobileAds.Samples
{
    /// <summary>
    ///     Google 사용자 메시징 플랫폼(UMP) SDK를 사용하여 동의를 구현하는 헬퍼 클래스입니다.
    /// </summary>
    public class GoogleMobileAdsConsentController : MonoBehaviour
    {
        [SerializeField] [Tooltip("사용자 동의 및 개인정보 보호 설정을 표시하는 버튼입니다.")]
        private Button _privacyButton;

        [SerializeField] [Tooltip("게임 오브젝트에 오류 팝업이 표시됩니다.")]
        private GameObject _errorPopup;

        [SerializeField] [Tooltip("오류 팝업의 오류 메시지입니다,")]
        private Text _errorText;

        /// <summary>
        ///     true이면 MobileAds.Initialize()를 호출하고 광고를 로드해도 안전합니다.
        /// </summary>
        public bool CanRequestAds => ConsentInformation.CanRequestAds();

        private void Start()
        {
            // 오류 팝업을 비활성화합니다,
            if (_errorPopup != null) _errorPopup.SetActive(false);
        }

        /// <summary>
        ///     구글 사용자 메시징 플랫폼(UMP) SDK의 시작 방법
        ///     필요한 모든 로딩을 포함하여 모든 시작 로직을 실행합니다.
        ///     업데이트를 로드하고 필요한 양식을 표시하는 등 모든 시작 로직을 실행합니다.
        /// </summary>
        public void GatherConsent(Action<string> onComplete)
        {
            Debug.Log("동의 수집");

            var requestParameters = new ConsentRequestParameters
            {
                // False는 사용자가 미성년자가 아님을 의미합니다.
                TagForUnderAgeOfConsent = false,
                ConsentDebugSettings = new ConsentDebugSettings
                {
                    // 지역별로 동의 설정을 디버깅하는 경우.
                    DebugGeography = DebugGeography.Disabled,
                    TestDeviceHashedIds = GoogleMobileAdsController.TestDeviceIds
                }
            };

            // 콜백을 오류 팝업 핸들러와 결합합니다.
            onComplete = onComplete == null
                ? UpdateErrorPopup
                : onComplete + UpdateErrorPopup;

            // Google 모바일 광고 SDK는 사용자 메시징 플랫폼(Google의
            // IAB 인증 동의 관리 플랫폼)을 하나의 솔루션으로 제공함으로써
            // 동의를 수집하는 하나의 솔루션을 제공합니다. 이것은 예시이며
            // 다른 동의 관리 플랫폼을 선택하여 동의를 수집할 수 있습니다.
            ConsentInformation.Update(requestParameters, updateError =>
            {
                // 개인정보 설정 변경 버튼을 활성화합니다.
                UpdatePrivacyButton();

                if (updateError != null)
                {
                    onComplete(updateError.Message);
                    return;
                }

                // 동의 상태에 따라 취할 동의 관련 조치를 결정합니다.
                if (CanRequestAds)
                {
                    // 동의가 이미 수집되었거나 필요하지 않습니다.
                    // 사용자에게 제어권을 다시 돌려줍니다.
                    onComplete(null);
                    return;
                }

                // 동의를 얻지 못했으며 동의를 받아야 합니다.
                // 사용자에 대한 초기 동의 요청 양식을 로드합니다.
                ConsentForm.LoadAndShowConsentFormIfRequired(showError =>
                {
                    UpdatePrivacyButton();
                    if (showError != null)
                    {
                        // 양식 표시가 실패했습니다.
                        if (onComplete != null) onComplete(showError.Message);
                    }
                    // 양식 표시가 성공했습니다.
                    else if (onComplete != null)
                    {
                        onComplete(null);
                    }
                });
            });
        }

        /// <summary>
        ///     사용자에게 개인정보 보호 옵션 양식을 표시합니다.
        /// </summary>
        /// <remarks>
        ///     앱에서 사용자가 언제든지 동의 상태를 변경할 수 있도록 허용해야 합니다.
        ///     사용자가 동의 상태를 변경할 수 있도록 다른 양식을 로드하고 저장합니다.
        /// </remarks>
        public void ShowPrivacyOptionsForm(Action<string> onComplete)
        {
            Debug.Log("Showing privacy options form.");

            // 콜백을 오류 팝업 핸들러와 결합합니다.
            onComplete = onComplete == null
                ? UpdateErrorPopup
                : onComplete + UpdateErrorPopup;

            ConsentForm.ShowPrivacyOptionsForm(showError =>
            {
                UpdatePrivacyButton();
                if (showError != null)
                    // 양식 표시가 실패했습니다.
                    onComplete?.Invoke(showError.Message);
                // 양식 표시가 성공했습니다.
                else
                    onComplete?.Invoke(null);
            });
        }

        /// <summary>
        ///     사용자에 대한 동의 정보 재설정.
        /// </summary>
        public void ResetConsentInformation()
        {
            ConsentInformation.Reset();
            UpdatePrivacyButton();
        }

        private void UpdatePrivacyButton()
        {
            if (_privacyButton != null)
                _privacyButton.interactable =
                    ConsentInformation.PrivacyOptionsRequirementStatus ==
                    PrivacyOptionsRequirementStatus.Required;
        }

        private void UpdateErrorPopup(string message)
        {
            if (string.IsNullOrEmpty(message)) return;

            if (_errorText != null) _errorText.text = message;

            if (_errorPopup != null) _errorPopup.SetActive(true);
            if (_privacyButton != null) _privacyButton.interactable = true;
        }
    }
}