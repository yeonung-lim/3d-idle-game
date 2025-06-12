using UnityEngine;

/// <summary>
/// Manages camera shake effects.
/// This class is responsible for applying positional and rotational disturbances
/// to a specified Transform (typically a camera rig) to simulate shaking.
/// It's designed to be instantiated and controlled by the <see cref="CameraController"/>.
/// </summary>
public class CameraShaker
{
    private Transform cameraRig;
    private float baseShakeDuration = 0f; // Renamed for clarity from shakeDuration
    private float baseShakeIntensity = 0f; // Renamed for clarity from shakeIntensity
    private float currentShakeTimeRemaining = 0f; // Renamed for clarity from currentShakeDuration

    private Vector3 originalLocalPosition;
    private Quaternion originalLocalRotation;

    /// <summary>
    /// Initializes a new instance of the <see cref="CameraShaker"/> class.
    /// </summary>
    /// <param name="rigTransform">The transform of the CameraRig that this shaker will manipulate.
    /// This transform will have its local position and rotation modified during shakes.</param>
    public CameraShaker(Transform rigTransform)
    {
        this.cameraRig = rigTransform;
    }

    /// <summary>
    /// Starts a camera shake effect.
    /// If a shake is already in progress, this new shake will override the existing one,
    /// resetting the camera rig to its pre-shake state before applying the new shake.
    /// </summary>
    /// <param name="duration">The total duration (in seconds) the shake effect should last.</param>
    /// <param name="intensity">The strength or magnitude of the shake. Higher values result in more pronounced shaking.</param>
    public void Shake(float duration, float intensity)
    {
        if (this.cameraRig == null)
        {
            Debug.LogError("CameraShaker: CameraRig transform is not set. Cannot initiate shake.");
            return;
        }

        // If a shake is already in progress, reset to the original pre-shake state first.
        if (currentShakeTimeRemaining > 0)
        {
            ResetShakeState();
        }

        this.baseShakeDuration = duration;
        this.baseShakeIntensity = intensity;
        this.currentShakeTimeRemaining = duration;

        // Store the rig's current local transform as the baseline for the shake.
        this.originalLocalPosition = cameraRig.localPosition;
        this.originalLocalRotation = cameraRig.localRotation;
    }

    /// <summary>
    /// Updates the shake effect. This method should be called every frame (e.g., in Update or LateUpdate)
    /// by the owning <see cref="CameraController"/> when a shake is active.
    /// It applies random positional and rotational offsets to the <see cref="cameraRig"/>.
    /// </summary>
    /// <param name="deltaTime">The time elapsed since the last frame, used for decrementing shake duration.</param>
    /// <returns><c>true</c> if the shake is still active after this update; <c>false</c> if the shake has just finished.</returns>
    public bool UpdateShake(float deltaTime)
    {
        if (cameraRig == null) return false;

        if (currentShakeTimeRemaining > 0)
        {
            // Calculate current intensity with a linear falloff over the duration of the shake.
            float currentIntensity = baseShakeIntensity * (currentShakeTimeRemaining / baseShakeDuration);

            // Apply random positional offset.
            // Uses Random.insideUnitSphere for a 3D shake. For more constrained shakes (e.g., 2D),
            // one might zero out certain axes or use Random.insideUnitCircle.
            Vector3 randomPositionOffset = Random.insideUnitSphere * currentIntensity;
            cameraRig.localPosition = originalLocalPosition + randomPositionOffset;

            // Apply random rotational offset.
            // This example applies small random pitch/yaw and a stronger random roll,
            // which can be typical for impact shakes. Adjust factors as needed.
            cameraRig.localRotation = Quaternion.Euler(
                originalLocalRotation.eulerAngles.x + Random.Range(-currentIntensity * 0.2f, currentIntensity * 0.2f), // Small pitch shake
                originalLocalRotation.eulerAngles.y + Random.Range(-currentIntensity * 0.2f, currentIntensity * 0.2f), // Small yaw shake
                originalLocalRotation.eulerAngles.z + Random.Range(-currentIntensity, currentIntensity) * 1.0f      // Stronger roll shake
            );

            currentShakeTimeRemaining -= deltaTime;
            if (currentShakeTimeRemaining <= 0)
            {
                ResetShakeState();
                return false; // Shake ended this frame.
            }
            return true; // Shake is ongoing.
        }
        return false; // Shake is not active.
    }

    /// <summary>
    /// Resets the <see cref="cameraRig"/>'s transform to its original local position and rotation
    /// that were recorded before the current shake started. Also clears internal shake timers and intensity.
    /// </summary>
    private void ResetShakeState()
    {
        if (cameraRig != null)
        {
            cameraRig.localPosition = originalLocalPosition;
            cameraRig.localRotation = originalLocalRotation;
        }
        currentShakeTimeRemaining = 0f;
        baseShakeDuration = 0f;
        baseShakeIntensity = 0f;
    }

    /// <summary>
    /// Checks if a camera shake is currently in progress.
    /// </summary>
    /// <returns><c>true</c> if <see cref="currentShakeTimeRemaining"/> is greater than zero; otherwise, <c>false</c>.</returns>
    public bool IsShaking()
    {
        return currentShakeTimeRemaining > 0f;
    }
}
