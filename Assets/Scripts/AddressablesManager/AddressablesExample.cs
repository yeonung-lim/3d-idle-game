using UnityEngine;
using UnityEngine.UI;
using TMPro; // If using TextMeshPro for Text elements
using System.Collections.Generic; // For List<string> in updates
using System; // For Action

public class AddressablesExample : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField keyInput; // Use InputField if not using TextMeshPro
    public Button checkUpdatesButton;
    public Button loadAssetButton;
    public Button unloadAssetButton;
    public Button loadGroupButton;
    public Button unloadGroupButton;
    public TMP_Text statusText; // Use Text if not using TextMeshPro
    public GameObject assetDisplayObject; // Optional: assign a placeholder in scene

    private GameObject _instantiatedAsset;
    private List<string> _catalogsToUpdate; // Stores catalogs that need updating

    void Start()
    {
        // Ensure AddressablesManager instance is ready
        if (AddressablesManager.Instance == null)
        {
            LogStatus("AddressablesManager not initialized!");
            // Disable UI if manager is not available
            SetUIInteractable(false);
            return;
        }

        // Assign button listeners
        checkUpdatesButton.onClick.AddListener(HandleCheckUpdates);
        loadAssetButton.onClick.AddListener(HandleLoadAsset);
        unloadAssetButton.onClick.AddListener(HandleUnloadAsset);
        loadGroupButton.onClick.AddListener(HandleLoadGroup);
        unloadGroupButton.onClick.AddListener(HandleUnloadGroup);

        LogStatus("Addressables Example Initialized. Enter an asset key or label and choose an action.");
        SetUIInteractable(true); // Initially, update check might be the first logical step
    }

    void SetUIInteractable(bool isInteractable, bool keepUpdateEnabled = false)
    {
        loadAssetButton.interactable = isInteractable;
        unloadAssetButton.interactable = isInteractable;
        loadGroupButton.interactable = isInteractable;
        unloadGroupButton.interactable = isInteractable;
        keyInput.interactable = isInteractable;

        // Check updates button might have different interactable states
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
        LogStatus("Checking for catalog updates...");
        SetUIInteractable(false, true); // Disable other buttons, keep update button enabled or manage its state separately

        AddressablesManager.Instance.CheckForCatalogUpdates(
            catalogs => // onUpdateAvailable
            {
                _catalogsToUpdate = catalogs;
                LogStatus($"Updates available for: {string.Join(", ", catalogs)}. Click 'Update Catalogs' (not implemented in this example UI) or proceed to download them.");
                // In a real scenario, you might enable an "Update Now" button here.
                // For this example, let's try to update them immediately.
                HandleUpdateCatalogs();
            },
            () => // onNoUpdateNeeded
            {
                LogStatus("No catalog updates needed.");
                SetUIInteractable(true);
            }
        );
    }

    void HandleUpdateCatalogs()
    {
        if (_catalogsToUpdate == null || _catalogsToUpdate.Count == 0)
        {
            LogStatus("No catalogs marked for update.");
            SetUIInteractable(true);
            return;
        }

        LogStatus($"Starting download for catalogs: {string.Join(", ", _catalogsToUpdate)}");
        SetUIInteractable(false); // Disable UI during update

        AddressablesManager.Instance.UpdateCatalogs(
            _catalogsToUpdate,
            progress => // onProgress
            {
                LogStatus($"Update progress: {progress * 100:F2}%");
            },
            () => // onComplete
            {
                LogStatus("Catalog updates downloaded and applied successfully!");
                _catalogsToUpdate = null; // Clear the list
                SetUIInteractable(true);
            },
            error => // onError
            {
                LogStatus($"Catalog update failed: {error}");
                SetUIInteractable(true);
            }
        );
    }


    void HandleLoadAsset()
    {
        string key = keyInput.text;
        if (string.IsNullOrEmpty(key))
        {
            LogStatus("Please enter an asset key to load.");
            return;
        }

        LogStatus($"Loading asset with key: {key}...");
        SetUIInteractable(false);

        // Destroy previously instantiated asset
        if (_instantiatedAsset != null)
        {
            Destroy(_instantiatedAsset);
            _instantiatedAsset = null;
        }

        AddressablesManager.Instance.LoadAsset<GameObject>(
            key,
            loadedAsset => // onComplete
            {
                LogStatus($"Asset '{key}' loaded successfully.");
                if (loadedAsset != null)
                {
                    // Instantiate the loaded asset
                    _instantiatedAsset = Instantiate(loadedAsset, assetDisplayObject != null ? assetDisplayObject.transform : Vector3.zero, Quaternion.identity);
                    _instantiatedAsset.name = loadedAsset.name + "_Instance";
                    LogStatus($"Instantiated '{_instantiatedAsset.name}'.");

                    // Example: If you loaded a Material instead of a GameObject
                    // if (assetDisplayObject != null && loadedAsset is Material newMat) {
                    //     Renderer renderer = assetDisplayObject.GetComponent<Renderer>();
                    //     if (renderer != null) renderer.material = newMat;
                    //     LogStatus($"Applied material '{newMat.name}' to {assetDisplayObject.name}.");
                    // } else if (loadedAsset is Material) {
                    //     LogStatus("Loaded material but no display object to apply it to.");
                    // }

                }
                SetUIInteractable(true);
            },
            error => // onError
            {
                LogStatus($"Failed to load asset '{key}'. Error: {error}");
                SetUIInteractable(true);
            }
        );
    }

    void HandleUnloadAsset()
    {
        string key = keyInput.text;
        if (string.IsNullOrEmpty(key))
        {
            LogStatus("Please enter an asset key to unload.");
            return;
        }

        LogStatus($"Unloading asset: {key}...");

        // Destroy the specific instantiated object if it matches the key (more complex logic might be needed if keys don't match names)
        // For simplicity, this example assumes one instantiated asset at a time.
        if (_instantiatedAsset != null)
        {
             // A more robust system would track instantiated objects by their key.
             // For this example, we assume the current key corresponds to the current object.
            Destroy(_instantiatedAsset);
            _instantiatedAsset = null;
            LogStatus("Destroyed instantiated asset instance.");
        }

        AddressablesManager.Instance.UnloadAsset(key);
        LogStatus($"Unload call for asset '{key}' complete. Check logs from AddressablesManager for details.");
        SetUIInteractable(true);
    }

    void HandleLoadGroup()
    {
        string label = keyInput.text;
        if (string.IsNullOrEmpty(label))
        {
            LogStatus("Please enter an asset label to load as a group.");
            return;
        }

        LogStatus($"Loading assets with label: {label}...");
        SetUIInteractable(false);

        // Clear previously instantiated asset if any, as group loading might bring multiple.
        if (_instantiatedAsset != null)
        {
            Destroy(_instantiatedAsset);
            _instantiatedAsset = null;
        }
        // Note: This example doesn't instantiate group-loaded assets to keep it simple.
        // You would need a more complex system to manage multiple game objects from a group load.

        AddressablesManager.Instance.LoadAssetsByLabel<GameObject>(
            label,
            asset => // onAssetLoaded (for each asset in the group)
            {
                if (asset != null)
                {
                    LogStatus($"Asset '{asset.name}' loaded as part of group '{label}'.");
                    // Example: Instantiate or process each asset.
                    // For simplicity, we're just logging here.
                    // If instantiating, manage these objects in a list.
                }
            },
            () => // onAllComplete
            {
                LogStatus($"All assets for label '{label}' loaded.");
                SetUIInteractable(true);
            },
            error => // onError
            {
                LogStatus($"Failed to load assets for label '{label}'. Error: {error}");
                SetUIInteractable(true);
            }
        );
    }

    void HandleUnloadGroup()
    {
        string label = keyInput.text;
        if (string.IsNullOrEmpty(label))
        {
            LogStatus("Please enter an asset label to unload.");
            return;
        }

        LogStatus($"Unloading assets with label: {label}...");
        // Note: If you instantiated assets from HandleLoadGroup, you'd destroy them here.
        // This example only logs, so no GameObjects to clean up from group loading here.

        AddressablesManager.Instance.UnloadAssetsByLabel(label);
        LogStatus($"Unload call for label '{label}' complete. Check logs from AddressablesManager.");
        SetUIInteractable(true);
    }

    void OnDestroy()
    {
        // Clean up button listeners
        if (checkUpdatesButton != null) checkUpdatesButton.onClick.RemoveAllListeners();
        if (loadAssetButton != null) loadAssetButton.onClick.RemoveAllListeners();
        if (unloadAssetButton != null) unloadAssetButton.onClick.RemoveAllListeners();
        if (loadGroupButton != null) loadGroupButton.onClick.RemoveAllListeners();
        if (unloadGroupButton != null) unloadGroupButton.onClick.RemoveAllListeners();

        // Destroy any remaining instantiated asset
        if (_instantiatedAsset != null)
        {
            Destroy(_instantiatedAsset);
        }
    }
}
