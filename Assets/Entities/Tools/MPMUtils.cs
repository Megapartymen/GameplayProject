using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using JetBrains.Annotations;
using Unity.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Mathematics;
using Random = System.Random;
// using CareBoo.Serially;
using Unity.Jobs.LowLevel.Unsafe;

public static class MpmUtilities
{
    #region SpaceModifiesUtils-----------------------------------------------------------------------------------------
    
    public static Vector3 ToXYPlane(this Vector3 vec) => new Vector3(vec.x, vec.y, 0);
    public static Vector3 ToXZPlane(this Vector3 vec) => new Vector3(vec.x, 0, vec.z);
    public static Vector3 ToXZPlane(this Vector3 vec, float y) => new Vector3(vec.x, y, vec.z);
    public static Vector3 ToYZPlane(this Vector3 vec) => new Vector3(0, vec.y, vec.z);
    
    public static Vector3 TransformToCanvasSpace(this Transform transform, Canvas targetCanvas)
    {
        Vector2 viewport = targetCanvas.worldCamera.WorldToViewportPoint(transform.position);
        Ray canvasRay = targetCanvas.worldCamera.ViewportPointToRay(viewport);
        return canvasRay.GetPoint(targetCanvas.planeDistance);
    }
    
    public static Vector3 ToXZSpace(this Vector2 vector2) => new Vector3(vector2.x, 0, vector2.y);
    public static Vector3 ToXYSpace(this Vector2 vector2) => new Vector3(vector2.x, vector2.y, 0);
    public static Vector2 FromXZSpace(this Vector3 vector3) => new Vector2(vector3.x, vector3.z);
    public static float3 ToXZSpace(this float2 vector2) => new float3(vector2.x, 0, vector2.y);
    public static float2 FromXZSpace(this float3 vector3) => new float2(vector3.x, vector3.z);

    #endregion
}
