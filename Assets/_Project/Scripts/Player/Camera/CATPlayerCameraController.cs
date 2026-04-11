using UnityEngine;

[RequireComponent(typeof(ICATCharacterInputController))]
[RequireComponent(typeof(CATCharacterMovementController))]
public class CATPlayerCameraController : MonoBehaviour
{
    // ============================================================================================
    //  UNITY INSPECTOR FIELDS
    // ============================================================================================

    [Header("Camera Controls")]

    [SerializeField]
    private bool _enableCamera = true;

    [SerializeField]
    private bool _lockPlayerCursor = true;

    [Header("Camera Yaw")]

    [SerializeField]
    private bool _invertYawAxisControls = false;

    [Header("Camera Pitch")]

    [SerializeField]
    private bool _invertPitchAxisControls = false;

    [SerializeField]
    [Range(0.0f, 180.0f)]
    private float _maximumPitchAngle = 45.0f;

    [SerializeField]
    [Range(-180.0f, 0.0f)]
    private float _minimumPitchAngle = -15.0f;

    [Header("Camera Sensitivity")]

    [Range(0.1f, 1.0f)]
    [SerializeField]
    private float _xAxisSensitivity = 0.5f;

    [Range(0.1f, 1.0f)]
    [SerializeField]
    private float _yAxisSensitivity = 0.5f;

    [Header("Camera References")]

    [SerializeField]
    private Transform _cameraRootReference;

    [SerializeField]
    private Transform _cameraAnchorReference;

    [Header("Camera Movement")]

    [Min(0.01f)]
    [SerializeField]
    private float _nominalRotationalVelocity = 3.0f;

    [Min(0.01f)]
    [SerializeField]
    private float _rotationalAccelerationTime = 0.5f;

    [Min(0.01f)]
    [SerializeField]
    private float _rotationalDecelerationTime = 0.5f;

    [SerializeField]
    private AnimationCurve _rotationalAccelerationCurve = new AnimationCurve(
        new Keyframe(0.0f, 0.0f, 0.0f, 3.0f),
        new Keyframe(1.0f, 1.0f, 0.0f, 0.0f)
    );

    [SerializeField]
    private AnimationCurve _rotationalDecelerationCurve = new AnimationCurve(
        new Keyframe(0.0f, 1.0f, 0.0f, -3.0f),
        new Keyframe(1.0f, 0.0f, 0.0f, 0.0f)
    );

    // ============================================================================================
    //  PRIVATE MEMBERS
    // ============================================================================================

    private ICATCharacterInputController _characterInputController;

    private CATCharacterMovementController _characterMovementController;

    private float _currentYaw = 0.0f;

    private float _currentPitch = 0.0f;

    private float _rotationBlend = 0.0f;

    private float _rotationBlendTime = 0.0f;

    private bool _wasRotating = false;

    // ============================================================================================
    //  PUBLIC MEMBERS
    // ============================================================================================
    public float CurrentYaw => _currentYaw;
    public float CurrentPitch => _currentPitch;

    // ============================================================================================
    //  PRIVATE METHODS
    // ============================================================================================

    private void InitializeCameraAngles()
    {
        if (_cameraRootReference != null)
        {
            _currentYaw = _cameraRootReference.eulerAngles.y;
        }

        if (_cameraAnchorReference != null)
        {
            float seedPitch = _cameraAnchorReference.localEulerAngles.x;

            if (seedPitch > 180.0f)
            {
                seedPitch -= 360.0f;
            }
            
            _currentPitch = Mathf.Clamp(seedPitch, _minimumPitchAngle, _maximumPitchAngle);
        }
    }

    private void ApplyPlayerCursorLock()
    {
        if (_lockPlayerCursor == true)
        {
            Cursor.lockState = CursorLockMode.Locked;
     
            Cursor.visible = false;
        }
    }

    private void ApplyRotationalBlend(bool isRotating)
    {
        if (isRotating != _wasRotating)
        {
            _rotationBlendTime = 1.0f - _rotationBlendTime;

            _wasRotating = isRotating;
        }

        if (isRotating)
        {
            _rotationBlendTime = Mathf.Clamp01(_rotationBlendTime + Time.deltaTime / _rotationalAccelerationTime);

            _rotationBlend = _rotationalAccelerationCurve.Evaluate(_rotationBlendTime);
        }
        else
        {
            _rotationBlendTime = Mathf.Clamp01(_rotationBlendTime + Time.deltaTime / _rotationalDecelerationTime);
            
            _rotationBlend = _rotationalDecelerationCurve.Evaluate(_rotationBlendTime);
        }
    }

    private void ApplyCameraRootRotation()
    {
        if (_enableCamera == true && _cameraRootReference != null)
        {
            Vector2 LookVector = _characterInputController.LookVector;

            float yawAxis = LookVector.x;

            if (_invertYawAxisControls == true)
            {
                yawAxis *= -1.0f;
            }

            bool isRotating = LookVector.sqrMagnitude > 0.0001f;

            ApplyRotationalBlend(isRotating);

            _currentYaw += yawAxis * _nominalRotationalVelocity * _xAxisSensitivity * _rotationBlend;

            _cameraRootReference.rotation = Quaternion.Euler(0.0f, _currentYaw, 0.0f);

            _characterMovementController.MovementOrientation = _cameraRootReference.rotation;
        }
    }

    private void ApplyCameraAnchorRotation()
    {
        if (_enableCamera == true && _cameraAnchorReference != null)
        {
            Vector2 LookVector = _characterInputController.LookVector;

            float pitchAxis = LookVector.y;

            if (_invertPitchAxisControls == true)
            {
                pitchAxis *= -1.0f;
            }

            float pitchDelta = pitchAxis * _nominalRotationalVelocity * _yAxisSensitivity * _rotationBlend;

            _currentPitch = Mathf.Clamp(_currentPitch + pitchDelta, _minimumPitchAngle, _maximumPitchAngle);

            _cameraAnchorReference.localRotation = Quaternion.Euler(_currentPitch, 0.0f, 0.0f);
        }
    }

    // ============================================================================================
    //  UNITY EVENT FUNCTIONS
    // ============================================================================================

    private void Awake()
    {
        _characterInputController = GetComponent<ICATCharacterInputController>();

        _characterMovementController = GetComponent<CATCharacterMovementController>();

        InitializeCameraAngles();
    }

    private void Start()
    {
        ApplyPlayerCursorLock();
    }

    private void LateUpdate()
    {
        ApplyCameraRootRotation();

        ApplyCameraAnchorRotation();
    }
}
