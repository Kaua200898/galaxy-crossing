using UnityEngine;
using System;

public interface ICATCharacterInputController
{
    // ============================================================================================
    //  PUBLIC MEMBERS
    // ============================================================================================

    Vector2 MoveVector { get; }

    Vector2 LookVector { get; }

    bool SprintHeld { get; }

    bool JumpHeld { get; }

    public event Action SprintPressed;

    public event Action JumpPressed;

    public event Action AttackPressed;
}
