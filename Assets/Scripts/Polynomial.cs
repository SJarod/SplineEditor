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
    public int id = 0;

    // degree is useless for now
    [SerializeField]
    private int degree = 3;
    [SerializeField]
    private SplineType splineType = new SplineType(ESplineType.BEZIER);
    [SerializeField]
    private EContinuity continuity = EContinuity.C0;

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

    public void InitSpline(ESplineType st = ESplineType.COUNT, EContinuity c = EContinuity.COUNT)
    {
        {
            ESplineType reinit = splineType.splineType;
            if (st != ESplineType.COUNT)
                reinit = st;
            splineType = new SplineType(reinit);
        }
        {
            EContinuity reinit = continuity;
            if (c != EContinuity.COUNT)
                reinit = c;
            continuity = reinit;
        }
    }

    public void SetPreviousJunction(Polynomial prev)
    {
        previousJunction = prev;
        A.pos = prev.D.pos;
        B.pos = prev.D.pos;
        C.pos = prev.D.pos;
        D.pos = prev.D.pos;
    }

    private void UpdateControlPoints()
    {
        // take previous junction control points if possible (to ensure continuity)
        if (!previousJunction)
            return;

        switch (splineType.splineType)
        {
            case ESplineType.HERMITE:
                switch (continuity)
                {
                    case EContinuity.C1:
                        break;
                    case EContinuity.C2:
                        B = previousJunction.C;
                        break;
                }
                A = previousJunction.D;
                break;
            case ESplineType.BEZIER:
                switch (continuity)
                {
                    case EContinuity.C1:
                    case EContinuity.C2:
                        break;
                }
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

        // S = T * M * G
        return (splineType.G(A.pos, B.pos, C.pos, D.pos) * splineType.M).MultiplyPoint(T);
    }

    public void DrawSpline()
    {
        UpdateControlPoints();

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
        switch (splineType.splineType)
        {
            case ESplineType.HERMITE:
                Gizmos.color = Color.red;
                Gizmos.DrawLine(A.pos, B.pos);
                Gizmos.DrawLine(C.pos, D.pos);
                break;
            case ESplineType.BEZIER:
                Gizmos.color = Color.red;
                Gizmos.DrawLine(A.pos, B.pos);
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(B.pos, C.pos);
                Gizmos.color = Color.red;
                Gizmos.DrawLine(C.pos, D.pos);
                break;
            case ESplineType.BSPLINE:
            case ESplineType.CATMULLROM:
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(A.pos, B.pos);
                Gizmos.DrawLine(B.pos, C.pos);
                Gizmos.DrawLine(C.pos, D.pos);
                break;
            default:
                break;
        }

        // main control points
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(A.pos, 1f);
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
        {
            A.pos = Handles.PositionHandle(A.pos, Quaternion.identity);
            B.pos = Handles.PositionHandle(B.pos, Quaternion.identity);
            C.pos = Handles.PositionHandle(C.pos, Quaternion.identity);
            D.pos = Handles.PositionHandle(D.pos, Quaternion.identity);
            return;
        }

        switch (previousJunction.splineType.splineType)
        {
            case ESplineType.HERMITE:
                if (continuity != EContinuity.C2)
                    B.pos = Handles.PositionHandle(B.pos, Quaternion.identity);
                C.pos = Handles.PositionHandle(C.pos, Quaternion.identity);
                break;
            case ESplineType.BEZIER:
                B.pos = Handles.PositionHandle(B.pos, Quaternion.identity);
                C.pos = Handles.PositionHandle(C.pos, Quaternion.identity);
                break;
            case ESplineType.BSPLINE:
            case ESplineType.CATMULLROM:
                break;
            default:
                A.pos = Handles.PositionHandle(A.pos, Quaternion.identity);
                B.pos = Handles.PositionHandle(B.pos, Quaternion.identity);
                C.pos = Handles.PositionHandle(C.pos, Quaternion.identity);
                break;
        }
        D.pos = Handles.PositionHandle(D.pos, Quaternion.identity);
    }

    public void ControlPointsInspector()
    {
        string name = "Spline " + id;

        if (!previousJunction)
        {
            A.pos = EditorGUILayout.Vector3Field(name + " : 0", A.pos);
            B.pos = EditorGUILayout.Vector3Field(name + " : 1", B.pos);
            C.pos = EditorGUILayout.Vector3Field(name + " : 2", C.pos);
            D.pos = EditorGUILayout.Vector3Field(name + " : 3", D.pos);
            return;
        }

        switch (previousJunction.splineType.splineType)
        {
            case ESplineType.HERMITE:
            case ESplineType.BEZIER:
                B.pos = EditorGUILayout.Vector3Field(name + " : 1", B.pos);
                C.pos = EditorGUILayout.Vector3Field(name + " : 2", C.pos);
                break;
            case ESplineType.BSPLINE:
            case ESplineType.CATMULLROM:
                break;
            default:
                A.pos = EditorGUILayout.Vector3Field(name + " : 0", A.pos);
                B.pos = EditorGUILayout.Vector3Field(name + " : 1", B.pos);
                C.pos = EditorGUILayout.Vector3Field(name + " : 2", C.pos);
                break;
        }
        D.pos = EditorGUILayout.Vector3Field(name + " : 3", D.pos);
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
        self.InitSpline();

        self.ControlPointsInspector();
    }
}
#endif