using UnityEngine;

public static class Vector2Extensions
{ 
    public static bool GetLinePolyIntersection(out Vector2 bestIntersection, Vector2 point, Vector2 direction, Vector2[] polyPoints)
    {
        float shortestDelta = float.PositiveInfinity;
        bestIntersection = Vector2.zero;

        for (int edgeIndex = 0; edgeIndex < polyPoints.Length; ++edgeIndex)
        {
            Vector3 intersection;

            Vector2 edgePointA = polyPoints[edgeIndex];
            Vector2 edgePointB = polyPoints[(edgeIndex + 1) % polyPoints.Length];
            Vector2 edgeDirection = edgePointB - edgePointA;

            if (Vector3Extensions.GetLineLineIntersection(out intersection, point, direction, edgePointA, edgeDirection))
            {
                Vector2 intersection2D = intersection;

                float dotProduct = Vector2.Dot(intersection2D - point, direction);
                if (dotProduct < 0.0f || dotProduct > direction.sqrMagnitude)
                {
                    continue;
                }

                dotProduct = Vector2.Dot(intersection2D - edgePointA, edgeDirection);
                if (dotProduct < 0.0f || dotProduct > edgeDirection.sqrMagnitude)
                {
                    continue;
                }

                float delta = (intersection2D - point).sqrMagnitude;
                if (delta < shortestDelta)
                {
                    shortestDelta = delta;
                    bestIntersection = intersection2D;
                }
            }
        }

        return shortestDelta != float.PositiveInfinity;
    }
}
