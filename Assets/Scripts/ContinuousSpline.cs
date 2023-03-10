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
        GameObject spline = new GameObject();
        Polynomial p = spline.AddComponent<Polynomial>();
        p.InitSpline(splineType);
        if (splineCount > 0)
            p.SetPreviousJunction(GetPolynomialAtIndex(splineCount - 1));

        // add to hierarchy
        spline.transform.parent = transform;
        ++splineCount;
    }

    public void RemovePolynomial()
    {
        DestroyImmediate(transform.GetChild(splineCount - 1).gameObject);
        --splineCount;
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

        self.ControlPointsInspector();
    }
}
#endif