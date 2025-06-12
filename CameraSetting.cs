using UnityEngine;

/// <summary>
/// Defines the configuration for the camera's behavior for a specific state or context.
/// Instances of this class hold various parameters that dictate how the camera should
/// position itself, move, and what optical properties it should use.
/// These settings can be grouped within a <see cref="CameraConstants"/> ScriptableObject.
/// </summary>
[System.Serializable]
public class CameraSetting
{
    [Header("Identification")]
    /// <summary>
    /// A unique key to identify this camera setting.
    /// This key is used by the <see cref="CameraController"/> to retrieve specific settings.
    /// Examples: "Default", "Dungeon01_Corridor", "BossFight_Phase1", "PlayerSprinting".
    /// </summary>
    public string keyName = "Default";

    [Header("Tracking")]
    /// <summary>
    /// The desired positional offset of the camera rig from its target.
    /// This offset is typically applied in the target's local space, meaning
    /// if the target rotates, the camera's offset position will rotate with it.
    /// For example, (0, 2, -5) would place the camera 2 units above and 5 units behind the target.
    /// </summary>
    public Vector3 positionOffset = new Vector3(0f, 5f, -10f);

    /// <summary>
    /// The desired rotational offset of the camera rig, applied after it has positioned itself
    /// relative to the target. These are Euler angles.
    /// This allows for fixed rotational adjustments, like looking down slightly at the target
    /// or maintaining a specific roll angle.
    /// For example, (10, 0, 0) might make the camera look down by 10 degrees.
    /// </summary>
    public Vector3 rotationOffset = Vector3.zero;

    /// <summary>
    /// The speed at which the camera rig follows its target's position.
    /// This value is typically used in Lerp or SmoothDamp functions to provide smooth tracking.
    /// Higher values result in faster, more responsive following.
    /// </summary>
    public float followSpeed = 5f;

    /// <summary>
    /// The speed at which the camera rig follows its target's rotation.
    /// This value is typically used in Slerp or SmoothDamp functions for smooth rotational tracking.
    /// Higher values result in faster, more responsive rotational alignment.
    /// </summary>
    public float rotationSpeed = 5f;

    [Header("Optics")]
    /// <summary>
    /// The field of view (FOV) for the camera when this setting is active.
    /// Measured in degrees, this determines the extent of the observable world seen by the camera.
    /// </summary>
    public float fieldOfView = 60f;

    /// <summary>
    /// The speed at which the camera's field of view (FOV) changes when transitioning
    /// to the FOV defined in this setting.
    /// Higher values result in a faster FOV transition.
    /// </summary>
    public float fovChangeSpeed = 3f;

    // Future settings can be added here, e.g.:
    // /// <summary>
    // /// Factor to determine how much the camera should look ahead in the direction of target movement.
    // /// A value of 0 means no look-ahead.
    // /// </summary>
    // public float lookAheadFactor = 0f;

    // /// <summary>
    // /// Determines if the camera should track the target's rotation.
    // /// If false, the camera might maintain a world-space rotation or a fixed offset from world north.
    // /// </summary>
    // public bool enableRotationTracking = true;
}
