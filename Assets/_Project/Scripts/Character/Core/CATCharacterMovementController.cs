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
}