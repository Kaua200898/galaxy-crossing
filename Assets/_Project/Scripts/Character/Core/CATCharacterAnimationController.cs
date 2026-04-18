using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CATCharacterMovementController))]
public class CATCharacterAnimationController : MonoBehaviour
{
    // ============================================================================================
    //  UNITY INSPECTOR FIELDS
    // ============================================================================================

    [Header("Locomotion Animation")]

    [Min(0.0f)]
    [SerializeField]
    private float _idleSpeedThreshold = 0.05f;

    [Min(0.1f)]
    [SerializeField]
    private float _walkAnimationSpeed = 0.50f;

    [SerializeField]
    [Min(0.2f)]
    private float _sprintAnimationSpeed = 1.0f;

    [Min(0.0f)]
    [SerializeField]
    private float _speedBlendRate = 10.0f;

    // ============================================================================================
    //  PRIVATE MEMBERS
    // ============================================================================================

    private Animator _animatorComponent;

    private CATCharacterMovementController _characterMovementControllerComponent;

    private float _currentAnimationSpeed;

    // ============================================================================================
    //  PRIVATE METHODS
    // ============================================================================================

    public void OnJump()
    {
        _animatorComponent.SetTrigger("Activate_Jump");
    }

    public void OnDoubleJump()
    {
        _animatorComponent.SetTrigger("Activate_Double_Jump");
    }

    private void UpdateGrounding()
    {
        _animatorComponent.SetBool("Is_Grounded", _characterMovementControllerComponent.IsGrounded);
    }

    private void UpdateLocomotion()
    {
        Vector3 currentVelocity = _characterMovementControllerComponent.CurrentVelocity;

        float currentHorizontalVelocity = new Vector2(currentVelocity.x, currentVelocity.z).magnitude;

        float targetAnimationSpeed = 0.0f;

        bool isSprinting = _characterMovementControllerComponent.IsSprinting;

        if (currentHorizontalVelocity <= _idleSpeedThreshold)
        {
            targetAnimationSpeed = 0.0f;
        }
        else if (isSprinting == true)
        {
            targetAnimationSpeed = _sprintAnimationSpeed;
        }
        else
        {
            targetAnimationSpeed = _walkAnimationSpeed * Mathf.Clamp01(currentHorizontalVelocity);
        }

        _currentAnimationSpeed = Mathf.Lerp(_currentAnimationSpeed, targetAnimationSpeed, Time.deltaTime * _speedBlendRate);

        _animatorComponent.SetFloat("Move_Speed", _currentAnimationSpeed);
    }

    // ============================================================================================
    //  UNITY EVENT FUNCTIONS
    // ============================================================================================

    private void Awake()
    {
        _animatorComponent = GetComponent<Animator>();

        _characterMovementControllerComponent = GetComponent<CATCharacterMovementController>();

        _characterMovementControllerComponent.OnJump += OnJump;

        _characterMovementControllerComponent.OnDoubleJump += OnDoubleJump;
    }

    private void OnDestroy()
    {
        _characterMovementControllerComponent.OnJump -= OnJump;

        _characterMovementControllerComponent.OnDoubleJump -= OnDoubleJump;
    }

    public void Update()
    {
        UpdateGrounding();

        UpdateLocomotion();
    }
}

