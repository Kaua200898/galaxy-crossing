using CAT.Character.Enums;
using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(ICATCharacterInputController))]
public class CATCharacterMovementController : MonoBehaviour
{
    // ============================================================================================
    //  UNITY INSPECTOR FIELDS
    // ============================================================================================

    [Header("Walk Movement")]

    [SerializeField]
    private bool _enableWalk = true;

    [Min(0.1f)]
    [SerializeField]
    private float _walkNominalVelocity = 7.5f;

    [Header("Sprint Movement")]

    [SerializeField]
    private bool _enableSprint = true;

    [Min(0.1f)]
    [SerializeField]
    private float _sprintNominalVelocity = 12.0f;

    [Header("Jump")]

    [SerializeField]
    private bool _enableJump = true;

    [Min(1)]
    [SerializeField]
    private int _maximumJumpCount = 2;

    [Min(0.1f)]
    [SerializeField]
    private float _jumpMaximumHeight = 2.00f;

    [Min(0.1f)]
    [SerializeField]
    private float _jumpMinimumHeight = 0.50f;

    [Min(0.05f)]
    [SerializeField]
    private float _jumpFullHoldTime = 0.2f;

    [SerializeField]
    private AnimationCurve _jumpCurve = new AnimationCurve(
        new Keyframe(0.0f, 0.0f, 0.0f, 3.0f),
        new Keyframe(1.0f, 1.0f, 0.0f, 0.0f)
    );

    [Min(0.0f)]
    [SerializeField]
    private float _jumpCoyoteTime = 0.15f;

    [Min(0.0f)]
    [SerializeField]
    private float _jumpBufferTime = 0.15f;

    [Header("Rotation")]

    [Min(1.0f)]
    [SerializeField]
    private float _rotationSpeed = 720.0f;

    [Min(0.0f)]
    [SerializeField]
    private float _rotationAccelerationTime = 0.05f;

    [SerializeField]
    private AnimationCurve _rotationCurve = new AnimationCurve(
        new Keyframe(0.0f, 0.0f, 0.0f, 3.0f),
        new Keyframe(1.0f, 1.0f, 0.0f, 0.0f)
    );

    [Header("Physics")]

    [Min(0.1f)]
    [SerializeField]
    private float _characterMass = 70.0f;

    [Min(0.01f)]
    [SerializeField]
    private float _accelerationTime = 0.3f;

    [Min(0.01f)]
    [SerializeField]
    private float _decelerationTime = 0.2f;

    [SerializeField]
    private AnimationCurve _accelerationCurve = new AnimationCurve(
        new Keyframe(0.0f, 0.0f, 0.0f, 3.0f),
        new Keyframe(1.0f, 1.0f, 0.0f, 0.0f)
    );

    [SerializeField]
    private AnimationCurve _decelerationCurve = new AnimationCurve(
        new Keyframe(0.0f, 1.0f, 0.0f, -3.0f),
        new Keyframe(1.0f, 0.0f, 0.0f, 0.0f)
    );

    [Range(0f, 1f)]
    [SerializeField]
    private float _airControlabilityFactor = 0.35f;

    [Header("Gravity")]

    [SerializeField]
    private bool _enableGravity = true;
    [Min(0.1f)]

    [SerializeField]
    private float _gravityAcceleration = 9.81f;
    [Min(0.1f)]

    [SerializeField]
    private float _gravityRiseMultiplier = 0.75f;
    [Min(0.1f)]

    [SerializeField]
    private float _gravityFallMultiplier = 1.00f;

    [Header("Ground Detection")]

    [SerializeField]
    private Vector3 _groundCheckOffset = Vector3.zero;
    [Min(0.0f)]

    [SerializeField]
    private float _groundCheckRadius = 0.25f;

    [SerializeField]
    private LayerMask _groundLayers = -1;

    // ============================================================================================
    //  PRIVATE MEMBERS
    // ============================================================================================
    
    private Rigidbody _rigidbodyComponent;

    private ICATCharacterInputController _inputControllerComponent;

    private ECATCharacterMovementState _movementState = ECATCharacterMovementState.Falling;

    private bool _isSprinting = false;

    private float _speedBlend = 0.0f;

    private float _speedBlendTime = 0.0f;

    private bool _wasAccelerating = false;

    private float _rotationBlendTime = 0.0f;

    private bool _isGrounded = false;

    private bool _isJumping = false;

    private int _remainingJumpCount = 0;

    private float _jumpHoldTimer = 0.0f;

    private float _coyoteTimer = 0.0f;

    private float _jumpBufferTimer = 0.0f;

    private bool _jumpMinVelocityApplied = false;

    // ============================================================================================
    //  PUBLIC MEMBERS
    // ============================================================================================

    public Quaternion MovementOrientation { get; set; }

    public ECATCharacterMovementState MovementState => _movementState;

    public bool IsGrounded => _isGrounded;

    public bool IsSprinting => _isSprinting;

    public bool IsJumping => _isJumping;

