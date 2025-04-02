using System;
using UnityEngine;

[Serializable]
public struct MoveData
{
    public Vector3 handPosition;
    public Vector3 rawWorldVelocity;
    public Vector3 rawLocalVelocity;
    public Vector3 mixedVelocity;
    public Vector3 normal;
    public RealHandTransform realHandTransform;
    public float speed;

    public Transform collidedObject;

    public bool IsParallelToSurface(float threshold = 0.7f)
    {
        switch (realHandTransform.handSide)
        {
            case VRController.Right:
                return Vector3.Dot(normal, realHandTransform.rotation * Vector3.right) >= threshold;
            case VRController.Left:
                return Vector3.Dot(normal, realHandTransform.rotation * Vector3.left) >= threshold;
            default:
                return false;
        }
    }
}