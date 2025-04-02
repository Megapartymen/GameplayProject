using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocomotionBody : MonoBehaviour
{
    private Vector3 _previousTarget;
    
    public void RotateBodyToDirection(Vector3 direction)
    {
        Vector3 currentTarget = direction - transform.position;
        Vector3 lerpedTarget = Vector3.Lerp(_previousTarget, currentTarget, 0.01f);
        transform.localRotation = Quaternion.LookRotation(lerpedTarget, Vector3.up);
        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
        _previousTarget = currentTarget;
    }
    
    public void SetBodyPosition(Vector3 XZposition, Vector3 Yposition)
    {
        transform.position = new Vector3(XZposition.x, Yposition.y, XZposition.z);
    }
}
