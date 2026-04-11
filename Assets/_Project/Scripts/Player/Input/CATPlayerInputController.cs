using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class CATPlayerInputController : MonoBehaviour, ICATCharacterInputController
{
    // ============================================================================================
    //  PRIVATE MEMBERS
    // ============================================================================================

    private CATPlayerInputActions _playerInputActions;

    // ============================================================================================
    //  PUBLIC MEMBERS
    // ============================================================================================

    public Vector2 MoveVector { get; private set; } = Vector2.zero;

    public Vector2 LookVector { get; private set; } = Vector2.zero;

    public bool SprintHeld { get; private set; } = false;

    public bool JumpHeld { get; private set; } = false;

    public event Action SprintPressed;

    public event Action JumpPressed;

    public event Action AttackPressed;

    // ============================================================================================
    //  PRIVATE METHODS
    // ============================================================================================

    private void OnMove(InputAction.CallbackContext context)
    {
        MoveVector = context.ReadValue<Vector2>();
    }

    private void OnLook(InputAction.CallbackContext context)
    {
        LookVector = context.ReadValue<Vector2>();
    }

    private void OnSprint(InputAction.CallbackContext context)
    {
        SprintPressed?.Invoke();

        SprintHeld = !SprintHeld;
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        JumpPressed?.Invoke();

        JumpHeld = !JumpHeld;
    }

    private void OnAttack(InputAction.CallbackContext context)
    {
        AttackPressed?.Invoke();
    }

    private void Awake()
    {
        _playerInputActions = new CATPlayerInputActions();
    }

    private void OnEnable()
    {
        _playerInputActions.Enable();

        _playerInputActions.CAT_Character.Move.performed += OnMove;
        _playerInputActions.CAT_Character.Move.canceled += OnMove;

        _playerInputActions.CAT_Character.Look.performed += OnLook;
        _playerInputActions.CAT_Character.Look.canceled += OnLook;

        _playerInputActions.CAT_Character.Sprint.performed += OnSprint;

        _playerInputActions.CAT_Character.Jump.performed += OnJump;

        _playerInputActions.CAT_Character.Attack.performed += OnAttack;
    }

    private void OnDisable()
    {
        _playerInputActions.CAT_Character.Move.performed -= OnMove;
        _playerInputActions.CAT_Character.Move.canceled -= OnMove;

        _playerInputActions.CAT_Character.Look.performed -= OnLook;
        _playerInputActions.CAT_Character.Look.canceled -= OnLook;

        _playerInputActions.CAT_Character.Sprint.performed -= OnSprint;

        _playerInputActions.CAT_Character.Jump.performed -= OnJump;

        _playerInputActions.CAT_Character.Attack.performed -= OnAttack;

        _playerInputActions.Disable();
    }
}