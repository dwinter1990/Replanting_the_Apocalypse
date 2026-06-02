using CS.AudioToolkit;
using Unity.Cinemachine;
using UnityEngine;

/// <summary>
/// Adds head bob while moving and jump/land feedback to a Cinemachine camera.
/// Call SetMovementState from your controller each frame and NotifyJump when jump starts.
/// </summary>
public class FirstPersonCameraFeedback : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CinemachineCamera cam;
    [SerializeField] private Transform feedbackTarget;

    [Header("Base FOV")]
    [SerializeField] private float baseFov = 60f;
    [SerializeField] private float sprintFov = 90f;
    [SerializeField] private float fovLerpSpeed = 8f;

    [Header("Head Bob")]
    [SerializeField] private float bobFrequency = 10f;
    [SerializeField] private float bobAmplitude = 0.04f;
    [SerializeField] private float bobLerpSpeed = 10f;

    [Header("Jump / Land Feedback")]
    [SerializeField] private float jumpTilt = -2.5f;
    [SerializeField] private float landTilt = 3.5f;
    [SerializeField] private float tiltLerpSpeed = 12f;

    private Vector3 _initialLocalPos;
    private float _bobTimer;
    private float _targetTilt;
    private float _currentTilt;
    private float _lastAppliedTilt;
    private bool _isMoving;
    private bool _isSprinting;

    public static FirstPersonCameraFeedback Instance { get; private set; }
    private void Awake()
    {
        if (cam == null)
        {
            cam = FindAnyObjectByType<CinemachineCamera>();
        }

        _initialLocalPos = transform.localPosition;

        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Multiple instances of FirstPersonCameraFeedback detected. There should only be one in the scene.");
        }
        else
        {
            Instance = this;
        }

        if (feedbackTarget == null)
        {
            feedbackTarget = transform;
        }

        _initialLocalPos = feedbackTarget.localPosition;

    }

    private void Update()
    {
        if (cam != null)
        {
            float targetFov = _isSprinting ? sprintFov : baseFov;
            cam.Lens.FieldOfView = Mathf.Lerp(cam.Lens.FieldOfView, targetFov, fovLerpSpeed * Time.deltaTime);
        }

        UpdateHeadBob();
        UpdateTilt();
    }

    /// <summary>
    /// Feed movement state each frame from your movement controller.
    /// </summary>
    public void SetMovementState(bool isMoving, bool isSprinting)
    {
        _isMoving = isMoving;
        _isSprinting = isSprinting;
    }

    /// <summary>
    /// Call when player jumps.
    /// </summary>
    public void NotifyJump()
    {
        _targetTilt = jumpTilt;
    }

    /// <summary>
    /// Call when player lands.
    /// </summary>
    public void NotifyLand()
    {
        AudioController.Play("Landing");
        _targetTilt = landTilt;
    }

    private void UpdateHeadBob()
    {
        Vector3 targetPos = _initialLocalPos;

        if (_isMoving)
        {
            _bobTimer += Time.deltaTime * bobFrequency * (_isSprinting ? 1.35f : 1f);
            float bobOffsetY = Mathf.Sin(_bobTimer) * bobAmplitude;
            float bobOffsetX = Mathf.Cos(_bobTimer * 0.5f) * bobAmplitude * 0.5f;
            targetPos += new Vector3(bobOffsetX, bobOffsetY, 0f);
        }
        else
        {
            _bobTimer = 0f;
        }

        feedbackTarget.localPosition = Vector3.Lerp(feedbackTarget.localPosition, targetPos, bobLerpSpeed * Time.deltaTime);
    }

private void UpdateTilt()
    {
        _currentTilt = Mathf.Lerp(_currentTilt, _targetTilt, tiltLerpSpeed * Time.deltaTime);

        // Preserve existing look rotation from other systems by removing prior tilt and applying new tilt.
        Quaternion withoutPreviousTilt = feedbackTarget.localRotation * Quaternion.Inverse(Quaternion.Euler(_lastAppliedTilt, 0f, 0f));
        feedbackTarget.localRotation = withoutPreviousTilt * Quaternion.Euler(_currentTilt, 0f, 0f);
        _lastAppliedTilt = _currentTilt;

        // Return to neutral after an impulse.
        _targetTilt = Mathf.Lerp(_targetTilt, 0f, tiltLerpSpeed * Time.deltaTime);
    }
}
