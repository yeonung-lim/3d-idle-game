using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections;
using System.Collections.Generic;
using System.Reflection; // private 멤버 접근용
using System; // Action 사용을 위해

public class AddressablesManagerRuntimeTests
{
    private AddressablesManager _manager;
    private const string TestCubeKey = "TestCubePrefab"; // 이 키로 Addressable 프리팹을 정의하세요
    private const string TestTextKey = "TestTextFile";   // 이 키로 Addressable 텍스트 파일을 정의하세요
    private const string TestLabel = "TestLabel";       // 이 라벨을 정의하고 일부 에셋에 할당하세요
    private const float TimeoutSeconds = 10f; // 비동기 작업을 위한 타임아웃

    // private 딕셔너리 _loadedAssetsHandles에 접근하기 위한 헬퍼
    private Dictionary<string, AsyncOperationHandle> GetLoadedAssetsHandles(AddressablesManager instance)
    {
        FieldInfo field = typeof(AddressablesManager).GetField("_loadedAssetsHandles", BindingFlags.NonPublic | BindingFlags.Instance);
        return field?.GetValue(instance) as Dictionary<string, AsyncOperationHandle>;
    }

    // private 딕셔너리 _loadedGroupHandles에 접근하기 위한 헬퍼
    private Dictionary<string, AsyncOperationHandle> GetLoadedGroupHandles(AddressablesManager instance)
    {
        FieldInfo field = typeof(AddressablesManager).GetField("_loadedGroupHandles", BindingFlags.NonPublic | BindingFlags.Instance);
        return field?.GetValue(instance) as Dictionary<string, AsyncOperationHandle>;
    }

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        // Addressables가 초기화되었는지 확인
        var initHandle = Addressables.InitializeAsync();
        yield return initHandle;
        if (initHandle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError("SetUp에서 Addressables 초기화 실패.");
            Assert.Fail("Addressables 초기화 실패.");
            yield break;
        }

        // 싱글톤 인스턴스 가져오기
        _manager = AddressablesManager.Instance;
        // 각 테스트를 위해 깨끗한 상태로 초기화 (같은 씬/컨텍스트에서 테스트가 실행되는 경우 중요)
        _manager.UnloadAllAssets();
        yield return null; // 잠재적인 정리를 위해 한 프레임 대기
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        if (_manager != null)
        {
            _manager.UnloadAllAssets(); // 모든 에셋이 언로드되었는지 확인
            // 다음 테스트를 위해 매니저 GameObject를 파괴하여 깨끗한 상태로 시작
            // 싱글톤이 기본적으로 유지되기 때문에 중요
            GameObject.Destroy(_manager.gameObject);
            _manager = null;

            // AddressablesManager의 정적 인스턴스 필드 초기화
            var type = typeof(AddressablesManager);
            var instanceField = type.GetField("_instance", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            if (instanceField != null)
            {
                instanceField.SetValue(null, null);
            }
        }
        yield return null;
    }

    [UnityTest]
    public IEnumerator LoadAsset_ValidKey_LoadsSuccessfully()
    {
        bool completed = false;
        bool success = false;
        UnityEngine.Object loadedAsset = null;

        _manager.LoadAsset<GameObject>(TestCubeKey,
            result => {
                completed = true;
                success = true;
                loadedAsset = result;
            },
            error => { completed = true; success = false; Debug.LogError(error); });

        yield return new WaitUntil(() => completed || Time.time > Time.time + TimeoutSeconds);

        Assert.IsTrue(completed, $"'{TestCubeKey}' 키에 대한 LoadAsset이 타임아웃되었습니다.");
        Assert.IsTrue(success, $"'{TestCubeKey}'에 대한 LoadAsset이 실패했습니다.");
        Assert.IsNotNull(loadedAsset, $"로드된 에셋 '{TestCubeKey}'는 null이 아니어야 합니다.");
        Assert.IsInstanceOf<GameObject>(loadedAsset, $"로드된 에셋 '{TestCubeKey}'는 GameObject여야 합니다.");
        Assert.IsTrue(GetLoadedAssetsHandles(_manager).ContainsKey(TestCubeKey), $"'{TestCubeKey}'에 대한 핸들이 저장되어야 합니다.");
    }

