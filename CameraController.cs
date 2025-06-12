using UnityEngine;
using System;

/// <summary>
/// Controls the game camera, managing its behavior including following targets,
/// transitioning between targets, playing animations, applying shake effects,
/// and adjusting optical properties like Field of View (FOV).
/// It operates on a "CameraRig" transform, which should be a parent of the actual Unity Camera.
/// This class is designed to be controlled by an external manager that calls its <see cref="CustomUpdate"/> method.
/// </summary>
public class CameraController : MonoBehaviour
{
    /// <summary>
    /// Defines the various operational modes of the CameraController.
    /// </summary>
    public enum CameraMode
    {
        /// <summary>
        /// The camera is idle, not actively following a target or performing any special action.
        /// It might maintain its current position or execute a subtle idle behavior (if implemented).
        /// </summary>
        Idle,
        /// <summary>
        /// The camera is actively following its <see cref="currentTarget"/> based on the <see cref="activeSetting"/>.
        /// </summary>
        Following,
        /// <summary>
        /// The camera is smoothly transitioning from its previous state to a new target or position.
        /// </summary>
        TransitioningToTarget,
        /// <summary>
        /// The camera's movement and properties are being controlled by an AnimationClip via its <see cref="cameraAnimator"/>.
        /// </summary>
        Animating,
        /// <summary>
        /// The camera is undergoing a shake effect, managed by the <see cref="cameraShaker"/>.
        /// </summary>
        Shaking,
        /// <summary>
        /// The camera is focused on a temporary target and will return to its original target after a specified duration.
        /// </summary>
        HoldingTemporaryTarget
    }

    [Header("Core Components")]
    private Camera mainCamera;
    private Transform cameraRig;
    private Animator cameraAnimator;
    private CameraShaker cameraShaker;

    [Header("Targeting")]
    private Transform currentTarget;
    private CameraSetting activeSetting;

    [Header("State")]
    private CameraMode currentMode = CameraMode.Idle;
    private CameraMode previousModeBeforeSpecialState = CameraMode.Following;

    // Settings
    private CameraConstants cameraConstantsSource;
    private string currentSettingsKey;

    // FOV
    private float baseTargetFOV;
    private float temporaryFovOffset;
    private float currentActualFOV; // The FOV the camera is lerping towards (base + temporary)

    // Target Switching State
    private Transform transitionFromTarget;
    private Transform transitionToTarget;
    private Vector3 transitionStartPosition;
    private Quaternion transitionStartRotation;
    private float transitionDuration;
    private float transitionProgress;
    private Action onTransitionCompleteCallback;

    // Temporary Target Switching State
    private Transform originalTargetBeforeTemporarySwitch;
    private float temporaryTargetHoldTimer;
    private float temporaryTargetOutLerpTime;
    private Action temporaryTargetReturnCallback;

    // Animation State
    private float animationDurationRemaining;
    private Action onAnimationCompleteCallback;

    // Temporary Offset State
    private Vector3 additionalPositionOffset;
    private Quaternion additionalRotationOffset;
    private float temporaryOffsetDurationRemaining;
    private bool temporaryOffsetActive = false;

    /// <summary>
    /// Helper struct to return both position and rotation from calculation methods.
    /// </summary>
    private struct Vector3_Quaternion { public Vector3 Position; public Quaternion Rotation; }


