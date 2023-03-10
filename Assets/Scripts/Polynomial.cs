using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
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
    [SerializeField]
    private SplineType splineType = SplineType.Bezier;

    // spline resolution
    [SerializeField]
    private int knots = 99;

    [SerializeField]
    private List<ControlPoint> controlPoints = new List<ControlPoint>();

    public Vector3 SplinePolynomial(float t)
    {
        float t3 = t * t * t;
        float t2 = t * t;
        Vector4 T = new Vector4(t3, t2, t, 1f);

        Vector3 A = controlPoints[0].pos;
        Vector3 B = controlPoints[1].pos;
        Vector3 C = controlPoints[2].pos;
        Vector3 D = controlPoints[3].pos;

        // S = T * M * G
        return (splineType.G(A, B, C, D) * splineType.M).MultiplyPoint(T);
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
}
#endif