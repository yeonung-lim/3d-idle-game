using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro를 사용하는 경우
using System.Collections.Generic; // List<string>을 위한 업데이트
using System; // Action을 위한 것

public class AddressablesExample : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField keyInput; // TextMeshPro를 사용하지 않는 경우 InputField 사용
    public Button checkUpdatesButton;
    public Button loadAssetButton;
    public Button unloadAssetButton;
    public Button loadGroupButton;
    public Button unloadGroupButton;
    public TMP_Text statusText; // TextMeshPro를 사용하지 않는 경우 Text 사용
    public GameObject assetDisplayObject; // 선택사항: 씬에서 플레이스홀더 할당

    private GameObject _instantiatedAsset;
    private List<string> _catalogsToUpdate; // 업데이트가 필요한 카탈로그 저장

    void Start()
    {
        // AddressablesManager 인스턴스가 준비되었는지 확인
        if (AddressablesManager.Instance == null)
        {
            LogStatus("AddressablesManager가 초기화되지 않았습니다!");
            // 매니저를 사용할 수 없는 경우 UI 비활성화
            SetUIInteractable(false);
            return;
        }

        // 버튼 리스너 할당
        checkUpdatesButton.onClick.AddListener(HandleCheckUpdates);
        loadAssetButton.onClick.AddListener(HandleLoadAsset);
        unloadAssetButton.onClick.AddListener(HandleUnloadAsset);
        loadGroupButton.onClick.AddListener(HandleLoadGroup);
        unloadGroupButton.onClick.AddListener(HandleUnloadGroup);

        LogStatus("Addressables 예제가 초기화되었습니다. 에셋 키나 라벨을 입력하고 작업을 선택하세요.");
        SetUIInteractable(true); // 처음에는 업데이트 확인이 첫 번째 논리적 단계일 수 있음
    }

    void SetUIInteractable(bool isInteractable, bool keepUpdateEnabled = false)
    {
        loadAssetButton.interactable = isInteractable;
        unloadAssetButton.interactable = isInteractable;
        loadGroupButton.interactable = isInteractable;
        unloadGroupButton.interactable = isInteractable;
        keyInput.interactable = isInteractable;

        // 업데이트 확인 버튼은 다른 상호작용 상태를 가질 수 있음
        checkUpdatesButton.interactable = keepUpdateEnabled || isInteractable;
    }

    void LogStatus(string message)
    {
        Debug.Log($"[AddressablesExample] {message}");
        if (statusText != null)
        {
            statusText.text = message;
        }
    }

    void HandleCheckUpdates()
    {
        LogStatus("카탈로그 업데이트 확인 중...");
        SetUIInteractable(false, true); // 다른 버튼 비활성화, 업데이트 버튼은 활성화 유지

        AddressablesManager.Instance.CheckForCatalogUpdates(
            catalogs => // onUpdateAvailable
            {
                _catalogsToUpdate = catalogs;
                LogStatus($"업데이트 가능한 카탈로그: {string.Join(", ", catalogs)}. '카탈로그 업데이트'를 클릭하거나 다운로드를 진행하세요.");
                // 실제 시나리오에서는 여기서 "지금 업데이트" 버튼을 활성화할 수 있습니다.
                // 이 예제에서는 즉시 업데이트를 시도해보겠습니다.
                HandleUpdateCatalogs();
            },
            () => // onNoUpdateNeeded
            {
                LogStatus("카탈로그 업데이트가 필요하지 않습니다.");
                SetUIInteractable(true);
            }
        );
    }

    void HandleUpdateCatalogs()
    {
        if (_catalogsToUpdate == null || _catalogsToUpdate.Count == 0)
        {
            LogStatus("업데이트할 카탈로그가 없습니다.");
            SetUIInteractable(true);
            return;
        }

        LogStatus($"카탈로그 다운로드 시작: {string.Join(", ", _catalogsToUpdate)}");
        SetUIInteractable(false); // 업데이트 중 UI 비활성화

        AddressablesManager.Instance.UpdateCatalogs(
            _catalogsToUpdate,
            progress => // onProgress
            {
                LogStatus($"업데이트 진행률: {progress * 100:F2}%");
            },
            () => // onComplete
            {
                LogStatus("카탈로그 업데이트가 성공적으로 다운로드되고 적용되었습니다!");
                _catalogsToUpdate = null; // 목록 초기화
                SetUIInteractable(true);
            },
            error => // onError
            {
                LogStatus($"카탈로그 업데이트 실패: {error}");
                SetUIInteractable(true);
            }
        );
    }

    void HandleLoadAsset()
    {
        string key = keyInput.text;
        if (string.IsNullOrEmpty(key))
        {
            LogStatus("로드할 에셋 키를 입력해주세요.");
            return;
        }

        LogStatus($"에셋 로드 중 (키: {key})...");
        SetUIInteractable(false);

        // 이전에 인스턴스화된 에셋 제거
        if (_instantiatedAsset != null)
        {
            Destroy(_instantiatedAsset);
            _instantiatedAsset = null;
        }

        AddressablesManager.Instance.LoadAsset<GameObject>(
            key,
            loadedAsset => // onComplete
            {
                LogStatus($"에셋 '{key}' 로드 성공.");
                if (loadedAsset != null)
                {
                    // 로드된 에셋 인스턴스화
                    _instantiatedAsset = Instantiate(loadedAsset, assetDisplayObject != null ? assetDisplayObject.transform.position : Vector3.zero, Quaternion.identity);
                    _instantiatedAsset.name = loadedAsset.name + "_Instance";
                    LogStatus($"'{_instantiatedAsset.name}' 인스턴스화 완료.");

                    // 예시: GameObject 대신 Material을 로드한 경우
                    // if (assetDisplayObject != null && loadedAsset is Material newMat) {
                    //     Renderer renderer = assetDisplayObject.GetComponent<Renderer>();
                    //     if (renderer != null) renderer.material = newMat;
                    //     LogStatus($"'{newMat.name}' 머티리얼을 {assetDisplayObject.name}에 적용했습니다.");
                    // } else if (loadedAsset is Material) {
                    //     LogStatus("머티리얼을 로드했지만 적용할 디스플레이 오브젝트가 없습니다.");
                    // }
                }
                SetUIInteractable(true);
            },
            error => // onError
            {
                LogStatus($"에셋 '{key}' 로드 실패. 오류: {error}");
                SetUIInteractable(true);
            }
        );
    }

    void HandleUnloadAsset()
    {
        string key = keyInput.text;
        if (string.IsNullOrEmpty(key))
        {
            LogStatus("언로드할 에셋 키를 입력해주세요.");
            return;
        }

        LogStatus($"에셋 언로드 중: {key}...");

        // 키가 일치하는 특정 인스턴스화된 오브젝트 제거 (키가 이름과 일치하지 않을 경우 더 복잡한 로직이 필요할 수 있음)
        // 단순화를 위해 이 예제는 한 번에 하나의 인스턴스화된 에셋만 가정합니다.
        if (_instantiatedAsset != null)
        {
            // 더 견고한 시스템은 키별로 인스턴스화된 오브젝트를 추적할 것입니다.
            // 이 예제에서는 현재 키가 현재 오브젝트에 해당한다고 가정합니다.
            Destroy(_instantiatedAsset);
            _instantiatedAsset = null;
            LogStatus("인스턴스화된 에셋 인스턴스 제거됨.");
        }

        AddressablesManager.Instance.UnloadAsset(key);
        LogStatus($"에셋 '{key}' 언로드 호출 완료. 자세한 내용은 AddressablesManager 로그를 확인하세요.");
        SetUIInteractable(true);
    }

    void HandleLoadGroup()
    {
        string label = keyInput.text;
        if (string.IsNullOrEmpty(label))
        {
            LogStatus("그룹으로 로드할 에셋 라벨을 입력해주세요.");
            return;
        }

        LogStatus($"라벨이 있는 에셋 로드 중: {label}...");
        SetUIInteractable(false);

        // 그룹 로딩이 여러 개를 가져올 수 있으므로 이전에 인스턴스화된 에셋이 있다면 제거
        if (_instantiatedAsset != null)
        {
            Destroy(_instantiatedAsset);
            _instantiatedAsset = null;
        }
        // 참고: 이 예제는 단순하게 유지하기 위해 그룹 로드된 에셋을 인스턴스화하지 않습니다.
        // 그룹 로드에서 여러 게임 오브젝트를 관리하려면 더 복잡한 시스템이 필요합니다.

        AddressablesManager.Instance.LoadAssetsByLabel<GameObject>(
            label,
            asset => // onAssetLoaded (그룹의 각 에셋에 대해)
            {
                if (asset != null)
                {
                    LogStatus($"그룹 '{label}'의 일부로 에셋 '{asset.name}' 로드됨.");
                    // 예시: 각 에셋 인스턴스화 또는 처리
                    // 단순화를 위해 여기서는 로깅만 수행합니다.
                    // 인스턴스화하는 경우 이 오브젝트들을 리스트로 관리하세요.
                }
            },
            () => // onAllComplete
            {
                LogStatus($"라벨 '{label}'의 모든 에셋 로드 완료.");
                SetUIInteractable(true);
            },
            error => // onError
            {
                LogStatus($"라벨 '{label}'의 에셋 로드 실패. 오류: {error}");
                SetUIInteractable(true);
            }
        );
    }

    void HandleUnloadGroup()
    {
        string label = keyInput.text;
        if (string.IsNullOrEmpty(label))
        {
            LogStatus("언로드할 에셋 라벨을 입력해주세요.");
            return;
        }

        LogStatus($"라벨이 있는 에셋 언로드 중: {label}...");
        // 참고: HandleLoadGroup에서 에셋을 인스턴스화했다면 여기서 제거해야 합니다.
        // 이 예제는 로깅만 수행하므로 그룹 로딩에서 제거할 GameObjects가 없습니다.

        AddressablesManager.Instance.UnloadAssetsByLabel(label);
        LogStatus($"라벨 '{label}' 언로드 호출 완료. 자세한 내용은 AddressablesManager 로그를 확인하세요.");
        SetUIInteractable(true);
    }

    void OnDestroy()
    {
        // 버튼 리스너 정리
        if (checkUpdatesButton != null) checkUpdatesButton.onClick.RemoveAllListeners();
        if (loadAssetButton != null) loadAssetButton.onClick.RemoveAllListeners();
        if (unloadAssetButton != null) unloadAssetButton.onClick.RemoveAllListeners();
        if (loadGroupButton != null) loadGroupButton.onClick.RemoveAllListeners();
        if (unloadGroupButton != null) unloadGroupButton.onClick.RemoveAllListeners();

        // 남아있는 인스턴스화된 에셋 제거
        if (_instantiatedAsset != null)
        {
            Destroy(_instantiatedAsset);
        }
    }
}
