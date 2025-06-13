using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders; // Added for IResourceLocator
using System.Collections.Generic;
using System; // Added for Action
using System.Collections; // Added for IEnumerator

public class AddressablesManager : MonoBehaviour
{
    // Static instance for singleton access
    private static AddressablesManager _instance;
    public static AddressablesManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("AddressablesManager");
                _instance = go.AddComponent<AddressablesManager>();
                DontDestroyOnLoad(go); // Ensure the manager persists across scene loads
            }
            return _instance;
        }
    }

    // --- Handle Management ---
    // Stores handles for individually loaded assets. Key is the Addressable key.
    private Dictionary<string, AsyncOperationHandle> _loadedAssetsHandles = new Dictionary<string, AsyncOperationHandle>();
    // Stores handles for assets loaded by label. Key is the label.
    // The handle stored here is the main one from LoadAssetsAsync, which manages all sub-assets for that label.
    private Dictionary<string, AsyncOperationHandle> _loadedGroupHandles = new Dictionary<string, AsyncOperationHandle>();

    // Placeholder for asset loading queue - can be used for sequential loading if needed
    // private Queue<string> _loadQueue = new Queue<string>();

    // Placeholder for update check logic
    // private bool _isCheckingForUpdates = false;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject); // Destroy duplicate instances
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject); // Ensure the manager persists across scene loads
    }

    // --- Asset Loading ---
    /// <summary>
    /// Loads a single asset by its key.
    /// </summary>
    /// <typeparam name="T">The type of asset to load.</typeparam>
    /// <param name="key">The Addressable key of the asset.</param>
    /// <param name="onComplete">Callback invoked when the asset is successfully loaded.</param>
    /// <param name="onError">Callback invoked if an error occurs during loading.</param>
    public void LoadAsset<T>(string key, Action<T> onComplete, Action<string> onError) where T : UnityEngine.Object
    {
        if (string.IsNullOrEmpty(key))
        {
            onError?.Invoke("[AddressablesManager] LoadAsset failed: Key cannot be null or empty.");
            return;
        }

        if (_loadedAssetsHandles.TryGetValue(key, out AsyncOperationHandle existingHandle))
        {
            if (existingHandle.IsDone && existingHandle.Status == AsyncOperationStatus.Succeeded)
            {
                Debug.Log($"[AddressablesManager] Asset '{key}' already loaded. Returning cached asset.");
                onComplete?.Invoke(existingHandle.Result as T);
                return;
            }
            else if (!existingHandle.IsDone)
            {
                // Asset is already loading
                Debug.Log($"[AddressablesManager] Asset '{key}' is already loading. Attaching to existing operation.");
                existingHandle.Completed += handle =>
                {
                    if (handle.Status == AsyncOperationStatus.Succeeded)
                        onComplete?.Invoke(handle.Result as T);
                    else
                        onError?.Invoke($"[AddressablesManager] Failed to load asset '{key}'. Error: {handle.OperationException}");
                };
                return;
            }
            // Handle was there but failed or was invalid, try reloading
            Debug.LogWarning($"[AddressablesManager] Asset '{key}' had a previous failed/invalid handle. Attempting to reload.");
            Addressables.Release(existingHandle); // Release previous before trying again
            _loadedAssetsHandles.Remove(key);
        }

        Debug.Log($"[AddressablesManager] Loading asset with key: {key}");
        AsyncOperationHandle<T> loadHandle = Addressables.LoadAssetAsync<T>(key);
        _loadedAssetsHandles[key] = loadHandle; // Store handle immediately

        loadHandle.Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                Debug.Log($"[AddressablesManager] Asset '{key}' loaded successfully.");
                onComplete?.Invoke(handle.Result);
            }
            else
            {
                string errorMsg = $"[AddressablesManager] Failed to load asset '{key}'. Error: {handle.OperationException}";
                Debug.LogError(errorMsg);
                onError?.Invoke(errorMsg);
                // Remove failed handle
                if (_loadedAssetsHandles.ContainsKey(key) && _loadedAssetsHandles[key].Equals(handle))
                     _loadedAssetsHandles.Remove(key);
            }
        };
    }

    /// <summary>
    /// Loads multiple assets that share a common label.
    /// The onAssetLoaded callback is invoked for each asset as it loads.
    /// The onAllComplete callback is invoked once all assets for the label are loaded.
    /// </summary>
    /// <typeparam name="T">The type of assets to load.</typeparam>
    /// <param name="label">The Addressable label.</param>
    /// <param name="onAssetLoaded">Callback invoked for each individual asset loaded under this label.</param>
    /// <param name="onAllComplete">Callback invoked when all assets for the label have been processed.</param>
    /// <param name="onError">Callback invoked if a general error occurs with the label loading operation.</param>
    public void LoadAssetsByLabel<T>(string label, Action<T> onAssetLoaded, Action onAllComplete, Action<string> onError) where T : UnityEngine.Object
    {
        if (string.IsNullOrEmpty(label))
        {
            onError?.Invoke("[AddressablesManager] LoadAssetsByLabel failed: Label cannot be null or empty.");
            return;
        }

        if (_loadedGroupHandles.ContainsKey(label))
        {
            // Could potentially re-attach to an existing operation if it's still running,
            // but for simplicity, we warn and return or allow re-load if desired.
            Debug.LogWarning($"[AddressablesManager] Assets for label '{label}' are already loaded or loading. If you need to reload, unload them first.");
            // If you want to treat this as success and call onAllComplete:
            // onAllComplete?.Invoke();
            // Or if individual assets are needed, this logic would need to be more complex
            // to retrieve already loaded assets from the group operation.
            return;
        }

        Debug.Log($"[AddressablesManager] Loading assets with label: {label}");
        // The callback `Action<T> asset` in `LoadAssetsAsync` is invoked for each asset loaded that matches the label.
        AsyncOperationHandle<IList<T>> groupLoadHandle = Addressables.LoadAssetsAsync<T>(label, asset =>
        {
            // This callback is for *each* asset loaded.
            Debug.Log($"[AddressablesManager] Asset of type {typeof(T).Name} loaded via label '{label}': {asset}");
            onAssetLoaded?.Invoke(asset);
        });

        _loadedGroupHandles[label] = groupLoadHandle; // Store the main group operation handle

        groupLoadHandle.Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                Debug.Log($"[AddressablesManager] All assets for label '{label}' loaded successfully. Total count: {handle.Result.Count}");
                onAllComplete?.Invoke();
            }
            else
            {
                string errorMsg = $"[AddressablesManager] Failed to load assets for label '{label}'. Error: {handle.OperationException}";
                Debug.LogError(errorMsg);
                onError?.Invoke(errorMsg);
                // Remove failed handle
                if (_loadedGroupHandles.ContainsKey(label) && _loadedGroupHandles[label].Equals(handle))
                    _loadedGroupHandles.Remove(label);
            }
        };
    }


    // --- Asset Unloading ---
    // Note: More sophisticated automatic release (e.g., reference counting or tying to GameObject lifecycle)
    // can be added later if needed. For now, unloading is explicit.

    /// <summary>
    /// Unloads a single asset identified by its key.
    /// </summary>
    /// <param name="key">The Addressable key of the asset to unload.</param>
    public void UnloadAsset(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            Debug.LogWarning("[AddressablesManager] UnloadAsset failed: Key cannot be null or empty.");
            return;
        }

        if (_loadedAssetsHandles.TryGetValue(key, out AsyncOperationHandle handleToRelease))
        {
            Debug.Log($"[AddressablesManager] Unloading asset: {key}");
            Addressables.Release(handleToRelease);
            _loadedAssetsHandles.Remove(key);
        }
        else
        {
            Debug.LogWarning($"[AddressablesManager] Attempted to unload asset '{key}', but it was not found in loaded assets. It might have been released as part of a group or already unloaded.");
        }
    }

    /// <summary>
    /// Unloads all assets associated with a given label.
    /// </summary>
    /// <param name="label">The Addressable label of the assets to unload.</param>
    public void UnloadAssetsByLabel(string label)
    {
        if (string.IsNullOrEmpty(label))
        {
            Debug.LogWarning("[AddressablesManager] UnloadAssetsByLabel failed: Label cannot be null or empty.");
            return;
        }

        if (_loadedGroupHandles.TryGetValue(label, out AsyncOperationHandle groupHandleToRelease))
        {
            Debug.Log($"[AddressablesManager] Unloading all assets for label: {label}");
            Addressables.Release(groupHandleToRelease); // This releases all assets loaded by this group operation
            _loadedGroupHandles.Remove(label);
        }
        else
        {
            Debug.LogWarning($"[AddressablesManager] Attempted to unload assets for label '{label}', but the label was not found in loaded groups.");
        }
    }

    /// <summary>
    /// Unloads all assets currently managed by this AddressablesManager instance.
    /// This includes individually loaded assets and all assets loaded by labels.
    /// </summary>
    public void UnloadAllAssets()
    {
        Debug.Log("[AddressablesManager] Unloading all loaded Addressable assets...");

        // Unload individually loaded assets
        List<string> individualKeys = new List<string>(_loadedAssetsHandles.Keys);
        foreach (string key in individualKeys)
        {
            UnloadAsset(key); // Uses the existing UnloadAsset method for proper logging and removal
        }
        _loadedAssetsHandles.Clear(); // Should be empty by now, but clear just in case.

        // Unload assets loaded by label
        List<string> groupLabels = new List<string>(_loadedGroupHandles.Keys);
        foreach (string label in groupLabels)
        {
            UnloadAssetsByLabel(label); // Uses the existing UnloadAssetsByLabel for proper logging and removal
        }
        _loadedGroupHandles.Clear(); // Should be empty, but clear for safety.

        Debug.Log("[AddressablesManager] All Addressable assets unloaded.");
    }


    // --- Asset Updates ---
    /// <summary>
    /// Checks if there are any catalog updates available from the server.
    /// </summary>
    /// <param name="onUpdateAvailable">Callback invoked if updates are available, passing the list of catalog IDs to update.</param>
    /// <param name="onNoUpdateNeeded">Callback invoked if no updates are needed.</param>
    public void CheckForCatalogUpdates(System.Action<List<string>> onUpdateAvailable, System.Action onNoUpdateNeeded)
    {
        Debug.Log("[AddressablesManager] Checking for catalog updates...");
        AsyncOperationHandle<List<string>> checkHandle = Addressables.CheckForCatalogUpdates(false);

        checkHandle.Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                List<string> catalogsToUpdate = handle.Result;
                if (catalogsToUpdate != null && catalogsToUpdate.Count > 0)
                {
                    Debug.Log($"[AddressablesManager] Updates available for catalogs: {string.Join(", ", catalogsToUpdate)}");
                    onUpdateAvailable?.Invoke(catalogsToUpdate);
                }
                else
                {
                    Debug.Log("[AddressablesManager] No catalog updates needed.");
                    onNoUpdateNeeded?.Invoke();
                }
            }
            else
            {
                Debug.LogError("[AddressablesManager] Failed to check for catalog updates.");
                onNoUpdateNeeded?.Invoke(); // Or a specific error callback
            }
            // Addressables.Release(checkHandle); // Release the handle once done.
            // It's good practice but CheckForCatalogUpdates is tricky with its handle lifecycle if we want to use the result later.
            // For now, we assume it's okay or rely on Addressables internal management for this specific handle.
            // If issues arise, this might need revisiting.
        };
    }

    /// <summary>
    /// Downloads updates for the specified list of catalogs.
    /// </summary>
    /// <param name="catalogsToUpdate">The list of catalog IDs that need to be updated.</param>
    /// <param name="onProgress">Callback invoked with download progress (0.0 to 1.0).</param>
    /// <param name="onComplete">Callback invoked when all specified catalogs are successfully updated.</param>
    /// <param name="onError">Callback invoked if an error occurs during the update process.</param>
    public void UpdateCatalogs(List<string> catalogsToUpdate, System.Action<float> onProgress, System.Action onComplete, System.Action<string> onError)
    {
        if (catalogsToUpdate == null || catalogsToUpdate.Count == 0)
        {
            Debug.LogWarning("[AddressablesManager] UpdateCatalogs called with no catalogs to update.");
            onComplete?.Invoke();
            return;
        }

        Debug.Log($"[AddressablesManager] Starting download for catalogs: {string.Join(", ", catalogsToUpdate)}");
        AsyncOperationHandle<List<IResourceLocator>> updateHandle = Addressables.UpdateCatalogs(catalogsToUpdate, false);

        // Coroutine to monitor progress
        StartCoroutine(TrackUpdateProgress(updateHandle, onProgress, () =>
        {
            if (updateHandle.Status == AsyncOperationStatus.Succeeded)
            {
                Debug.Log("[AddressablesManager] Catalog update completed successfully.");
                onComplete?.Invoke();
            }
            else
            {
                string errorMsg = $"[AddressablesManager] Failed to update catalogs. Error: {updateHandle.OperationException}";
                Debug.LogError(errorMsg);
                onError?.Invoke(errorMsg);
            }
            Addressables.Release(updateHandle); // Release the handle
        }));
    }

    private System.Collections.IEnumerator TrackUpdateProgress(AsyncOperationHandle handle, System.Action<float> onProgress, System.Action onOperationComplete)
    {
        while (!handle.IsDone)
        {
            onProgress?.Invoke(handle.PercentComplete);
            yield return null;
        }
        // Final progress update
        onProgress?.Invoke(handle.PercentComplete);
        onOperationComplete?.Invoke();
    }

    // Placeholder for method to download/apply updates -- This comment can be removed or updated.
    // public AsyncOperationHandle<List<string>> UpdateContent() { /* ... */ return default; }


    // --- Utility Methods ---
    // Placeholder for method to get download size of assets
    // public AsyncOperationHandle<long> GetDownloadSize(string keyOrLabel) { /* ... */ return default; }

    // Placeholder for method to clear asset cache
    // public bool ClearCache(string keyOrLabel) { /* ... */ return false; }


    // --- Coroutines for Async Operations ---
    // Placeholder for coroutine to process the loading queue
    // private IEnumerator ProcessLoadQueue() { /* ... */ yield return null; }

    // Placeholder for coroutine to handle update check and download -- This comment can be removed or updated.
    // private IEnumerator CheckAndApplyUpdatesRoutine() { /* ... */ yield return null; }


    // --- IResourceLocator (related to UpdateCatalogs result, if needed for more advanced scenarios) ---
    // The result of Addressables.UpdateCatalogs is List<IResourceLocator>
    // These locators are automatically registered with Addressables, so direct interaction is often not needed.
    // However, if you need to inspect the updated locators, you could pass them in the onComplete callback.


    // --- Lifecycle Methods ---
    void Start()
    {
        // Initialization logic, e.g., start processing load queue or check for updates
        // StartCoroutine(ProcessLoadQueue());

        // Example Usage (Can be removed or moved to a UI Controller)
        // CheckForCatalogUpdates(
        //     catalogs => {
        //         Debug.Log("Updates found! Starting download...");
        //         UpdateCatalogs(catalogs,
        //             progress => Debug.Log($"Update progress: {progress * 100}%"),
        //             () => Debug.Log("All updates downloaded and applied!"),
        //             error => Debug.LogError($"Update failed: {error}")
        //         );
        //     },
        //     () => Debug.Log("No updates needed at this time.")
        // );
    }

    void OnDestroy()
    {
        // Cleanup logic, e.g., unload all assets
        UnloadAllAssets(); // Ensure all managed assets are released when the manager is destroyed
        if (_instance == this)
        {
            _instance = null;
        }
    }
}
