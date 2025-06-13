using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Build; // For AddressablesPlayerBuildResult and BuildScript
using UnityEngine; // For Debug.Log
using System; // For Action

public class AddressablesBuildScript
{
    /// <summary>
    /// Action hook that external scripts can subscribe to for uploading content after a successful build.
    /// The AddressablesPlayerBuildResult contains information about the build, including output paths.
    /// </summary>
    public static Action<AddressablesPlayerBuildResult> OnBuildCompletedUploadHook;

    [MenuItem("Addressables/Build Content")]
    public static void BuildAddressablesContent()
    {
        Debug.Log("[AddressablesBuildScript] Starting Addressables content build...");

        // Ensure Addressable Asset Settings are available
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogError("[AddressablesBuildScript] Addressable Asset Settings not found. Cannot build content.");
            return;
        }

        // Conceptual: Differentiating Local and Remote Groups
        // Before building, one could iterate through groups and modify settings if needed:
        // foreach (var group in settings.groups)
        // {
        //     if (group.Name.Contains("_Remote")) // Example: Naming convention for remote groups
        //     {
        //         // Potentially set specific options for this group, e.g., its BundleMode if not already set.
        //         // group.Settings.SetValue("BundleMode", BundledAssetGroupSchema.BundlePackingMode.PackSeparately);
        //         Debug.Log($"[AddressablesBuildScript] Group '{group.Name}' identified as remote (conceptual).");
        //     }
        //     else
        //     {
        //         Debug.Log($"[AddressablesBuildScript] Group '{group.Name}' identified as local (conceptual).");
        //     }
        // }
        // For this script, we rely on the existing group configurations.

        // Clean existing build artifacts before building new ones.
        // This is often a good practice to prevent stale data.
        // AddressableAssetSettings.CleanPlayerContent(settings.ActivePlayerDataBuilder); // Optional: depends on workflow
        // Debug.Log("[AddressablesBuildScript] Previous build content cleaned (if any).");


        // Start the build process
        AddressablesPlayerBuildResult buildResult = AddressableAssetSettings.BuildPlayerContent();
        // An alternative way to build if more control is needed:
        // AddressableAssetSettings.BuildPlayerContent(out AddressablesPlayerBuildResult result);
        // Or using a specific builder:
        // BuildScript.buildCompleted होता है जो परिणाम देता है
        // IInitializePlayerLoop.Initialize(); // Might be needed depending on context or if custom build steps are involved.
        // BuildScript.BuildAddressableAssets(out AddressablesPlayerBuildResult result, settings);


        if (string.IsNullOrEmpty(buildResult.Error))
        {
            Debug.Log($"[AddressablesBuildScript] Addressables content build successful. Output path: {buildResult.OutputPath}");

            // Invoke upload hook if anyone is subscribed
            if (OnBuildCompletedUploadHook != null)
            {
                Debug.Log("[AddressablesBuildScript] Invoking OnBuildCompletedUploadHook...");
                OnBuildCompletedUploadHook.Invoke(buildResult);
            }
            else
            {
                Debug.Log("[AddressablesBuildScript] No subscribers to OnBuildCompletedUploadHook.");
            }
        }
        else
        {
            Debug.LogError($"[AddressablesBuildScript] Addressables content build failed. Error: {buildResult.Error}");
        }
    }

    // Example of a method that could subscribe to the hook (for demonstration)
    // This would typically be in a different script responsible for uploading.
    /*
    [InitializeOnLoadMethod] // To auto-subscribe when Unity loads
    private static void RegisterUploadHook()
    {
        AddressablesBuildScript.OnBuildCompletedUploadHook += MyUploadImplementation;
        Debug.Log("[MyUploader] Registered for Addressables build completion hook.");
    }

    private static void MyUploadImplementation(AddressablesPlayerBuildResult result)
    {
        Debug.Log($"[MyUploader] Received build result. Output path: {result.OutputPath}. Ready to upload!");
        // Add actual upload logic here, e.g., to a CDN, FTP, etc.
        // For example, result.OutputPath will point to the folder containing the built bundles and catalog.
        // You might need to iterate through files in this directory.
    }
    */
}
