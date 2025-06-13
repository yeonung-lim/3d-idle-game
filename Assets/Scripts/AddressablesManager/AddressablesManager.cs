using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections.Generic;
using System;
using UnityEngine.AddressableAssets.ResourceLocators;

public class AddressablesManager : MonoBehaviour
{
    // 싱글톤 접근을 위한 정적 인스턴스
    private static AddressablesManager _instance;
    public static AddressablesManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("AddressablesManager");
                _instance = go.AddComponent<AddressablesManager>();
                DontDestroyOnLoad(go); // 씬 전환 시에도 매니저가 유지되도록 설정
            }
            return _instance;
        }
    }

    // --- 핸들 관리 ---
    // 개별적으로 로드된 에셋의 핸들을 저장합니다. 키는 Addressable 키입니다.
    private Dictionary<string, AsyncOperationHandle> _loadedAssetsHandles = new Dictionary<string, AsyncOperationHandle>();
    // 라벨로 로드된 에셋의 핸들을 저장합니다. 키는 라벨입니다.
    // 여기에 저장된 핸들은 LoadAssetsAsync의 메인 핸들로, 해당 라벨의 모든 서브 에셋을 관리합니다.
    private Dictionary<string, AsyncOperationHandle> _loadedGroupHandles = new Dictionary<string, AsyncOperationHandle>();

    // 에셋 로딩 큐를 위한 플레이스홀더 - 필요한 경우 순차적 로딩에 사용할 수 있습니다
    // private Queue<string> _loadQueue = new Queue<string>();

    // 업데이트 체크 로직을 위한 플레이스홀더
    // private bool _isCheckingForUpdates = false;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject); // 중복 인스턴스 제거
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject); // 씬 전환 시에도 매니저가 유지되도록 설정
    }

    // --- 에셋 로딩 ---
    /// <summary>
    /// 키를 통해 단일 에셋을 로드합니다.
    /// </summary>
    /// <typeparam name="T">로드할 에셋의 타입입니다.</typeparam>
    /// <param name="key">에셋의 Addressable 키입니다.</param>
    /// <param name="onComplete">에셋이 성공적으로 로드되었을 때 호출되는 콜백입니다.</param>
    /// <param name="onError">로딩 중 오류가 발생했을 때 호출되는 콜백입니다.</param>
    public void LoadAsset<T>(string key, Action<T> onComplete, Action<string> onError) where T : UnityEngine.Object
    {
        if (string.IsNullOrEmpty(key))
        {
            onError?.Invoke("[AddressablesManager] 에셋 로드 실패: 키가 비어있거나 null일 수 없습니다.");
            return;
        }

        if (_loadedAssetsHandles.TryGetValue(key, out AsyncOperationHandle existingHandle))
        {
            if (existingHandle.IsDone && existingHandle.Status == AsyncOperationStatus.Succeeded)
            {
                Debug.Log($"[AddressablesManager] 에셋 '{key}'가 이미 로드되어 있습니다. 캐시된 에셋을 반환합니다.");
                onComplete?.Invoke(existingHandle.Result as T);
                return;
            }
            else if (!existingHandle.IsDone)
            {
                // 에셋이 이미 로딩 중입니다
                Debug.Log($"[AddressablesManager] 에셋 '{key}'가 이미 로딩 중입니다. 기존 작업에 연결합니다.");
                existingHandle.Completed += handle =>
                {
                    if (handle.Status == AsyncOperationStatus.Succeeded)
                        onComplete?.Invoke(handle.Result as T);
                    else
                        onError?.Invoke($"[AddressablesManager] 에셋 '{key}' 로드 실패. 오류: {handle.OperationException}");
                };
                return;
            }
            // 핸들이 있었지만 실패했거나 유효하지 않았습니다. 다시 로드 시도
            Debug.LogWarning($"[AddressablesManager] 에셋 '{key}'의 이전 핸들이 실패했거나 유효하지 않았습니다. 다시 로드 시도합니다.");
            Addressables.Release(existingHandle); // 다시 시도하기 전에 이전 핸들 해제
            _loadedAssetsHandles.Remove(key);
        }

        Debug.Log($"[AddressablesManager] 키로 에셋 로드 중: {key}");
        AsyncOperationHandle<T> loadHandle = Addressables.LoadAssetAsync<T>(key);
        _loadedAssetsHandles[key] = loadHandle; // 즉시 핸들 저장

        loadHandle.Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                Debug.Log($"[AddressablesManager] 에셋 '{key}' 로드 성공.");
                onComplete?.Invoke(handle.Result);
            }
            else
            {
                string errorMsg = $"[AddressablesManager] 에셋 '{key}' 로드 실패. 오류: {handle.OperationException}";
                Debug.LogError(errorMsg);
                onError?.Invoke(errorMsg);
                // 실패한 핸들 제거
                if (_loadedAssetsHandles.ContainsKey(key) && _loadedAssetsHandles[key].Equals(handle))
                     _loadedAssetsHandles.Remove(key);
            }
        };
    }

    /// <summary>
    /// 공통 라벨을 공유하는 여러 에셋을 로드합니다.
    /// onAssetLoaded 콜백은 각 에셋이 로드될 때마다 호출됩니다.
    /// onAllComplete 콜백은 라벨의 모든 에셋이 로드되었을 때 한 번 호출됩니다.
    /// </summary>
    /// <typeparam name="T">로드할 에셋의 타입입니다.</typeparam>
    /// <param name="label">Addressable 라벨입니다.</param>
    /// <param name="onAssetLoaded">이 라벨 아래 로드된 각 개별 에셋에 대해 호출되는 콜백입니다.</param>
    /// <param name="onAllComplete">라벨의 모든 에셋이 처리되었을 때 호출되는 콜백입니다.</param>
    /// <param name="onError">라벨 로딩 작업 중 일반 오류가 발생했을 때 호출되는 콜백입니다.</param>
    public void LoadAssetsByLabel<T>(string label, Action<T> onAssetLoaded, Action onAllComplete, Action<string> onError) where T : UnityEngine.Object
    {
        if (string.IsNullOrEmpty(label))
        {
            onError?.Invoke("[AddressablesManager] 라벨로 에셋 로드 실패: 라벨이 비어있거나 null일 수 없습니다.");
            return;
        }

        if (_loadedGroupHandles.ContainsKey(label))
        {
            // 실행 중인 기존 작업에 다시 연결할 수 있지만,
            // 단순화를 위해 경고를 표시하고 반환하거나 원하는 경우 다시 로드를 허용합니다.
            Debug.LogWarning($"[AddressablesManager] 라벨 '{label}'의 에셋이 이미 로드되었거나 로딩 중입니다. 다시 로드가 필요한 경우 먼저 언로드하세요.");
            return;
        }

        Debug.Log($"[AddressablesManager] 라벨로 에셋 로드 중: {label}");
        AsyncOperationHandle<IList<T>> groupLoadHandle = Addressables.LoadAssetsAsync<T>(label, asset =>
        {
            Debug.Log($"[AddressablesManager] 라벨 '{label}'을 통해 로드된 {typeof(T).Name} 타입의 에셋: {asset}");
            onAssetLoaded?.Invoke(asset);
        });

        _loadedGroupHandles[label] = groupLoadHandle; // 메인 그룹 작업 핸들 저장

        groupLoadHandle.Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                Debug.Log($"[AddressablesManager] 라벨 '{label}'의 모든 에셋 로드 성공. 총 개수: {handle.Result.Count}");
                onAllComplete?.Invoke();
            }
            else
            {
                string errorMsg = $"[AddressablesManager] 라벨 '{label}'의 에셋 로드 실패. 오류: {handle.OperationException}";
                Debug.LogError(errorMsg);
                onError?.Invoke(errorMsg);
                // 실패한 핸들 제거
                if (_loadedGroupHandles.ContainsKey(label) && _loadedGroupHandles[label].Equals(handle))
                    _loadedGroupHandles.Remove(label);
            }
        };
    }

    // --- 에셋 언로딩 ---
    // 참고: 더 정교한 자동 해제(예: 참조 카운팅 또는 GameObject 생명주기에 연결)
    // 는 나중에 필요하다면 추가할 수 있습니다. 현재는 언로딩이 명시적입니다.

    /// <summary>
    /// 키로 식별된 단일 에셋을 언로드합니다.
    /// </summary>
    /// <param name="key">언로드할 에셋의 Addressable 키입니다.</param>
    public void UnloadAsset(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            Debug.LogWarning("[AddressablesManager] 에셋 언로드 실패: 키가 비어있거나 null일 수 없습니다.");
            return;
        }

        if (_loadedAssetsHandles.TryGetValue(key, out AsyncOperationHandle handleToRelease))
        {
            Debug.Log($"[AddressablesManager] 에셋 언로드 중: {key}");
            Addressables.Release(handleToRelease);
            _loadedAssetsHandles.Remove(key);
        }
        else
        {
            Debug.LogWarning($"[AddressablesManager] 에셋 '{key}' 언로드 시도, 하지만 로드된 에셋에서 찾을 수 없습니다. 그룹의 일부로 해제되었거나 이미 언로드되었을 수 있습니다.");
        }
    }

    /// <summary>
    /// 주어진 라벨과 관련된 모든 에셋을 언로드합니다.
    /// </summary>
    /// <param name="label">언로드할 에셋의 Addressable 라벨입니다.</param>
    public void UnloadAssetsByLabel(string label)
    {
        if (string.IsNullOrEmpty(label))
        {
            Debug.LogWarning("[AddressablesManager] 라벨로 에셋 언로드 실패: 라벨이 비어있거나 null일 수 없습니다.");
            return;
        }

        if (_loadedGroupHandles.TryGetValue(label, out AsyncOperationHandle groupHandleToRelease))
        {
            Debug.Log($"[AddressablesManager] 라벨의 모든 에셋 언로드 중: {label}");
            Addressables.Release(groupHandleToRelease); // 이 그룹 작업으로 로드된 모든 에셋을 해제합니다
            _loadedGroupHandles.Remove(label);
        }
        else
        {
            Debug.LogWarning($"[AddressablesManager] 라벨 '{label}'의 에셋 언로드 시도, 하지만 로드된 그룹에서 라벨을 찾을 수 없습니다.");
        }
    }

    /// <summary>
    /// 이 AddressablesManager 인스턴스가 현재 관리하는 모든 에셋을 언로드합니다.
    /// 여기에는 개별적으로 로드된 에셋과 라벨로 로드된 모든 에셋이 포함됩니다.
    /// </summary>
    public void UnloadAllAssets()
    {
        Debug.Log("[AddressablesManager] 로드된 모든 Addressable 에셋 언로드 중...");

        // 개별적으로 로드된 에셋 언로드
        List<string> individualKeys = new List<string>(_loadedAssetsHandles.Keys);
        foreach (string key in individualKeys)
        {
            UnloadAsset(key); // 적절한 로깅과 제거를 위해 기존 UnloadAsset 메서드 사용
        }
        _loadedAssetsHandles.Clear(); // 이미 비어있어야 하지만, 안전을 위해 클리어

        // 라벨로 로드된 에셋 언로드
        List<string> groupLabels = new List<string>(_loadedGroupHandles.Keys);
        foreach (string label in groupLabels)
        {
            UnloadAssetsByLabel(label); // 적절한 로깅과 제거를 위해 기존 UnloadAssetsByLabel 사용
        }
        _loadedGroupHandles.Clear(); // 이미 비어있어야 하지만, 안전을 위해 클리어

        Debug.Log("[AddressablesManager] 모든 Addressable 에셋 언로드 완료.");
    }

    // --- 에셋 업데이트 ---
    /// <summary>
    /// 서버에서 사용 가능한 카탈로그 업데이트가 있는지 확인합니다.
    /// </summary>
    /// <param name="onUpdateAvailable">업데이트가 사용 가능한 경우 호출되는 콜백으로, 업데이트할 카탈로그 ID 목록을 전달합니다.</param>
    /// <param name="onNoUpdateNeeded">업데이트가 필요하지 않은 경우 호출되는 콜백입니다.</param>
    public void CheckForCatalogUpdates(System.Action<List<string>> onUpdateAvailable, System.Action onNoUpdateNeeded)
    {
        Debug.Log("[AddressablesManager] 카탈로그 업데이트 확인 중...");
        AsyncOperationHandle<List<string>> checkHandle = Addressables.CheckForCatalogUpdates(false);

        checkHandle.Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                List<string> catalogsToUpdate = handle.Result;
                if (catalogsToUpdate != null && catalogsToUpdate.Count > 0)
                {
                    Debug.Log($"[AddressablesManager] 다음 카탈로그에 대한 업데이트 사용 가능: {string.Join(", ", catalogsToUpdate)}");
                    onUpdateAvailable?.Invoke(catalogsToUpdate);
                }
                else
                {
                    Debug.Log("[AddressablesManager] 카탈로그 업데이트가 필요하지 않습니다.");
                    onNoUpdateNeeded?.Invoke();
                }
            }
            else
            {
                Debug.LogError("[AddressablesManager] 카탈로그 업데이트 확인 실패.");
                onNoUpdateNeeded?.Invoke(); // 또는 특정 오류 콜백
            }
        };
    }

    /// <summary>
    /// 지정된 카탈로그 목록에 대한 업데이트를 다운로드합니다.
    /// </summary>
    /// <param name="catalogsToUpdate">업데이트가 필요한 카탈로그 ID 목록입니다.</param>
    /// <param name="onProgress">다운로드 진행률(0.0에서 1.0)과 함께 호출되는 콜백입니다.</param>
    /// <param name="onComplete">지정된 모든 카탈로그가 성공적으로 업데이트되었을 때 호출되는 콜백입니다.</param>
    /// <param name="onError">업데이트 프로세스 중 오류가 발생했을 때 호출되는 콜백입니다.</param>
    public void UpdateCatalogs(List<string> catalogsToUpdate, System.Action<float> onProgress, System.Action onComplete, System.Action<string> onError)
    {
        if (catalogsToUpdate == null || catalogsToUpdate.Count == 0)
        {
            Debug.LogWarning("[AddressablesManager] 업데이트할 카탈로그 없이 UpdateCatalogs가 호출되었습니다.");
            onComplete?.Invoke();
            return;
        }

        Debug.Log($"[AddressablesManager] 다음 카탈로그 다운로드 시작: {string.Join(", ", catalogsToUpdate)}");
        AsyncOperationHandle<List<IResourceLocator>> updateHandle = Addressables.UpdateCatalogs(catalogsToUpdate, false);

        // 진행률 모니터링을 위한 코루틴
        StartCoroutine(TrackUpdateProgress(updateHandle, onProgress, () =>
        {
            if (updateHandle.Status == AsyncOperationStatus.Succeeded)
            {
                Debug.Log("[AddressablesManager] 카탈로그 업데이트 성공적으로 완료.");
                onComplete?.Invoke();
            }
            else
            {
                string errorMsg = $"[AddressablesManager] 카탈로그 업데이트 실패. 오류: {updateHandle.OperationException}";
                Debug.LogError(errorMsg);
                onError?.Invoke(errorMsg);
            }
            Addressables.Release(updateHandle); // 핸들 해제
        }));
    }

    private System.Collections.IEnumerator TrackUpdateProgress(AsyncOperationHandle handle, System.Action<float> onProgress, System.Action onOperationComplete)
    {
        while (!handle.IsDone)
        {
            onProgress?.Invoke(handle.PercentComplete);
            yield return null;
        }
        // 최종 진행률 업데이트
        onProgress?.Invoke(handle.PercentComplete);
        onOperationComplete?.Invoke();
    }

    // --- 유틸리티 메서드 ---
    // 에셋의 다운로드 크기를 가져오는 메서드를 위한 플레이스홀더
    // public AsyncOperationHandle<long> GetDownloadSize(string keyOrLabel) { /* ... */ return default; }

    // 에셋 캐시를 지우는 메서드를 위한 플레이스홀더
    // public bool ClearCache(string keyOrLabel) { /* ... */ return false; }

    // --- 비동기 작업을 위한 코루틴 ---
    // 로딩 큐를 처리하기 위한 코루틴을 위한 플레이스홀더
    // private IEnumerator ProcessLoadQueue() { /* ... */ yield return null; }

    // --- IResourceLocator (UpdateCatalogs 결과와 관련, 더 고급 시나리오에 필요한 경우) ---
    // Addressables.UpdateCatalogs의 결과는 List<IResourceLocator>입니다
    // 이러한 로케이터는 자동으로 Addressables에 등록되므로 직접 상호작용이 필요한 경우는 드뭅니다.
    // 그러나 업데이트된 로케이터를 검사해야 하는 경우, onComplete 콜백에서 전달할 수 있습니다.

    // --- 생명주기 메서드 ---
    void Start()
    {
        // 초기화 로직, 예: 로드 큐 처리 시작 또는 업데이트 확인
        // StartCoroutine(ProcessLoadQueue());

        // 사용 예시 (제거하거나 UI 컨트롤러로 이동 가능)
        // CheckForCatalogUpdates(
        //     catalogs => {
        //         Debug.Log("업데이트 발견! 다운로드 시작...");
        //         UpdateCatalogs(catalogs,
        //             progress => Debug.Log($"업데이트 진행률: {progress * 100}%"),
        //             () => Debug.Log("모든 업데이트 다운로드 및 적용 완료!"),
        //             error => Debug.LogError($"업데이트 실패: {error}")
        //         );
        //     },
        //     () => Debug.Log("현재 업데이트가 필요하지 않습니다.")
        // );
    }

    void OnDestroy()
    {
        // 정리 로직, 예: 모든 에셋 언로드
        UnloadAllAssets(); // 매니저가 파괴될 때 모든 관리 중인 에셋이 해제되도록 보장
        if (_instance == this)
        {
            _instance = null;
        }
    }
}
