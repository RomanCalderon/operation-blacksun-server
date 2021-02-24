using System;
using System.Collections.Generic;
using UnityEngine;
using NetworkMessages;

[Serializable]
public class ClientInputState : Message
{
    public bool MoveForward;
    public bool MoveBackward;
    public bool MoveRight;
    public bool MoveLeft;
    public bool Jump;
    public bool Run;
    public bool Crouch;

    [SerializeField]
    public Quaternion Rotation;
}
