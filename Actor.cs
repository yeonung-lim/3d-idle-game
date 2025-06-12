using UnityEngine;

/// <summary>
/// Abstract class for game entities that can be targeted by the camera.
/// This class serves as a base for any object in the game world
/// that the CameraController might need to focus on or interact with.
/// </summary>
public abstract class Actor : MonoBehaviour
{
    // Example property that might be common to all actors:
    // /// <summary>
    // /// Gets the primary transform of this actor, typically used for position and rotation.
    // /// </summary>
    // public Transform PrimaryTransform => transform;

    // Currently, this class is primarily for type identification.
    // The CameraController will use actor.transform for its operations.
    // Future enhancements could include common actor properties like health, state, etc.,
    // which the camera might react to.
}
