using UnityEngine;

using Fusion;

public struct PlaneInputData : INetworkInput
{
    public float Pitch;
    public float Yaw;
    public float Roll;
    public float Thrust;
    public float Turn;
    public bool Fire;
    public bool SecondaryFire;
}