using UnityEngine;

public static class Vector3Extensions
{
    public static bool GetLineLineIntersection(out Vector3 intersection, Vector3 pointA, Vector3 dirA, Vector3 pointB, Vector3 dirB)
    {
        Vector3 pointVec = pointB - pointA;
        Vector3 crossDir = Vector3.Cross(dirA, dirB);
        Vector3 crossPointVec = Vector3.Cross(pointVec, dirB);

        float planarFactor = Vector3.Dot(pointVec, crossDir);

        //is coplanar, and not parallel
        if (Mathf.Abs(planarFactor) < float.Epsilon && crossDir.sqrMagnitude > float.Epsilon)
        {
            float s = Vector3.Dot(crossPointVec, crossDir) / crossDir.sqrMagnitude;
            intersection = pointA + (dirA * s);
            return true;
        }
        else
        {
            intersection = Vector3.zero;
            return false;
        }
    }

    public static float GetTriangularArea(Vector3 pointA, Vector3 pointB, Vector3 pointC)
    {
        Vector3 ab = pointB - pointA;
        Vector3 ac = pointC - pointA;

        return 0.5f * ab.magnitude * ac.magnitude * Mathf.Sin(Vector3.Angle(ab, ac) * Mathf.Deg2Rad);
    }
}