    #region Initialization
    /// <summary>
    /// Initializes the CameraController with essential components and initial settings.
    /// This method must be called before the CameraController can function correctly.
    /// </summary>
    /// <param name="cameraToControl">The main Unity <see cref="Camera"/> component that this controller will manage.</param>
    /// <param name="rigTransform">The <see cref="Transform"/> of the CameraRig object. The <paramref name="cameraToControl"/> should ideally be a child of this rig.</param>
    /// <param name="rigAnimator">The <see cref="Animator"/> component attached to the <paramref name="rigTransform"/>. Can be <c>null</c> if animations are not used.</param>
    /// <param name="initialConstants">The <see cref="CameraConstants"/> ScriptableObject containing predefined camera settings.</param>
    /// <param name="initialSettingsKey">The key for the initial <see cref="CameraSetting"/> to load from <paramref name="initialConstants"/>. Defaults to "Default".</param>
    public void Initialize(Camera cameraToControl, Transform rigTransform, Animator rigAnimator, CameraConstants initialConstants, string initialSettingsKey = "Default")
    {
        if (cameraToControl == null || rigTransform == null || initialConstants == null)
        {
            Debug.LogError("CameraController: MainCamera, CameraRig, or CameraConstants cannot be null. Initialization failed.");
            enabled = false; // Disable this component if essential references are missing
            return;
        }

        mainCamera = cameraToControl;
        cameraRig = rigTransform;
        cameraAnimator = rigAnimator;
        cameraConstantsSource = initialConstants;

        cameraShaker = new CameraShaker(cameraRig);

        if (!LoadSettings(initialSettingsKey))
        {
            Debug.LogError($"CameraController: Failed to load initial settings with key '{initialSettingsKey}'. Attempting fallback to any 'Default' or first available setting.");
            activeSetting = cameraConstantsSource.GetDefaultSetting(); // GetDefaultSetting handles internal fallbacks
            if (activeSetting == null)
            {
                 Debug.LogError("CameraController: No default or fallback settings found in CameraConstants. Controller will be disabled.");
                 enabled = false;
                 return;
            }
            Debug.LogWarning($"CameraController: Using fallback settings '{activeSetting.keyName}'.");
            currentSettingsKey = activeSetting.keyName;
            baseTargetFOV = activeSetting.fieldOfView; // Ensure baseTargetFOV is set from fallback
        }

        currentActualFOV = baseTargetFOV;
        mainCamera.fieldOfView = currentActualFOV; // Apply initial FOV immediately

        // Ensure camera is a child of the rig for predictable local space manipulations
        if (mainCamera.transform.parent != cameraRig)
        {
            Debug.LogWarning("CameraController: MainCamera is not a child of the CameraRig. Reparenting for correct behavior.");
            mainCamera.transform.SetParent(cameraRig);
            mainCamera.transform.localPosition = Vector3.zero;
            mainCamera.transform.localRotation = Quaternion.identity;
        }

        currentMode = CameraMode.Idle; // Start in Idle mode
        Debug.Log($"CameraController Initialized. Animator {(cameraAnimator != null ? "found" : "not found/assigned")}. CameraShaker initialized.");
    }

    /// <summary>
    /// Loads a <see cref="CameraSetting"/> configuration from the <see cref="cameraConstantsSource"/>.
    /// Updates <see cref="activeSetting"/>, <see cref="currentSettingsKey"/>, and <see cref="baseTargetFOV"/>.
    /// </summary>
    /// <param name="key">The <see cref="CameraSetting.keyName"/> of the <see cref="CameraSetting"/> to load.</param>
    /// <returns><c>true</c> if the settings were loaded successfully; <c>false</c> otherwise.</returns>
    public bool LoadSettings(string key)
    {
        if (cameraConstantsSource == null) {
            Debug.LogError("CameraController: CameraConstants source is not set. Cannot load settings.");
            return false;
        }
        CameraSetting newSettings = cameraConstantsSource.GetSetting(key);
        if (newSettings != null)
        {
            activeSetting = newSettings;
            currentSettingsKey = key;
            baseTargetFOV = activeSetting.fieldOfView; // Update base FOV from the new settings
            UpdateCurrentActualFOV(); // Recalculate the actual FOV target including any temporary offsets
            Debug.Log($"CameraController: Loaded settings '{key}'.");
            return true;
        }
        Debug.LogWarning($"CameraController: Settings with key '{key}' not found in CameraConstants. Current settings remain unchanged.");
        return false;
    }
    #endregion

