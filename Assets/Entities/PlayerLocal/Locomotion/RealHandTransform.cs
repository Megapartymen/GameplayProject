using System;
using UnityEngine;

[Serializable]
public struct RealHandTransform
{
    public Vector3 position;
    public Vector3 localPosition;
    public Quaternion localRotation;
    public Quaternion rotation;
    public VRController handSide;
    public bool controllerActive;

        
}