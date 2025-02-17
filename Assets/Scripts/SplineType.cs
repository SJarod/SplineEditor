using System;
using Unity.VisualScripting;
using UnityEngine;

// S(t) = T * M * G
// S(t) the spline result depending on t
// T a constant vector (t�, t�, t, 1)
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

    public static Matrix4x4 CatmullRomM = new Matrix4x4(
        (1f / 2f) * new Vector4(-1f, 3f, -3f, 1f),
        (1f / 2f) * new Vector4(2f, -5f, 4f, -1f),
        (1f / 2f) * new Vector4(-1f, 0f, 1f, 0f),
        (1f / 2f) * new Vector4(0f, 2f, 0f, 0f)
        );
}

public class SplineUsage
{
    public static Matrix4x4 HermiteG(Vector3 entry, Vector3 R0, Vector3 R1, Vector3 exit)
    {
        return new Matrix4x4(
            new Vector4(entry.x, entry.y, entry.z, 1f),
            new Vector4(exit.x, exit.y, exit.z, 1f),
            new Vector4(R0.x, R0.y, R0.z, 1f),
            new Vector4(R1.x, R1.y, R1.z, 1f)
            );
    }

    public static Matrix4x4 BezierG(Vector3 entry, Vector3 P1, Vector3 P2, Vector3 exit)
    {
        return new Matrix4x4(
            new Vector4(entry.x, entry.y, entry.z, 1f),
            new Vector4(P1.x, P1.y, P1.z, 1f),
            new Vector4(P2.x, P2.y, P2.z, 1f),
            new Vector4(exit.x, exit.y, exit.z, 1f)
            );
    }

    public static Matrix4x4 BSplineG(Vector3 entry, Vector3 P1, Vector3 P2, Vector3 exit)
    {
        return BezierG(entry, P1, P2, exit);
    }

    public static Matrix4x4 CatmullRomG(Vector3 entry, Vector3 P1, Vector3 P2, Vector3 exit)
    {
        return BezierG(entry, P1, P2, exit);
    }
}

public enum ESplineType
{
    HERMITE,
    BEZIER,
    BSPLINE,
    CATMULLROM,
    COUNT
}

[Serializable]
public class SplineType
{
    public ESplineType splineType;

    [HideInInspector]
    public Matrix4x4 M;
    [HideInInspector]
    public Func<Vector3, Vector3, Vector3, Vector3, Matrix4x4> G;

    public SplineType(ESplineType st)
    {
        splineType = st;

        switch (splineType)
        {
            case ESplineType.HERMITE:
                M = SplineMatrix.HermiteM;
                G = SplineUsage.HermiteG;
                break;
            case ESplineType.BEZIER:
                M = SplineMatrix.BezierM;
                G = SplineUsage.BezierG;
                break;
            case ESplineType.BSPLINE:
                M = SplineMatrix.BSplineM;
                G = SplineUsage.BSplineG;
                break;
            case ESplineType.CATMULLROM:
                M = SplineMatrix.CatmullRomM;
                G = SplineUsage.CatmullRomG;
                break;
            default:
                break;
        }

    }

    public static SplineType Hermite = new SplineType(ESplineType.HERMITE);
    public static SplineType Bezier = new SplineType(ESplineType.BEZIER);
    public static SplineType BSpline = new SplineType(ESplineType.BSPLINE);
    public static SplineType CatmullRom = new SplineType(ESplineType.CATMULLROM);
}

[Serializable]
public enum EContinuity
{
    C0,
    C1,
    C2,
    COUNT
}