    [UnityTest]
    public IEnumerator LoadAsset_InvalidKey_CallsOnError()
    {
        bool completed = false;
        bool errorCalled = false;
        string invalidKey = "InvalidKey123";

        _manager.LoadAsset<GameObject>(invalidKey,
            result => { completed = true; /* 실행되지 않아야 함 */ },
            error => { completed = true; errorCalled = true; });

        yield return new WaitUntil(() => completed || Time.time > Time.time + TimeoutSeconds);

        Assert.IsTrue(completed, $"'{invalidKey}' 키에 대한 LoadAsset(잘못된 키)이 타임아웃되었습니다.");
        Assert.IsTrue(errorCalled, $"잘못된 키 '{invalidKey}'에 대해 onError가 호출되어야 합니다.");
        Assert.IsFalse(GetLoadedAssetsHandles(_manager).ContainsKey(invalidKey), "잘못된 키에 대한 핸들이 저장되지 않아야 합니다.");
    }

    [UnityTest]
    public IEnumerator UnloadAsset_RemovesHandle()
    {
        // 먼저 에셋을 로드
        bool loadCompleted = false;
        _manager.LoadAsset<GameObject>(TestCubeKey, result => loadCompleted = true, error => loadCompleted = true);
        yield return new WaitUntil(() => loadCompleted || Time.time > Time.time + TimeoutSeconds);
        Assert.IsTrue(loadCompleted, "UnloadAsset 테스트를 위한 사전 조건 LoadAsset이 실패하거나 타임아웃되었습니다.");
        Assert.IsTrue(GetLoadedAssetsHandles(_manager).ContainsKey(TestCubeKey), "UnloadAsset 테스트를 위한 에셋이 로드되지 않았습니다.");

        // 이제 언로드
        _manager.UnloadAsset(TestCubeKey);
        yield return null; // 언로드 처리를 위해 한 프레임 대기

        Assert.IsFalse(GetLoadedAssetsHandles(_manager).ContainsKey(TestCubeKey), $"언로드 후 '{TestCubeKey}'에 대한 핸들이 제거되어야 합니다.");
    }

    [UnityTest]
    public IEnumerator LoadAssetsByLabel_ValidLabel_LoadsAssets()
    {
        bool assetLoadedCallbackCalled = false;
        bool allCompleteCallbackCalled = false;
        int assetsLoadedCount = 0;

        _manager.LoadAssetsByLabel<GameObject>(TestLabel,
            asset => // onAssetLoaded
            {
                assetLoadedCallbackCalled = true;
                assetsLoadedCount++;
                Assert.IsNotNull(asset, "라벨로 로드된 에셋은 null이 아니어야 합니다.");
            },
            () => // onAllComplete
            {
                allCompleteCallbackCalled = true;
            },
            error => // onError
            {
                allCompleteCallbackCalled = true; // 대기 중단을 위해
                Debug.LogError(error);
                Assert.Fail($"'{TestLabel}'에 대한 LoadAssetsByLabel 실패: {error}");
            });

        yield return new WaitUntil(() => allCompleteCallbackCalled || Time.time > Time.time + TimeoutSeconds);

        Assert.IsTrue(allCompleteCallbackCalled, $"'{TestLabel}' 라벨에 대한 LoadAssetsByLabel이 타임아웃되었습니다.");
        // TestLabel이 이 assertion을 위해 최소한 하나의 에셋을 로드하도록 구성되었다고 가정
        Assert.IsTrue(assetLoadedCallbackCalled, "라벨 로딩에 대해 onAssetLoaded 콜백이 호출되지 않았습니다.");
        Assert.Greater(assetsLoadedCount, 0, "라벨에 대해 최소한 하나의 에셋이 로드되어야 합니다.");
        Assert.IsTrue(GetLoadedGroupHandles(_manager).ContainsKey(TestLabel), $"'{TestLabel}'에 대한 그룹 핸들이 저장되어야 합니다.");
    }

    [UnityTest]
    public IEnumerator UnloadAssetsByLabel_RemovesGroupHandle()
    {
        // 먼저 라벨로 에셋들을 로드
        bool loadCompleted = false;
        _manager.LoadAssetsByLabel<GameObject>(TestLabel, null, () => loadCompleted = true, error => loadCompleted = true);
        yield return new WaitUntil(() => loadCompleted || Time.time > Time.time + TimeoutSeconds);
        Assert.IsTrue(loadCompleted, "UnloadAssetsByLabel 테스트를 위한 사전 조건 LoadAssetsByLabel이 실패하거나 타임아웃되었습니다.");
        Assert.IsTrue(GetLoadedGroupHandles(_manager).ContainsKey(TestLabel), "UnloadAssetsByLabel 테스트를 위한 에셋들이 라벨로 로드되지 않았습니다.");

        // 이제 언로드
        _manager.UnloadAssetsByLabel(TestLabel);
        yield return null;

        Assert.IsFalse(GetLoadedGroupHandles(_manager).ContainsKey(TestLabel), $"언로드 후 '{TestLabel}'에 대한 그룹 핸들이 제거되어야 합니다.");
    }

