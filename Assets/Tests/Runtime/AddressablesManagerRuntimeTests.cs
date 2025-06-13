using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections;
using System.Collections.Generic;
using System.Reflection; // For accessing private members
using System; // For Action

public class AddressablesManagerRuntimeTests
{
    private AddressablesManager _manager;
    private const string TestCubeKey = "TestCubePrefab"; // Define an Addressable prefab with this key
    private const string TestTextKey = "TestTextFile";   // Define an Addressable text file with this key
    private const string TestLabel = "TestLabel";       // Define a label and assign it to some assets
    private const float TimeoutSeconds = 10f; // Timeout for async operations

    // Helper to access private dictionary _loadedAssetsHandles
    private Dictionary<string, AsyncOperationHandle> GetLoadedAssetsHandles(AddressablesManager instance)
    {
        FieldInfo field = typeof(AddressablesManager).GetField("_loadedAssetsHandles", BindingFlags.NonPublic | BindingFlags.Instance);
        return field?.GetValue(instance) as Dictionary<string, AsyncOperationHandle>;
    }

    // Helper to access private dictionary _loadedGroupHandles
    private Dictionary<string, AsyncOperationHandle> GetLoadedGroupHandles(AddressablesManager instance)
    {
        FieldInfo field = typeof(AddressablesManager).GetField("_loadedGroupHandles", BindingFlags.NonPublic | BindingFlags.Instance);
        return field?.GetValue(instance) as Dictionary<string, AsyncOperationHandle>;
    }

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        // Ensure Addressables is initialized
        var initHandle = Addressables.InitializeAsync();
        yield return initHandle;
        if (initHandle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError("Addressables failed to initialize in SetUp.");
            Assert.Fail("Addressables initialization failed.");
            yield break;
        }

        // Get the singleton instance
        _manager = AddressablesManager.Instance;
        // Ensure it's clean for each test (important if tests run in the same scene/context)
        _manager.UnloadAllAssets();
        yield return null; // Wait a frame for any potential cleanup
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        if (_manager != null)
        {
            _manager.UnloadAllAssets(); // Ensure all assets are unloaded
             // Destroy the manager GameObject to ensure a fresh start for the next test.
            // This is important because the singleton persists by default.
            GameObject.Destroy(_manager.gameObject);
            _manager = null;

            // Reset the static instance field in AddressablesManager
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

        Assert.IsTrue(completed, $"LoadAsset timed out for key '{TestCubeKey}'.");
        Assert.IsTrue(success, $"LoadAsset for '{TestCubeKey}' failed.");
        Assert.IsNotNull(loadedAsset, $"Loaded asset '{TestCubeKey}' should not be null.");
        Assert.IsInstanceOf<GameObject>(loadedAsset, $"Loaded asset '{TestCubeKey}' should be a GameObject.");
        Assert.IsTrue(GetLoadedAssetsHandles(_manager).ContainsKey(TestCubeKey), $"Handle for '{TestCubeKey}' should be stored.");
    }

    [UnityTest]
    public IEnumerator LoadAsset_InvalidKey_CallsOnError()
    {
        bool completed = false;
        bool errorCalled = false;
        string invalidKey = "InvalidKey123";

        _manager.LoadAsset<GameObject>(invalidKey,
            result => { completed = true; /* Should not happen */ },
            error => { completed = true; errorCalled = true; });

        yield return new WaitUntil(() => completed || Time.time > Time.time + TimeoutSeconds);

        Assert.IsTrue(completed, $"LoadAsset (invalid key) timed out for key '{invalidKey}'.");
        Assert.IsTrue(errorCalled, $"onError should be called for invalid key '{invalidKey}'.");
        Assert.IsFalse(GetLoadedAssetsHandles(_manager).ContainsKey(invalidKey), "Handle for invalid key should not be stored.");
    }