    // ============================================================================================
    //  PRIVATE METHODS
    // ============================================================================================

     private void OnSprintPressed()
    {
        if (_enableSprint == true && _isGrounded == true && _isJumping == false)
        {
            _isSprinting = !_isSprinting;
        }
    }

    private void OnJumpPressed()
    {
        if (_enableJump == true)
        {
            _jumpBufferTimer = _jumpBufferTime;
        }
    }

    private void SetupRigidbody()
    {
        _rigidbodyComponent.useGravity = false;

        _rigidbodyComponent.mass = _characterMass;

        _rigidbodyComponent.constraints = RigidbodyConstraints.FreezeRotation;
    }

     private void CheckGround()
    {
        bool wasGrounded = _isGrounded;

        Vector3 groundCheckOrigin = transform.position + transform.TransformDirection(_groundCheckOffset);

        _isGrounded = Physics.CheckSphere(groundCheckOrigin, _groundCheckRadius, _groundLayers, QueryTriggerInteraction.Ignore);

        bool justLanded = wasGrounded == false && _isGrounded == true && _rigidbodyComponent.velocity.y <= 0.0f;

        if (justLanded == true)
        {
            _remainingJumpCount = _maximumJumpCount + 1;

            _isJumping = false;

            _jumpHoldTimer = 0.0f;

            _jumpMinVelocityApplied = false;
        }

        if (wasGrounded == true && _isGrounded == false && _isJumping == false)
        {
            _coyoteTimer = _jumpCoyoteTime;
        }
    }

    private void ApplyJump()
    {
        if (_enableJump == true)
        {
            _coyoteTimer = Mathf.Max(0.0f, _coyoteTimer - Time.fixedDeltaTime);

            _jumpBufferTimer = Mathf.Max(0.0f, _jumpBufferTimer - Time.fixedDeltaTime);

            if (_jumpBufferTimer > 0.0f)
            {
                bool canJumpFromGround = _isGrounded == true || _coyoteTimer > 0.0f;

                bool canJumpFromAir = _isGrounded == false && _remainingJumpCount > 0;

                if (canJumpFromGround == true || canJumpFromAir == true)
                {
                    if (canJumpFromGround == false)
                    {
                        _remainingJumpCount--;
                    }

                    _isJumping = true;

                    _isSprinting = false;

                    _jumpMinVelocityApplied = false;

                    _jumpHoldTimer = 0.0f;

                    _jumpBufferTimer = 0.0f;

                    _coyoteTimer = 0.0f;

                    Vector3 currentVelocity = _rigidbodyComponent.velocity;

                    currentVelocity.y = 0.0f;

                    _rigidbodyComponent.velocity = currentVelocity;
                }
            }

            if (_isJumping == true)
            {
                bool jumpHeld = _inputControllerComponent.JumpHeld;

                if (jumpHeld == true && _jumpHoldTimer < _jumpFullHoldTime)
                {
                    _jumpHoldTimer += Time.fixedDeltaTime;

                    float curveTime = Mathf.Clamp01(_jumpHoldTimer / _jumpFullHoldTime);

                    float targetHeight = Mathf.Lerp(_jumpMinimumHeight, _jumpMaximumHeight, _jumpCurve.Evaluate(curveTime));

                    float effectiveGravity = _gravityAcceleration * _gravityRiseMultiplier;

                    float targetVerticalVelocity = Mathf.Sqrt(2.0f * effectiveGravity * targetHeight);

                    Vector3 currentVelocity = _rigidbodyComponent.velocity;

                    currentVelocity.y = targetVerticalVelocity;

                    _rigidbodyComponent.velocity = currentVelocity;
                }
                else if (_jumpMinVelocityApplied == false)
                {
                    float effectiveGravity = _gravityAcceleration * _gravityRiseMultiplier;

                    float minimalVerticalVelocity = Mathf.Sqrt(2.0f * effectiveGravity * _jumpMinimumHeight);

                    if (_rigidbodyComponent.velocity.y < minimalVerticalVelocity)
                    {
                        Vector3 currentVelocity = _rigidbodyComponent.velocity;

                        currentVelocity.y = minimalVerticalVelocity;

                        _rigidbodyComponent.velocity = currentVelocity;
                    }

                    _jumpMinVelocityApplied = true;

                    _jumpHoldTimer = _jumpFullHoldTime;
                }
            }
        }
    }

