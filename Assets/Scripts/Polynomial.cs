using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.TerrainTools;
using UnityEditor.UIElements;
using UnityEngine;

[Serializable]
public struct ControlPoint
{
    public Vector3 pos;
}

[Serializable]
public class Polynomial : MonoBehaviour
{
    // degree is useless for now
    [SerializeField]
    private int degree = 3;
    public SplineType splineType = new SplineType(ESplineType.BEZIER);

    // spline resolution
    [SerializeField]
    private int knots = 99;


    // CONTROL POINTS

    // entry point
    private ControlPoint A;

    // secondary points
    [HideInInspector]
    public ControlPoint B;
    [HideInInspector]
    public ControlPoint C;

    // exit point
    [HideInInspector]
    public ControlPoint D;

    private Polynomial previousJunction = null;

    public void InitSpline(ESplineType st)
    {
        splineType = new SplineType(st);
    }

    public void SetPreviousJunction(Polynomial prev)
    {
        previousJunction = prev;
        A.pos = prev.D.pos;
        B.pos = prev.D.pos;
        C.pos = prev.D.pos;
        D.pos = prev.D.pos;
    }

    public void OverrideControlPoints()
    {
        if (!previousJunction)
            return;

        switch (splineType.splineType)
        {
            case ESplineType.HERMITE:
            case ESplineType.BEZIER:
                A = previousJunction.D;
                break;
            case ESplineType.BSPLINE:
            case ESplineType.CATMULLROM:
                A = previousJunction.B;
                B = previousJunction.C;
                C = previousJunction.D;
                break;
            default:
                break;
        }
    }

    public Vector3 SplinePolynomial(float t)
    {
        float t3 = t * t * t;
        float t2 = t * t;
        Vector4 T = new Vector4(t3, t2, t, 1f);

        OverrideControlPoints();

        // S = T * M * G
        return (splineType.G(A.pos, B.pos, C.pos, D.pos) * splineType.M).MultiplyPoint(T);
    }

    public void DrawSpline()
    {
        Gizmos.color = Color.white;

        for (int i = 0; i < knots; ++i)
        {
            float t = i / (float)knots;
            float tt = (i + 1) / (float)knots;
            Gizmos.DrawLine(SplinePolynomial(t), SplinePolynomial(tt));
        }
    }

    private void OnDrawGizmos()
    {
        DrawSpline();
    }

    public void DrawDebugSpline()
    {
        Vector3 entry = A.pos;
        if (previousJunction)
            entry = previousJunction.D.pos;

        switch (splineType.splineType)
        {
            case ESplineType.HERMITE:
                Gizmos.color = Color.red;
                Gizmos.DrawLine(entry, B.pos);
                Gizmos.DrawLine(C.pos, D.pos);
                break;
            case ESplineType.BEZIER:
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(entry, B.pos);
                Gizmos.DrawLine(B.pos, C.pos);
                Gizmos.DrawLine(C.pos, D.pos);
                break;
            default:
                break;
        }

        // main control points
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(entry, 1f);
        Gizmos.DrawSphere(D.pos, 1f);
        // secondary control points
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(B.pos, 1f);
        Gizmos.DrawSphere(C.pos, 1f);
    }

    private void OnDrawGizmosSelected()
    {
        DrawDebugSpline();
    }

    public void DrawMoveTool()
    {
        if (!previousJunction)
            A.pos = Handles.PositionHandle(A.pos, Quaternion.identity);
        if (!previousJunction || previousJunction.splineType.splineType != ESplineType.BSPLINE)
        {
            B.pos = Handles.PositionHandle(B.pos, Quaternion.identity);
            C.pos = Handles.PositionHandle(C.pos, Quaternion.identity);
        }
        D.pos = Handles.PositionHandle(D.pos, Quaternion.identity);
    }

    public void ControlPointsInspector()
    {
        if (!previousJunction)
            A.pos = EditorGUILayout.Vector3Field("Control Point 0", A.pos);
        if (!previousJunction || previousJunction.splineType.splineType != ESplineType.BSPLINE)
        {
            B.pos = EditorGUILayout.Vector3Field("Control Point 1", B.pos);
            C.pos = EditorGUILayout.Vector3Field("Control Point 2", C.pos);
        }
        D.pos = EditorGUILayout.Vector3Field("Control Point 3", D.pos);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(Polynomial))]
public class PolynomialEditor : Editor
{
    private Polynomial self;

    private void OnEnable()
    {
        self = (Polynomial)target;
    }

    private void OnSceneGUI()
    {
        self.DrawMoveTool();
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        // call spline type constructor on inspector update
        self.splineType = new SplineType(self.splineType.splineType);

        self.ControlPointsInspector();
    }
}
#endif