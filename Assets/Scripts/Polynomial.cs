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

public class Polynomial : MonoBehaviour
{
    // degree is useless for now
    [SerializeField]
    private int degree = 3;
    public SplineType splineType = new SplineType(ESplineType.BEZIER);

    // spline resolution
    [SerializeField]
    private int knots = 99;

    [SerializeField]
    private ControlPoint CP0;
    [SerializeField]
    private ControlPoint CP1;
    [SerializeField]
    private ControlPoint CP2;
    [SerializeField]
    private ControlPoint CP3;

    public Vector3 SplinePolynomial(float t)
    {
        float t3 = t * t * t;
        float t2 = t * t;
        Vector4 T = new Vector4(t3, t2, t, 1f);

        // S = T * M * G
        return (splineType.G(CP0.pos, CP1.pos, CP2.pos, CP3.pos) * splineType.M).MultiplyPoint(T);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;

        for (int i = 0; i < knots; ++i)
        {
            float t = i / (float)knots;
            float tt = (i + 1) / (float)knots;
            Gizmos.DrawLine(SplinePolynomial(t), SplinePolynomial(tt));
        }

        if (splineType.splineType == ESplineType.HERMITE)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(CP0.pos, CP1.pos);
            Gizmos.DrawLine(CP2.pos, CP3.pos);
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(Polynomial))]
public class PolynomialEditor : Editor
{
    private GameObject gameObject;
    private Polynomial self;

    private void OnEnable()
    {
        gameObject = target.GameObject();
        self = gameObject.GetComponentInParent<Polynomial>();
    }

    private void OnSceneGUI()
    {
        for (int i = 0; i < gameObject.transform.childCount; ++i)
        {
            GameObject child = gameObject.transform.GetChild(i).gameObject;
        }
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        // call spline type constructor on inspector update
        self.splineType = new SplineType(self.splineType.splineType);
    }
}
#endif