    private void ApplyTranslationMovement()
    {
        Vector2 moveVector = _inputControllerComponent.MoveVector;

        bool wantsToMove = moveVector.sqrMagnitude > 0.001f;

        float targetSpeed = 0.0f;

        if (wantsToMove == true)
        {
            if (_enableSprint == true && _isSprinting == true)
            {
                targetSpeed = _sprintNominalVelocity;
            }
            else if (_enableWalk == true)
            {
                targetSpeed = _walkNominalVelocity;
            }
        }

        if (_isGrounded == false)
        {
            targetSpeed *= _airControlabilityFactor;
        }

        Vector3 cameraForward = MovementOrientation * Vector3.forward;

        cameraForward = new Vector3(cameraForward.x, 0.0f, cameraForward.z).normalized;

        Vector3 cameraRight = MovementOrientation * Vector3.right;

        cameraRight = new Vector3(cameraRight.x, 0.0f, cameraRight.z).normalized;

        Vector3 targetHorizontalVelocity = Vector3.zero;

        if (wantsToMove == true && targetSpeed > 0.0f)
        {
            Vector3 inputDirection = (cameraForward * moveVector.y + cameraRight * moveVector.x).normalized;

            targetHorizontalVelocity = inputDirection * targetSpeed;
        }

        bool isAccelerating = wantsToMove && targetSpeed > 0.0f;

        if (isAccelerating != _wasAccelerating)
        {
            _speedBlendTime = 1.0f - _speedBlendTime;

            _wasAccelerating = isAccelerating;
        }

        if (isAccelerating == true)
        {
            _speedBlendTime = Mathf.Clamp01(_speedBlendTime + Time.fixedDeltaTime / _accelerationTime);

            _speedBlend = _accelerationCurve.Evaluate(_speedBlendTime);
        }
        else
        {
            _speedBlendTime = Mathf.Clamp01(_speedBlendTime + Time.fixedDeltaTime / _decelerationTime);

            _speedBlend = _decelerationCurve.Evaluate(_speedBlendTime);
        }

        Vector3 appliedVelocity = targetHorizontalVelocity * _speedBlend;

        _rigidbodyComponent.velocity = new Vector3(appliedVelocity.x, _rigidbodyComponent.velocity.y, appliedVelocity.z);
    }

    private void ApplyRotationMovement()
    {
        Vector2 moveVector = _inputControllerComponent.MoveVector;

        if (moveVector.sqrMagnitude < 0.001f)
        {
            _rotationBlendTime = 0.0f;
        }
        else
        {
            Vector3 cameraForward = MovementOrientation * Vector3.forward;

            cameraForward = new Vector3(cameraForward.x, 0.0f, cameraForward.z).normalized;

            Vector3 cameraRight = MovementOrientation * Vector3.right;

            cameraRight = new Vector3(cameraRight.x, 0.0f, cameraRight.z).normalized;

            Vector3 moveDirection = (cameraForward * moveVector.y + cameraRight * moveVector.x).normalized;

            Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);

            _rotationBlendTime = Mathf.Clamp01(_rotationBlendTime + Time.fixedDeltaTime / Mathf.Max(0.001f, _rotationAccelerationTime));

            float rotationSpeedThisFrame = _rotationSpeed * _rotationCurve.Evaluate(_rotationBlendTime);

            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeedThisFrame * Time.fixedDeltaTime);
        }
    }

    private void ApplyGravity()
    {
        if (_enableGravity == true && (_isJumping == false || _jumpHoldTimer >= _jumpFullHoldTime))
        {
            float gravityMultiplier = (_rigidbodyComponent.velocity.y > 0.00f) ? _gravityRiseMultiplier : _gravityFallMultiplier;

            Vector3 gravityForce = _characterMass * _gravityAcceleration * gravityMultiplier * Vector3.down;

            _rigidbodyComponent.AddForce(gravityForce, ForceMode.Force);
        }
    }

    private void UpdateMovementState()
    {
        if (_isGrounded == false)
        {
            _movementState = (_rigidbodyComponent.velocity.y >= 0.0f) ? ECATCharacterMovementState.Rising : ECATCharacterMovementState.Falling;
        }
        else
        {
            Vector3 horizontalVelocity = new Vector3(_rigidbodyComponent.velocity.x, 0.0f, _rigidbodyComponent.velocity.z);

            if (horizontalVelocity.sqrMagnitude < 0.01f)
            {
                _movementState = ECATCharacterMovementState.Idle;
            }
            else if (_isSprinting == true)
            {
                _movementState = ECATCharacterMovementState.Sprinting;
            }
            else
            {
                _movementState = ECATCharacterMovementState.Walking;
            }
        }
    }
    
    // ============================================================================================
	//  UNITY EVENT FUNCTIONS
	// ============================================================================================

     private void Awake()
    {
        _rigidbodyComponent = GetComponent<Rigidbody>();

        _inputControllerComponent = GetComponent<ICATCharacterInputController>();

        _inputControllerComponent.SprintPressed += OnSprintPressed;

        _inputControllerComponent.JumpPressed += OnJumpPressed;

        MovementOrientation = transform.rotation;

        _remainingJumpCount = _maximumJumpCount + 1;

        SetupRigidbody();
    }
    
    private void OnDestroy()
    {
        _inputControllerComponent.SprintPressed -= OnSprintPressed;

        _inputControllerComponent.JumpPressed -= OnJumpPressed;
    }

     private void FixedUpdate()
    {
        CheckGround();

        ApplyJump();

        ApplyGravity();

        ApplyTranslationMovement();

        ApplyRotationMovement();

        UpdateMovementState();
    }
}