using System;
using UnityEngine;
using NetworkMessages;

[Serializable]
public class SimulationState : Message
{
    [SerializeField]
    public Vector3 Position;
    [SerializeField]
    public Quaternion Rotation;
    [SerializeField]
    public Vector3 Velocity;
}
