using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class ContinuousSpline : MonoBehaviour
{
    public ESplineType splineType = ESplineType.BEZIER;
    private int splineCount = 0;

    public void AddPolynomial()
    {
        string name = "none";
        switch (splineType)
        {
            case ESplineType.HERMITE:
                name = "hermite";
                break;
            case ESplineType.BEZIER:
                name = "bezier";
                break;
            case ESplineType.BSPLINE:
                name = "b-spline";
                break;
            case ESplineType.CATMULLROM:
                name = "catmull-rom";
                break;
            default:
                break;
        }

        GameObject spline = new GameObject(name);
        Polynomial p = spline.AddComponent<Polynomial>();
        p.InitSpline(splineType);
        if (splineCount > 0)
            p.SetPreviousJunction(GetPolynomialAtIndex(splineCount - 1));

        // add to hierarchy
        spline.transform.parent = transform;
        p.id = splineCount++;
    }

    public void RemovePolynomial()
    {
        DestroyImmediate(transform.GetChild(splineCount - 1).gameObject);
        --splineCount;
    }

    public void ChangeSplineType()
    {
        for (int i = 0; i < transform.childCount; ++i)
        {
            GetPolynomialAtIndex(i).splineType = new SplineType(splineType);
        }
    }

    public Polynomial GetPolynomialAtIndex(int index)
    {
        return transform.GetChild(index).GetComponent<Polynomial>();
    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < transform.childCount; ++i)
        {
            GetPolynomialAtIndex(i).DrawSpline();
        }
    }

    private void OnDrawGizmosSelected()
    {
        for (int i = 0; i < transform.childCount; ++i)
        {
            GetPolynomialAtIndex(i).DrawDebugSpline();
        }
    }

    public void DrawMoveTools()
    {
        for (int i = 0; i < transform.childCount; ++i)
        {
            GetPolynomialAtIndex(i).DrawMoveTool();
        }
    }

    public void ControlPointsInspector()
    {
        for (int i = 0; i < transform.childCount; ++i)
        {
            GetPolynomialAtIndex(i).ControlPointsInspector();
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(ContinuousSpline))]
public class ContinuousSplineEditor : Editor
{
    private ContinuousSpline self;

    private void OnEnable()
    {
        self = (ContinuousSpline)target;
    }

    private void OnSceneGUI()
    {
        self.DrawMoveTools();
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Add spline"))
        {
            self.AddPolynomial();
        }
        if (GUILayout.Button("Remove spline"))
        {
            self.RemovePolynomial();
        }
        if (GUILayout.Button("Apply spline type to all splines"))
        {
            self.ChangeSplineType();
        }

        self.ControlPointsInspector();
    }
}
#endif