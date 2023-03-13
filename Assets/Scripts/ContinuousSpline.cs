using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Build.Content;
using UnityEngine;

public class ContinuousSpline : MonoBehaviour
{
    public ESplineType splineType = ESplineType.BEZIER;
    public EContinuity continuity = EContinuity.C0;
    public int splineCount = 0;

    // in-game rendering
    public bool renderSpline = true;
    public int lineRes = 99;
    [SerializeField]
    private Material lineRendererMat;
    [SerializeField]
    private Color lineColor = Color.white;
    private LineRenderer lr;

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
        p.InitSpline(splineType, continuity);
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
            GetPolynomialAtIndex(i).InitSpline(splineType);
    }

    public void ChangeContinuity()
    {
        for (int i = 0; i < transform.childCount; ++i)
            GetPolynomialAtIndex(i).InitSpline(ESplineType.COUNT, continuity);
    }

    public Polynomial GetPolynomialAtIndex(int index)
    {
        if (index >= transform.childCount)
            return null;

        Transform child = transform.GetChild(index);
        Polynomial p;
        bool valid = child.TryGetComponent<Polynomial>(out p);
        return valid ? p : null;
    }

    static private float Remap(float x, float a, float b, float c, float d)
    {
        return c + (x - a) * (d - c) / (b - a);
    }

    public Vector3 S(float t)
    {
        t = Mathf.Clamp01(t);
        float rt = Remap(t, 0f, 1f, 0f, splineCount);

        // decimal part of t
        float d = rt - (int)rt;

        // spline index;
        int index = (int)rt;

        // last point of spline
        if (index != 0 && rt % 1f == 0f)
        {
            --index;
            d = 1f;
        }

        return GetPolynomialAtIndex(index).SplinePolynomial(d);
    }

    private void UpdateSpline()
    {
        for (int i = 0; i < transform.childCount; ++i)
            GetPolynomialAtIndex(i).UpdateControlPoints();
    }

    private void Start()
    {
        UpdateSpline();

        GameObject go = new GameObject("LineRenderer");
        lr = go.AddComponent<LineRenderer>();

        lr.positionCount = lineRes;
        lr.material = lineRendererMat;
        lr.startWidth = 0.1f;
        lr.endWidth = 0.1f;
        lr.startColor = lineColor;
        lr.endColor = lineColor;

        for (int i = 0; i < lineRes; ++i)
        {
            float t = i / (float)(lineRes - 1);
            lr.SetPosition(i, S(t));
        }
    }

    private void OnDrawGizmos()
    {
        UpdateSpline();

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
            if (self.splineCount <= 0)
            {
                Debug.Log("No spline");
                return;
            }

            self.RemovePolynomial();
        }
        if (GUILayout.Button("Apply spline type to all splines"))
        {
            self.ChangeSplineType();
        }
        if (GUILayout.Button("Apply continuity settings"))
        {
            self.ChangeContinuity();
        }

        self.ControlPointsInspector();
    }
}
#endif