    #region Targeting & Transitions
    /// <summary>
    /// Sets the primary target for the camera to follow.
    /// This is an immediate switch. For smooth transitions, use <see cref="SwitchTarget(Transform, float, Action)"/>.
    /// If the camera is <see cref="CameraMode.Idle"/> or <see cref="CameraMode.Following"/>, it snaps to the new target's view.
    /// </summary>
    /// <param name="newTarget">The <see cref="Transform"/> of the new target. Can be an <see cref="Actor"/>'s transform or any other Transform.</param>
    public void SetTarget(Transform newTarget)
    {
        if (newTarget == null) {
            Debug.LogWarning("CameraController: SetTarget called with null. Camera will become Idle.");
            currentTarget = null;
            currentMode = CameraMode.Idle;
            return;
        }

        currentTarget = newTarget;
        if (currentMode == CameraMode.Idle || currentMode == CameraMode.Following)
        {
            currentMode = CameraMode.Following;
            // Snap to the target's calculated position and rotation immediately
            if (activeSetting != null && currentTarget != null) {
                Vector3_Quaternion pq = CalculateEffectivePositionRotation(currentTarget, activeSetting);
                cameraRig.position = pq.Position;
                cameraRig.rotation = pq.Rotation;
            }
        }
        Debug.Log($"CameraController: Target directly set to '{newTarget.name}'. Mode: {currentMode}");
    }

    /// <summary>
    /// Gets the current primary target <see cref="Transform"/> that the camera is configured to follow.
    /// </summary>
    /// <returns>The current target's <see cref="Transform"/>, or <c>null</c> if no target is set.</returns>
    public Transform GetCurrentTarget() => currentTarget;

    /// <summary>
    /// Switches the camera's target to a new <see cref="Transform"/> with a smooth transition.
    /// </summary>
    /// <param name="newTarget">The new target's <see cref="Transform"/>. Can be <c>null</c> to transition to a state without a specific target focus.</param>
    /// <param name="duration">The time (in seconds) the transition should take. If zero or negative, it behaves like <see cref="SetTarget"/>.</param>
    /// <param name="onComplete">An optional <see cref="Action"/> to invoke once the transition is finished.</param>
    public void SwitchTarget(Transform newTarget, float duration, Action onComplete = null)
    {
        if (newTarget == null && currentTarget == null) {
            Debug.LogWarning("CameraController: SwitchTarget called with newTarget null and currentTarget is also null. Aborting transition.");
            onComplete?.Invoke(); return;
        }
        if (newTarget == currentTarget && currentMode != CameraMode.TransitioningToTarget) {
             Debug.LogWarning("CameraController: SwitchTarget called with newTarget same as currentTarget and not already transitioning. Aborting.");
            onComplete?.Invoke(); return;
        }

        if (duration <= 0) { SetTarget(newTarget); onComplete?.Invoke(); return; }

        StorePreviousModeForSpecialState(); // Store mode to return to after transition (usually Following or Idle)

        currentMode = CameraMode.TransitioningToTarget;
        transitionFromTarget = currentTarget;
        transitionToTarget = newTarget;
        transitionDuration = duration;
        transitionProgress = 0f;
        onTransitionCompleteCallback = onComplete;
        transitionStartPosition = cameraRig.position;
        transitionStartRotation = cameraRig.rotation;
        Debug.Log($"CameraController: Starting transition from '{transitionFromTarget?.name ?? "null"}' to '{newTarget?.name ?? "null"}' over {duration}s.");
    }

    /// <summary>
    /// Temporarily switches the camera's focus to a specified target, holds for a duration, and then smoothly returns to the original target.
    /// </summary>
    /// <param name="temporaryTarget">The <see cref="Transform"/> to focus on temporarily.</param>
    /// <param name="inLerpTime">The time (in seconds) for the transition to the <paramref name="temporaryTarget"/>.</param>
    /// <param name="holdDuration">The time (in seconds) to stay focused on the <paramref name="temporaryTarget"/>.</param>
    /// <param name="outLerpTime">The time (in seconds) for the transition back to the original target.</param>
    /// <param name="onReturnComplete">An optional <see cref="Action"/> to invoke once the camera has returned to the original target.</param>
    public void SwitchTargetTemporary(Transform temporaryTarget, float inLerpTime, float holdDuration, float outLerpTime, Action onReturnComplete = null)
    {
        if (temporaryTarget == null) {
            Debug.LogWarning("CameraController: SwitchTargetTemporary called with a null temporaryTarget. Aborting.");
            onReturnComplete?.Invoke();
            return;
        }

        originalTargetBeforeTemporarySwitch = currentTarget; // Store the target to return to
        temporaryTargetOutLerpTime = outLerpTime;
        temporaryTargetReturnCallback = onReturnComplete;

        // This will set previousModeBeforeSpecialState before switching to TransitioningToTarget
        SwitchTarget(temporaryTarget, inLerpTime, () => {
            // This callback is executed once the camera has reached the temporary target
            currentMode = CameraMode.HoldingTemporaryTarget;
            temporaryTargetHoldTimer = holdDuration;
            Debug.Log($"CameraController: Reached temporary target '{temporaryTarget.name}'. Holding for {holdDuration}s.");
        });
    }
    #endregion

