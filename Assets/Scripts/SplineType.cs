using System;
using UnityEngine;

// S(t) = T * M * G
// S(t) the spline result depending on t
// T a constant vector (t³, t², t, 1)
// M the spline matrix
// G the spline control points usage

// 3rd degree always work with 4x4 matrices and vector4

public class SplineMatrix
{
    public static Matrix4x4 HermiteM = new Matrix4x4(
        new Vector4(2f, -2f, 1f, 1f),
        new Vector4(-3f, 3f, -2f, -1f),
        new Vector4(0f, 0f, 1f, 0f),
        new Vector4(1f, 0f, 0f, 0f)
        );

    public static Matrix4x4 BezierM = new Matrix4x4(
        new Vector4(-1f, 3f, -3f, 1f),
        new Vector4(3f, -6f, 3f, 0f),
        new Vector4(-3f, 3f, 0f, 0f),
        new Vector4(1f, 0f, 0f, 0f)
        );
    public static Matrix4x4 BSplineM = new Matrix4x4(
        (1f / 6f) * new Vector4(-1f, 3f, -3f, 1f),
        (1f / 6f) * new Vector4(3f, -6f, 3f, 0f),
        (1f / 6f) * new Vector4(-3f, 0f, 3f, 0f),
        (1f / 6f) * new Vector4(1f, 4f, 1f, 0f)
        );
}

public class SplineUsage
{
    public static Matrix4x4 HermiteG(Vector3 P0, Vector3 R0, Vector3 P1, Vector3 R1)
    {
        return new Matrix4x4(
            new Vector4(P0.x, P0.y, P0.z, 1f),
            new Vector4(P1.x, P1.y, P1.z, 1f),
            new Vector4(R0.x, R0.y, R0.z, 1f),
            new Vector4(R1.x, R1.y, R1.z, 1f)
            );
    }

    public static Matrix4x4 BezierG(Vector3 P0, Vector3 P1, Vector3 P2, Vector3 P3)
    {
        return new Matrix4x4(
            new Vector4(P0.x, P0.y, P0.z, 1f),
            new Vector4(P1.x, P1.y, P1.z, 1f),
            new Vector4(P2.x, P2.y, P2.z, 1f),
            new Vector4(P3.x, P3.y, P3.z, 1f)
            );
    }

    public static Matrix4x4 BSplineG(Vector3 P0, Vector3 P1, Vector3 P2, Vector3 P3)
    {
        return BezierG(P0, P1, P2, P3);
    }
}

public class SplineType
{
    public Matrix4x4 M;
    public Func<Vector3, Vector3, Vector3, Vector3, Matrix4x4> G;

    public SplineType(Matrix4x4 m, Func<Vector3, Vector3, Vector3, Vector3, Matrix4x4> g)
    {
        M = m;
        G = g;
    }

    public static SplineType Hermite = new SplineType(SplineMatrix.HermiteM, SplineUsage.HermiteG);
    public static SplineType Bezier = new SplineType(SplineMatrix.BezierM, SplineUsage.BezierG);
    public static SplineType BSpline = new SplineType(SplineMatrix.BSplineM, SplineUsage.BSplineG);
}