    [UnityTest]
    public IEnumerator UnloadAsset_RemovesHandle()
    {
        // First, load an asset
        bool loadCompleted = false;
        _manager.LoadAsset<GameObject>(TestCubeKey, result => loadCompleted = true, error => loadCompleted = true);
        yield return new WaitUntil(() => loadCompleted || Time.time > Time.time + TimeoutSeconds);
        Assert.IsTrue(loadCompleted, "Prerequisite LoadAsset failed or timed out for UnloadAsset test.");
        Assert.IsTrue(GetLoadedAssetsHandles(_manager).ContainsKey(TestCubeKey), "Asset not loaded for UnloadAsset test.");

        // Now, unload it
        _manager.UnloadAsset(TestCubeKey);
        yield return null; // Give a frame for unload to process

        Assert.IsFalse(GetLoadedAssetsHandles(_manager).ContainsKey(TestCubeKey), $"Handle for '{TestCubeKey}' should be removed after unload.");
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
                Assert.IsNotNull(asset, "Asset loaded by label should not be null.");
            },
            () => // onAllComplete
            {
                allCompleteCallbackCalled = true;
            },
            error => // onError
            {
                allCompleteCallbackCalled = true; // to stop waiting
                Debug.LogError(error);
                Assert.Fail($"LoadAssetsByLabel for '{TestLabel}' failed: {error}");
            });

        yield return new WaitUntil(() => allCompleteCallbackCalled || Time.time > Time.time + TimeoutSeconds);

        Assert.IsTrue(allCompleteCallbackCalled, $"LoadAssetsByLabel timed out for label '{TestLabel}'.");
        // Assuming TestLabel is configured to load at least one asset for this assertion
        Assert.IsTrue(assetLoadedCallbackCalled, "onAssetLoaded callback was not called for label loading.");
        Assert.Greater(assetsLoadedCount, 0, "Expected at least one asset to be loaded for the label.");
        Assert.IsTrue(GetLoadedGroupHandles(_manager).ContainsKey(TestLabel), $"Group handle for '{TestLabel}' should be stored.");
    }

    [UnityTest]
    public IEnumerator UnloadAssetsByLabel_RemovesGroupHandle()
    {
        // First, load assets by label
        bool loadCompleted = false;
        _manager.LoadAssetsByLabel<GameObject>(TestLabel, null, () => loadCompleted = true, error => loadCompleted = true);
        yield return new WaitUntil(() => loadCompleted || Time.time > Time.time + TimeoutSeconds);
        Assert.IsTrue(loadCompleted, "Prerequisite LoadAssetsByLabel failed or timed out for UnloadAssetsByLabel test.");
        Assert.IsTrue(GetLoadedGroupHandles(_manager).ContainsKey(TestLabel), "Assets not loaded by label for UnloadAssetsByLabel test.");

        // Now, unload them
        _manager.UnloadAssetsByLabel(TestLabel);
        yield return null;

        Assert.IsFalse(GetLoadedGroupHandles(_manager).ContainsKey(TestLabel), $"Group handle for '{TestLabel}' should be removed after unload.");
    }

    [UnityTest]
    public IEnumerator UnloadAllAssets_ClearsAllHandles()
    {
        // Load a single asset and a group
        bool singleLoadDone = false;
        bool groupLoadDone = false;
        _manager.LoadAsset<GameObject>(TestCubeKey, r => singleLoadDone = true, e => singleLoadDone = true);
        _manager.LoadAssetsByLabel<GameObject>(TestLabel, null, () => groupLoadDone = true, e => groupLoadDone = true);
        yield return new WaitUntil(() => (singleLoadDone && groupLoadDone) || Time.time > Time.time + TimeoutSeconds);

        Assert.IsTrue(singleLoadDone, "Single asset load failed/timed out for UnloadAllAssets test.");
        Assert.IsTrue(groupLoadDone, "Group asset load failed/timed out for UnloadAllAssets test.");
        Assert.IsTrue(GetLoadedAssetsHandles(_manager).ContainsKey(TestCubeKey), "Single asset not loaded for UnloadAllAssets test.");
        Assert.IsTrue(GetLoadedGroupHandles(_manager).ContainsKey(TestLabel), "Group assets not loaded for UnloadAllAssets test.");


        _manager.UnloadAllAssets();
        yield return null;

        Assert.IsEmpty(GetLoadedAssetsHandles(_manager), "_loadedAssetsHandles should be empty after UnloadAllAssets.");
        Assert.IsEmpty(GetLoadedGroupHandles(_manager), "_loadedGroupHandles should be empty after UnloadAllAssets.");
    }

    [UnityTest]
    public IEnumerator CheckForCatalogUpdates_InvokesACallback()
    {
        bool callbackInvoked = false;
        float startTime = Time.time;

        _manager.CheckForCatalogUpdates(
            updates => { callbackInvoked = true; /* Debug.Log("Updates available in test."); */ },
            () => { callbackInvoked = true; /* Debug.Log("No updates needed in test."); */ }
        );

        yield return new WaitUntil(() => callbackInvoked || Time.time > startTime + TimeoutSeconds);
        Assert.IsTrue(callbackInvoked, "CheckForCatalogUpdates did not invoke a callback within timeout.");
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

        Assert.IsTrue(onCompleteCalled, "UpdateCatalogs with empty list should call onComplete.");
        Assert.IsFalse(onErrorCalled, "UpdateCatalogs with empty list should not call onError.");
    }

    [UnityTest]
    public IEnumerator UpdateCatalogs_WithNullList_InvokesOnComplete()
    {
        // Based on current implementation, null list also calls onComplete due to the initial check.
        bool onCompleteCalled = false;
        bool onErrorCalled = false;
        float startTime = Time.time;

        _manager.UpdateCatalogs(null,
            progress => {},
            () => onCompleteCalled = true,
            error => onErrorCalled = true);

        yield return new WaitUntil(() => (onCompleteCalled || onErrorCalled) || Time.time > startTime + TimeoutSeconds);

        Assert.IsTrue(onCompleteCalled, "UpdateCatalogs with null list should call onComplete.");
        Assert.IsFalse(onErrorCalled, "UpdateCatalogs with null list should not call onError.");
    }
}
