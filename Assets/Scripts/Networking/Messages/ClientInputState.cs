using System;
using System.Collections.Generic;
using UnityEngine;
using NetworkMessages;

[Serializable]
public class ClientInputState : Message
{
    // Movement
    public bool MoveForward;
    public bool MoveBackward;
    public bool MoveRight;
    public bool MoveLeft;
    public bool Jump;
    public bool Run;
    public bool Crouch;
    // Weapons
    public bool Shoot;
    public bool Aiming;
    // Orientation
    [SerializeField]
    public Vector3 GunDirection;
    [SerializeField]
    public Vector3 LookDirection;
    public float CameraPitch;
    // Interactions
    public bool Interact;

    [SerializeField]
    public Quaternion Rotation;

    public ClientInputState ()
    {
        Rotation.Normalize ();
    }
}
