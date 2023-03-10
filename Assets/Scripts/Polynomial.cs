using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class Polynomial : MonoBehaviour
{
    [SerializeField]
    private int degree = 3;
    private Matrix4x4 splineType = SplineType.Bezier;

    // spline resolution
    [SerializeField]
    private int knots = 99;
    [SerializeField]
    private List<Vector3> controlPoints = new List<Vector3>();

    // Start is called before the first frame update
    void Start()
    {
        //for (int i = 0; i < degree + 1; ++i)
        //{
        //    controlPoints.Add(Vector3.zero);
        //}
    }

    public Vector3 SplinePolynomial(float t)
    {
        float t3 = t * t * t;
        float t2 = t * t;
        Vector4 T = new Vector4(t3, t2, t, 1f);

        Matrix4x4 M = splineType;

        Vector3 P0 = controlPoints[0];
        Vector3 P1 = controlPoints[1];
        Vector3 P2 = controlPoints[2];
        Vector3 P3 = controlPoints[3];
        Matrix4x4 G = new Matrix4x4(
            new Vector4(P0.x, P0.y, P0.z, 1f),
            new Vector4(P1.x, P1.y, P1.z, 1f),
            new Vector4(P2.x, P2.y, P2.z, 1f),
            new Vector4(P3.x, P3.y, P3.z, 1f)
            );

        // S = T * M * G
        return (G * M).MultiplyPoint(T);
    }

    // Update is called once per frame
    void Update()
    {
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

    private void OnDrawGizmosSelected()
    {
        // draw gizmos on control points
    }
}