    #region Animation Control
    /// <summary>
    /// Plays a camera animation using the <see cref="cameraAnimator"/>.
    /// The camera rig's position and rotation will be controlled by the animation.
    /// </summary>
    /// <param name="animationName">The name of the animation state to play in the <see cref="cameraAnimator"/>.</param>
    /// <param name="duration">The expected duration (in seconds) of the animation. Used to determine when to revert to the previous camera mode.</param>
    /// <param name="onComplete">An optional <see cref="Action"/> to invoke once the animation duration is met.</param>
    public void PlayAnimation(string animationName, float duration, Action onComplete = null)
    {
        if (cameraAnimator == null) {
            Debug.LogWarning("CameraController: CameraAnimator is null. Cannot play animation.");
            onComplete?.Invoke(); return;
        }
        if (duration <= 0) {
             Debug.LogWarning("CameraController: Animation duration must be positive.");
            onComplete?.Invoke(); return;
        }

        StorePreviousModeForSpecialState();
        currentMode = CameraMode.Animating;
        animationDurationRemaining = duration;
        onAnimationCompleteCallback = onComplete;

        cameraAnimator.Play(animationName, -1, 0f); // Play the animation from the beginning
        Debug.Log($"CameraController: Playing animation '{animationName}' for {duration}s. Will return to mode {previousModeBeforeSpecialState} after.");
    }
    #endregion

    #region Camera Shake Control
    /// <summary>
    /// Starts a camera shake effect using the <see cref="cameraShaker"/>.
    /// The shake is applied to the <see cref="cameraRig"/>.
    /// </summary>
    /// <param name="duration">How long (in seconds) the shake effect should last.</param>
    /// <param name="intensity">The strength or magnitude of the shake.</param>
    public void StartShake(float duration, float intensity)
    {
        if (cameraShaker == null) {
            Debug.LogWarning("CameraController: CameraShaker is null. Cannot start shake.");
            return;
        }
        if (duration <= 0 || intensity <= 0) {
            Debug.LogWarning("CameraController: Shake duration and intensity must be positive.");
            return;
        }

        StorePreviousModeForSpecialState();
        currentMode = CameraMode.Shaking;
        cameraShaker.Shake(duration, intensity);
        Debug.Log($"CameraController: Starting shake for {duration}s with intensity {intensity}. Will return to mode {previousModeBeforeSpecialState} after.");
    }
    #endregion

    #region FOV and Offset Adjustments
    /// <summary>
    /// Sets the base target Field of View (FOV) for the camera.
    /// The camera will smoothly transition to this FOV based on <see cref="CameraSetting.fovChangeSpeed"/>.
    /// This overrides the FOV from the active <see cref="CameraSetting"/> until <see cref="LoadSettings(string)"/> is called or this method is used again.
    /// </summary>
    /// <param name="newBaseFOV">The desired base Field of View in degrees.</param>
    public void SetBaseTargetFOV(float newBaseFOV)
    {
        baseTargetFOV = newBaseFOV;
        UpdateCurrentActualFOV(); // Recalculate the overall target FOV
    }

    /// <summary>
    /// Applies a temporary, additive offset to the camera's FOV, position, and rotation.
    /// These offsets are applied on top of the current camera behavior (e.g., following, transitioning).
    /// The offsets will remain active for the specified duration or indefinitely if duration is zero or negative.
    /// </summary>
    /// <param name="posOffset">Additional position offset, applied in the <see cref="cameraRig"/>'s local space after other calculations.</param>
    /// <param name="rotOffset">Additional rotation offset, applied as a local rotation to the <see cref="cameraRig"/>.</param>
    /// <param name="fovOffset">Additional FOV offset, added to the <see cref="baseTargetFOV"/>.</param>
    /// <param name="duration">How long (in seconds) the temporary offsets should last. If 0 or less, the offset is indefinite until <see cref="ClearTemporaryOffset"/> is called.</param>
    public void ApplyTemporaryOffset(Vector3 posOffset, Quaternion rotOffset, float fovOffset, float duration)
    {
        additionalPositionOffset = posOffset;
        additionalRotationOffset = rotOffset;
        temporaryFovOffset = fovOffset;
        temporaryOffsetDurationRemaining = duration;
        temporaryOffsetActive = true;

        UpdateCurrentActualFOV(); // Recalculate FOV target immediately

        if (duration <= 0) {
            Debug.Log("CameraController: Applied indefinite temporary offset. Clear manually with ClearTemporaryOffset().");
        } else {
            Debug.Log($"CameraController: Applied temporary offset for {duration}s.");
        }
    }

