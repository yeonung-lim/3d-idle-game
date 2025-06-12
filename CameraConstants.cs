using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// ScriptableObject designed to store a collection of <see cref="CameraSetting"/> configurations.
/// This allows camera behaviors to be defined and managed as assets in the Unity Editor,
/// making it easy to create, tweak, and assign different camera styles for various game contexts.
/// </summary>
[CreateAssetMenu(fileName = "CameraConstants", menuName = "Game/Camera Constants", order = 100)]
public class CameraConstants : ScriptableObject
{
    /// <summary>
    /// A list containing all the <see cref="CameraSetting"/> instances managed by this ScriptableObject.
    /// These can be configured in the Unity Inspector.
    /// </summary>
    public List<CameraSetting> settings = new List<CameraSetting>();

    /// <summary>
    /// Retrieves a <see cref="CameraSetting"/> by its unique <see cref="CameraSetting.keyName"/>.
    /// </summary>
    /// <param name="key">The <see cref="CameraSetting.keyName"/> of the <see cref="CameraSetting"/> to find.</param>
    /// <returns>
    /// The found <see cref="CameraSetting"/> if a match for <paramref name="key"/> is found.
    /// If not found, it logs a warning and attempts to return a setting with the key "Default".
    /// If "Default" is also not found, it returns the first setting in the list, if any.
    /// Returns <c>null</c> if no settings are available or no suitable fallback is found.
    /// </returns>
    public CameraSetting GetSetting(string key)
    {
        CameraSetting setting = settings.FirstOrDefault(s => s.keyName == key);
        if (setting == null)
        {
            Debug.LogWarning($"CameraSetting with key '{key}' not found in CameraConstants '{name}'. Attempting to return default setting.");
            // Attempt to return 'Default' setting as a fallback
            setting = settings.FirstOrDefault(s => s.keyName == "Default");
            if (setting == null)
            {
                // If 'Default' is not found, return the very first setting if any exist
                if (settings.Count > 0)
                {
                    Debug.LogWarning($"'Default' CameraSetting not found in '{name}'. Returning the first available setting: '{settings[0].keyName}'.");
                    return settings[0];
                }
                else
                {
                    Debug.LogError($"No CameraSettings defined in '{name}', and no 'Default' setting found. Cannot retrieve setting for key '{key}'.");
                    return null;
                }
            }
        }
        return setting;
    }

    /// <summary>
    /// Retrieves the default <see cref="CameraSetting"/>.
    /// The default setting is identified by having its <see cref="CameraSetting.keyName"/> as "Default".
    /// </summary>
    /// <returns>
    /// The default <see cref="CameraSetting"/> if found.
    /// If a setting with <see cref="CameraSetting.keyName"/> "Default" is not found,
    /// it logs a warning and returns the first setting in the list, if any.
    /// Logs an error and returns <c>null</c> if no settings are defined at all.
    /// </returns>
    public CameraSetting GetDefaultSetting()
    {
        CameraSetting defaultSetting = settings.FirstOrDefault(s => s.keyName == "Default");
        if (defaultSetting == null)
        {
            if (settings.Count > 0)
            {
                Debug.LogWarning($"Default CameraSetting (keyName 'Default') not found in '{name}'. Returning the first available setting: '{settings[0].keyName}'.");
                return settings[0]; // Return the first one as a fallback
            }
            Debug.LogError($"No CameraSettings defined in '{name}', and no 'Default' setting found. Cannot retrieve a default setting.");
            return null;
        }
        return defaultSetting;
    }
}