    [UnityTest]
    public IEnumerator UnloadAllAssets_ClearsAllHandles()
    {
        // 단일 에셋과 그룹 로드
        bool singleLoadDone = false;
        bool groupLoadDone = false;
        _manager.LoadAsset<GameObject>(TestCubeKey, r => singleLoadDone = true, e => singleLoadDone = true);
        _manager.LoadAssetsByLabel<GameObject>(TestLabel, null, () => groupLoadDone = true, e => groupLoadDone = true);
        yield return new WaitUntil(() => (singleLoadDone && groupLoadDone) || Time.time > Time.time + TimeoutSeconds);

        Assert.IsTrue(singleLoadDone, "UnloadAllAssets 테스트를 위한 단일 에셋 로드가 실패/타임아웃되었습니다.");
        Assert.IsTrue(groupLoadDone, "UnloadAllAssets 테스트를 위한 그룹 에셋 로드가 실패/타임아웃되었습니다.");
        Assert.IsTrue(GetLoadedAssetsHandles(_manager).ContainsKey(TestCubeKey), "UnloadAllAssets 테스트를 위한 단일 에셋이 로드되지 않았습니다.");
        Assert.IsTrue(GetLoadedGroupHandles(_manager).ContainsKey(TestLabel), "UnloadAllAssets 테스트를 위한 그룹 에셋들이 로드되지 않았습니다.");

        _manager.UnloadAllAssets();
        yield return null;

        Assert.IsEmpty(GetLoadedAssetsHandles(_manager), "UnloadAllAssets 후 _loadedAssetsHandles가 비어있어야 합니다.");
        Assert.IsEmpty(GetLoadedGroupHandles(_manager), "UnloadAllAssets 후 _loadedGroupHandles가 비어있어야 합니다.");
    }

    [UnityTest]
    public IEnumerator CheckForCatalogUpdates_InvokesACallback()
    {
        bool callbackInvoked = false;
        float startTime = Time.time;

        _manager.CheckForCatalogUpdates(
            updates => { callbackInvoked = true; /* Debug.Log("테스트에서 업데이트 가능."); */ },
            () => { callbackInvoked = true; /* Debug.Log("테스트에서 업데이트 불필요."); */ }
        );

        yield return new WaitUntil(() => callbackInvoked || Time.time > startTime + TimeoutSeconds);
        Assert.IsTrue(callbackInvoked, "CheckForCatalogUpdates가 타임아웃 내에 콜백을 호출하지 않았습니다.");
    }

    [UnityTest]
    public IEnumerator UpdateCatalogs_WithEmptyList_InvokesOnComplete()
    {
        bool onCompleteCalled = false;
        bool onErrorCalled = false;
        float startTime = Time.time;

        _manager.UpdateCatalogs(new List<string>(),
            progress => {},
            () => onCompleteCalled = true,
            error => onErrorCalled = true);

        yield return new WaitUntil(() => (onCompleteCalled || onErrorCalled) || Time.time > startTime + TimeoutSeconds);

        Assert.IsTrue(onCompleteCalled, "빈 리스트로 UpdateCatalogs를 호출하면 onComplete가 호출되어야 합니다.");
        Assert.IsFalse(onErrorCalled, "빈 리스트로 UpdateCatalogs를 호출하면 onError가 호출되지 않아야 합니다.");
    }

    [UnityTest]
    public IEnumerator UpdateCatalogs_WithNullList_InvokesOnComplete()
    {
        // 현재 구현에 따르면, 초기 검사로 인해 null 리스트도 onComplete를 호출합니다.
        bool onCompleteCalled = false;
        bool onErrorCalled = false;
        float startTime = Time.time;

        _manager.UpdateCatalogs(null,
            progress => {},
            () => onCompleteCalled = true,
            error => onErrorCalled = true);

        yield return new WaitUntil(() => (onCompleteCalled || onErrorCalled) || Time.time > startTime + TimeoutSeconds);

        Assert.IsTrue(onCompleteCalled, "null 리스트로 UpdateCatalogs를 호출하면 onComplete가 호출되어야 합니다.");
        Assert.IsFalse(onErrorCalled, "null 리스트로 UpdateCatalogs를 호출하면 onError가 호출되지 않아야 합니다.");
    }
}
