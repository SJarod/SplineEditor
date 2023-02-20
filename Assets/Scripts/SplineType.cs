using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplineType
{
    public static Matrix4x4 Hermite = new Matrix4x4(
        new Vector4( 2f, -2f,  1f,  1f),
        new Vector4(-3f,  3f, -2f, -1f),
        new Vector4( 0f,  0f,  1f,  0f),
        new Vector4( 1f,  0f,  0f,  0f)
        );

    public static Matrix4x4 Bezier = new Matrix4x4(
        new Vector4(-1f,  3f, -3f, 1f),
        new Vector4( 3f, -6f,  3f, 0f),
        new Vector4(-3f,  3f,  0f, 0f),
        new Vector4( 1f,  0f,  0f, 0f)
        );
}