    /// <summary>
    /// Clears any active temporary offsets for FOV, position, and rotation, reverting them to zero/identity.
    /// Also recalculates the <see cref="currentActualFOV"/>.
    /// </summary>
    public void ClearTemporaryOffset()
    {
        additionalPositionOffset = Vector3.zero;
        additionalRotationOffset = Quaternion.identity;
        temporaryFovOffset = 0f;
        temporaryOffsetDurationRemaining = 0f;
        temporaryOffsetActive = false;
        UpdateCurrentActualFOV(); // Recalculate FOV target
        Debug.Log("CameraController: Cleared temporary offsets.");
    }

    /// <summary>
    /// Updates <see cref="currentActualFOV"/> based on <see cref="baseTargetFOV"/> and <see cref="temporaryFovOffset"/>.
    /// This is the value the camera's FOV will lerp towards.
    /// </summary>
    private void UpdateCurrentActualFOV()
    {
        currentActualFOV = baseTargetFOV + temporaryFovOffset;
    }
    #endregion

    #region Update Loop
    /// <summary>
    /// Main update loop for the CameraController.
    /// This method should be called consistently (e.g., every frame from a global Update manager)
    /// to drive the camera's behavior based on its current mode and settings.
    /// </summary>
    /// <param name="deltaTime">The time elapsed since the last update call, used for smoothing and timers.</param>
    public void CustomUpdate(float deltaTime)
    {
        if (mainCamera == null || cameraRig == null) {
            // Essential components are missing; do not proceed.
            // This should ideally not happen if Initialize was called correctly and succeeded.
            return;
        }

        // Handle loss of target if not in a mode that can operate without one
        if (currentTarget == null &&
            currentMode != CameraMode.Animating &&
            currentMode != CameraMode.Idle &&
            currentMode != CameraMode.TransitioningToTarget && // Transitioning to null is allowed
            currentMode != CameraMode.Shaking) // Shaking can occur without a specific target
        {
            Debug.LogWarning("CameraController: Current target is null. Setting mode to Idle.");
            currentMode = CameraMode.Idle;
        }

        // Update duration for temporary offsets
        if (temporaryOffsetActive && temporaryOffsetDurationRemaining > 0)
        {
            temporaryOffsetDurationRemaining -= deltaTime;
            if (temporaryOffsetDurationRemaining <= 0) {
                ClearTemporaryOffset(); // Auto-clear when duration expires
            }
        }

        // Smoothly adjust the camera's actual FOV towards the calculated target FOV
        if (Mathf.Abs(mainCamera.fieldOfView - currentActualFOV) > 0.01f)
        {
            float fovSpeed = (activeSetting != null) ? activeSetting.fovChangeSpeed : 3.0f; // Use default if no activeSetting
            mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, currentActualFOV, fovSpeed * deltaTime);
        }

