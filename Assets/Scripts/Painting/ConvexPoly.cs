using System.Collections.Generic;
using UnityEngine;

public class ConvexPoly
{
    private List<Vector2> _points = null;
    public List<Vector2> Points
    {
        get
        {
            return _points;
        }
        set
        {
            _points = value;
            RecalculateArea();
        }
    }

    public float Area
    {
        get
        {
            return _area;
        }
    }

    public float ShortestEdgeLength
    {
        get
        {
            float shortestEdgeLength = float.PositiveInfinity;
            for (int shapePointIndex = 0; shapePointIndex < Points.Count; ++shapePointIndex)
            {
                shortestEdgeLength = Mathf.Min(shortestEdgeLength, Vector2.Distance(Points[shapePointIndex], Points[(shapePointIndex + 1) % Points.Count]));
            }

            return shortestEdgeLength;
        }
    }

    private float _area = 0.0f;

    public void RandomSplitPoly(bool square, out ConvexPoly leftPoly, out ConvexPoly rightPoly, float edgeADelta, float edgeBDelta)
    {
        int edgeAIndex = Random.Range(0, Points.Count);
        int edgeBIndex = 0;

        if (square)
        {
            edgeBIndex = (edgeAIndex + 2) % Points.Count;
        }
        else
        {
            edgeBIndex = (edgeAIndex + Random.Range(1, Points.Count)) % Points.Count;
        }

        if (edgeBIndex < edgeAIndex)
        {
            int temp = edgeAIndex;
            edgeAIndex = edgeBIndex;
            edgeBIndex = temp;
        }

        SplitPoly(edgeAIndex, edgeADelta, edgeBIndex, edgeBDelta, out leftPoly, out rightPoly);
    }

    public void SplitPoly(int edgeAIndex, float edgeADelta, int edgeBIndex, float edgeBDelta, out ConvexPoly leftPoly, out ConvexPoly rightPoly)
    {
        Vector2 newPointA = Vector2.Lerp(Points[edgeAIndex], Points[(edgeAIndex + 1) % Points.Count], edgeADelta);
        Vector2 newPointB = Vector2.Lerp(Points[edgeBIndex], Points[(edgeBIndex + 1) % Points.Count], edgeBDelta);

        List<Vector2> leftPoints = Points.GetRange(0, edgeAIndex + 1);
        leftPoints.Add(newPointA);
        leftPoints.Add(newPointB);
        if (edgeBIndex < Points.Count - 1)
        {
            leftPoints.AddRange(Points.GetRange(edgeBIndex + 1, Points.Count - (edgeBIndex + 1)));
        }

        List<Vector2> rightPoints = Points.GetRange(edgeAIndex + 1, edgeBIndex - edgeAIndex);
        rightPoints.Insert(0, newPointA);
        rightPoints.Add(newPointB);

        leftPoly = new ConvexPoly() { Points = leftPoints };
        rightPoly = new ConvexPoly() { Points = rightPoints };
    }

    public Vector2[] GetInsetPoints(float inset)
    {
        int pointCount = Points.Count;

        Vector2[] insetPoints = new Vector2[pointCount];

        for (int shapePointIndex = 0; shapePointIndex < pointCount; ++shapePointIndex)
        {
            Vector2 shapePrePoint = Points[(shapePointIndex + pointCount - 1) % pointCount];
            Vector2 shapePoint = Points[shapePointIndex];
            Vector2 shapePostPoint = Points[(shapePointIndex + 1) % pointCount];

            Vector2 preVector = shapePoint - shapePrePoint;
            Vector2 preVectorRight;
            preVectorRight.x = preVector.y;
            preVectorRight.y = -preVector.x;
            preVectorRight.Normalize();
            preVectorRight *= inset;

            Vector2 postVector = shapePostPoint - shapePoint;
            Vector2 postVectorRight;
            postVectorRight.x = postVector.y;
            postVectorRight.y = -postVector.x;
            postVectorRight.Normalize();
            postVectorRight *= inset;

            Vector3 intersect;
            Vector3Extensions.GetLineLineIntersection(out intersect, shapePrePoint + preVectorRight, preVector, shapePoint + postVectorRight, postVector);
            insetPoints[shapePointIndex] = intersect;
        }

        return insetPoints;
    }

    private void RecalculateArea()
    {
        _area = 0.0f;
        for (int pointIndex = 2; pointIndex < Points.Count; ++pointIndex)
        {
            _area += Vector3Extensions.GetTriangularArea(Points[0], Points[pointIndex - 1], Points[pointIndex]);
        }
    }
}