        // Mode-specific behavior updates
        switch (currentMode)
        {
            case CameraMode.Idle:
                HandleIdleMode(deltaTime);
                break;
            case CameraMode.Following:
                HandleFollowingMode(deltaTime);
                break;
            case CameraMode.TransitioningToTarget:
                HandleTransitioningMode(deltaTime);
                break;
            case CameraMode.HoldingTemporaryTarget:
                HandleHoldingTemporaryTargetMode(deltaTime);
                break;
            case CameraMode.Animating:
                HandleAnimatingMode(deltaTime);
                break;
            case CameraMode.Shaking:
                HandleShakingMode(deltaTime);
                break;
        }
    }

    /// <summary>Calculates the desired world position and rotation for the camera rig based on the target and settings.</summary>
    private Vector3_Quaternion CalculateEffectivePositionRotation(Transform target, CameraSetting settings)
    {
        if (target == null || settings == null) {
            // Fallback to current rig transform if target or settings are invalid
            return new Vector3_Quaternion { Position = cameraRig.position, Rotation = cameraRig.rotation };
        }
        // Position: Start at target's position, then apply offset in target's local space, then transform to world space.
        Vector3 basePosition = target.position + target.rotation * settings.positionOffset;
        // Rotation: Start with target's rotation, then apply Euler angle offset.
        Quaternion baseRotation = target.rotation * Quaternion.Euler(settings.rotationOffset);

        return new Vector3_Quaternion { Position = basePosition, Rotation = baseRotation };
    }

    /// <summary>Applies any active additional position and rotation offsets to the camera rig.</summary>
    private void ApplyAdditionalOffsetsToRig()
    {
        // Position offset is applied in the rig's local space.
        // If cameraRig is a root object for camera logic, this effectively acts as a world-space offset relative to its calculation.
        // If cameraRig is parented, this is local to that parent.
        cameraRig.localPosition += additionalPositionOffset;

        // Rotation offset is applied as a further local rotation.
        cameraRig.localRotation *= additionalRotationOffset;
    }

    /// <summary>Stores the current camera mode before switching to a special state like Animating, Shaking, or Transitioning.</summary>
    private void StorePreviousModeForSpecialState()
    {
        // Only store "base" modes (Idle, Following). If already in a special state,
        // previousModeBeforeSpecialState should already hold the true base mode we want to return to.
        if (currentMode == CameraMode.Idle || currentMode == CameraMode.Following)
        {
            previousModeBeforeSpecialState = currentMode;
        }
        // If currentMode is TransitioningToTarget, and another special state request comes in (e.g. Shake),
        // the 'previousModeBeforeSpecialState' should ideally be what the transition was GOING to (e.g. Following).
        // This is implicitly handled as SwitchTarget itself calls this, so 'previousModeBeforeSpecialState'
        // would have been set to the mode active *before* the transition started.
    }

    /// <summary>Handles camera behavior when in Idle mode.</summary>
    private void HandleIdleMode(float deltaTime)
    {
        // Camera may be static, or could have subtle idle movements (e.g., gentle drift).
        // Temporary offsets should still apply if active.
        if (temporaryOffsetActive) {
            // Applying offsets in Idle assumes the camera has a meaningful "current" transform to offset from.
            // If Idle means truly static, this might need care. Here, it offsets the current rig transform.
            ApplyAdditionalOffsetsToRig();
        }
    }

    /// <summary>Handles camera behavior when in Following mode.</summary>
    private void HandleFollowingMode(float deltaTime)
    {
        if (currentTarget == null || activeSetting == null) {
            // If target or settings are lost, revert to Idle to prevent errors.
            currentMode = CameraMode.Idle;
            return;
        }
        Vector3_Quaternion basePQ = CalculateEffectivePositionRotation(currentTarget, activeSetting);

        cameraRig.position = Vector3.Lerp(cameraRig.position, basePQ.Position, activeSetting.followSpeed * deltaTime);
        cameraRig.rotation = Quaternion.Slerp(cameraRig.rotation, basePQ.Rotation, activeSetting.rotationSpeed * deltaTime);

        if (temporaryOffsetActive) ApplyAdditionalOffsetsToRig(); // Apply additive offsets on top
    }

    /// <summary>Handles camera behavior when in TransitioningToTarget mode.</summary>
    private void HandleTransitioningMode(float deltaTime)
    {
        transitionProgress += deltaTime / transitionDuration;
        transitionProgress = Mathf.Clamp01(transitionProgress);

        Vector3_Quaternion endTransitionPQ;
        CameraSetting settingsForTransition = activeSetting;

        if (settingsForTransition == null && transitionToTarget != null) {
            Debug.LogWarning("CameraController: activeSetting is null during transition with a valid target. Using default CameraSetting parameters for target calculation.");
            settingsForTransition = new CameraSetting(); // Use a temporary default to avoid null refs in CalculateEffectivePositionRotation
        }

        if (transitionToTarget != null) {
            // Calculate the desired end state based on the target we are transitioning TO.
            endTransitionPQ = CalculateEffectivePositionRotation(transitionToTarget, settingsForTransition);
        } else {
            // Transitioning to a null target (e.g., camera moves to a fixed point or cinematic pose).
            // If transitionFromTarget existed, use its perspective as the "end point" but with current settings.
            // Otherwise, the camera just stays where the transition started. This behavior might need specific design.
            if(transitionFromTarget != null && settingsForTransition != null){
                 endTransitionPQ = CalculateEffectivePositionRotation(transitionFromTarget, settingsForTransition);
                 Debug.LogWarning("CameraController: Transitioning to a null target. Aiming for last known target's perspective with current settings.");
            } else {
                 endTransitionPQ = new Vector3_Quaternion { Position = transitionStartPosition, Rotation = transitionStartRotation };
                 Debug.LogWarning("CameraController: Transitioning to a null target with no prior target context. Holding start of transition pose.");
            }
        }

        cameraRig.position = Vector3.Lerp(transitionStartPosition, endTransitionPQ.Position, transitionProgress);
        cameraRig.rotation = Quaternion.Slerp(transitionStartRotation, endTransitionPQ.Rotation, transitionProgress);

        if (temporaryOffsetActive) ApplyAdditionalOffsetsToRig(); // Apply additive offsets on top

        if (transitionProgress >= 1.0f)
        {
            currentTarget = transitionToTarget; // Finalize the target switch
            currentMode = (currentTarget == null && previousModeBeforeSpecialState != CameraMode.Idle) ? CameraMode.Idle : previousModeBeforeSpecialState;
             if (currentMode == CameraMode.TransitioningToTarget) currentMode = CameraMode.Following; // Safety: Ensure we exit Transitioning mode.

            Debug.Log($"CameraController: Transition to '{currentTarget?.name ?? "null"}' complete. New mode: {currentMode}");

            Action callback = onTransitionCompleteCallback;
            onTransitionCompleteCallback = null; // Clear callback after invoking
            callback?.Invoke();
        }
    }

    /// <summary>Handles camera behavior when in HoldingTemporaryTarget mode.</summary>
    private void HandleHoldingTemporaryTargetMode(float deltaTime)
    {
        // currentTarget is the temporaryTarget during this mode.
        if (currentTarget != null && activeSetting != null) {
            HandleFollowingMode(deltaTime); // Continue to follow the temporary target smoothly
        }

        temporaryTargetHoldTimer -= deltaTime;
        if (temporaryTargetHoldTimer <= 0)
        {
            Debug.Log($"CameraController: Hold timer for temporary target '{currentTarget?.name ?? "null"}' expired. Returning to '{originalTargetBeforeTemporarySwitch?.name ?? "null"}'.");
            Transform targetToReturnTo = originalTargetBeforeTemporarySwitch;
            originalTargetBeforeTemporarySwitch = null;
            Action returnCallback = temporaryTargetReturnCallback;
            temporaryTargetReturnCallback = null;

            // SwitchTarget will set the mode to TransitioningToTarget and handle previousModeBeforeSpecialState.
            SwitchTarget(targetToReturnTo, temporaryTargetOutLerpTime, returnCallback);
        }
    }

    /// <summary>Handles camera behavior when in Animating mode.</summary>
    private void HandleAnimatingMode(float deltaTime)
    {
        if (animationDurationRemaining > 0)
        {
            animationDurationRemaining -= deltaTime;
            if (animationDurationRemaining <= 0)
            {
                currentMode = previousModeBeforeSpecialState;
                // Safety: if previousModeBeforeSpecialState was somehow Animating, default to Following.
                if (currentMode == CameraMode.Animating) currentMode = CameraMode.Following;

                Debug.Log($"CameraController: Animation finished. Reverting to mode {currentMode}.");
                Action callback = onAnimationCompleteCallback;
                onAnimationCompleteCallback = null; // Clear callback
                callback?.Invoke();

                // After animation, snap camera to current target's view if returning to Following mode.
                if(currentMode == CameraMode.Following && currentTarget != null && activeSetting != null)
                {
                    Vector3_Quaternion pq = CalculateEffectivePositionRotation(currentTarget, activeSetting);
                    cameraRig.position = pq.Position;
                    cameraRig.rotation = pq.Rotation;
                }
            }
        }
        // During animation, the Animator component on cameraRig is assumed to be updating its transform.
        // Temporary offsets are NOT applied during animation by default, to give Animator full control.
    }

    /// <summary>Handles camera behavior when in Shaking mode.</summary>
    private void HandleShakingMode(float deltaTime)
    {
        if (cameraShaker == null) {
            Debug.LogError("CameraController: CameraShaker is null in HandleShakingMode. Reverting mode.");
            currentMode = previousModeBeforeSpecialState; return;
        }

        // CameraShaker directly manipulates the cameraRig's local transform based on its stored original state + noise.
        bool isShaking = cameraShaker.UpdateShake(deltaTime);

        if (!isShaking) // Shake has finished
        {
            currentMode = previousModeBeforeSpecialState;
            // Safety: if previousModeBeforeSpecialState was somehow Shaking, default to Following.
            if (currentMode == CameraMode.Shaking) currentMode = CameraMode.Following;
            Debug.Log($"CameraController: Shake finished. Reverting to mode {currentMode}.");

            // After shake, CameraShaker's ResetShakeState should have returned rig to its pre-shake transform.
            // If returning to Following mode, snap to ensure correct alignment with the target.
            if(currentMode == CameraMode.Following && currentTarget != null && activeSetting != null)
            {
                Vector3_Quaternion pq = CalculateEffectivePositionRotation(currentTarget, activeSetting);
                cameraRig.position = pq.Position;
                cameraRig.rotation = pq.Rotation;
            } else if (currentMode == CameraMode.Idle) {
                 // If returning to Idle, camera just stays where shaker left it (its original pre-shake state).
            }
        }
        // Temporary offsets are generally NOT applied during shake by default, as shake takes full control of rig's local transform.
        // FOV offsets, however, are still applied via the main FOV update logic.
    }
    #endregion

    #region Public Accessors & Modifiers
    /// <summary>Gets the current operational mode of the camera.</summary>
    /// <returns>The current <see cref="CameraMode"/>.</returns>
    public CameraMode GetCurrentMode() => currentMode;

    /// <summary>Gets the currently active <see cref="CameraSetting"/> instance.</summary>
    /// <returns>The active <see cref="CameraSetting"/>, or <c>null</c> if none is active.</returns>
    public CameraSetting GetActiveSetting() => activeSetting;

    /// <summary>Gets the key name of the currently active <see cref="CameraSetting"/>.</summary>
    /// <returns>The <see cref="CameraSetting.keyName"/> of the active setting, or <c>null</c> or an empty string if none is active.</returns>
    public string GetCurrentSettingsKey() => currentSettingsKey;

    /// <summary>Gets the current base target Field of View (FOV) in degrees, before temporary offsets.</summary>
    /// <returns>The base target FOV.</returns>
    public float GetCurrentBaseFOV() => baseTargetFOV;

    /// <summary>Gets the current temporary FOV offset in degrees.</summary>
    /// <returns>The temporary FOV offset.</returns>
    public float GetCurrentTemporaryFOVOffset() => temporaryFovOffset;

    /// <summary>Gets the actual Field of View (FOV) of the Unity Camera component.</summary>
    /// <returns>The current FOV of <see cref="mainCamera"/>.</returns>
    public float GetCurrentActualFOV() => mainCamera != null ? mainCamera.fieldOfView : 0f;

    /// <summary>
    /// Checks if the <see cref="currentTarget"/> is currently visible within the <see cref="mainCamera"/>'s viewport.
    /// This is a basic frustum check.
    /// </summary>
    /// <returns><c>true</c> if the target is visible (in front and within viewport bounds); <c>false</c> otherwise, or if no target/camera is set.</returns>
    public bool IsTargetVisible()
    {
        if (mainCamera == null || currentTarget == null)
        {
            return false;
        }

        Vector3 viewportPoint = mainCamera.WorldToViewportPoint(currentTarget.position);

        // Check if target is in front of the camera (z > 0) and within normalized viewport bounds (0 to 1 for x and y)
        return viewportPoint.z > 0 &&
               viewportPoint.x >= 0 && viewportPoint.x <= 1 &&
               viewportPoint.y >= 0 && viewportPoint.y <= 1;
    }
    #endregion
}
