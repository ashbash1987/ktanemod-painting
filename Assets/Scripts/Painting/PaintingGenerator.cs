using System.Collections.Generic;
using UnityEngine;

public static class PaintingGenerator
{
    public static List<ConvexPoly> GeneratePainting(Vector2 canvasSize, int polyCount, bool square, float minimumArea, float minimumEdgeLength, float minRandomRange, float maxRandomRange, int attemptCount)
    {
        List<Vector2> points = new List<Vector2>();
        points.Add(new Vector2(canvasSize.x * -0.5f, canvasSize.y * 0.5f));
        points.Add(new Vector2(canvasSize.x * 0.5f, canvasSize.y * 0.5f));
        points.Add(new Vector2(canvasSize.x * 0.5f, canvasSize.y * -0.5f));
        points.Add(new Vector2(canvasSize.x * -0.5f, canvasSize.y * -0.5f));

        ConvexPoly baseCanvasPoly = new ConvexPoly() { Points = points };

        List<ConvexPoly> convexPolys = new List<ConvexPoly>();
        convexPolys.Add(baseCanvasPoly);

        while (attemptCount > 0 && convexPolys.Count < polyCount)
        {
            ConvexPoly polyToSplit = convexPolys.MaxBy((x) => x.Area);

            while (attemptCount > 0)
            {
                ConvexPoly candidatePolyA;
                ConvexPoly candidatePolyB;

                if (square)
                {
                    float randomValue = Random.Range(minRandomRange, maxRandomRange);
                    polyToSplit.RandomSplitPoly(square, out candidatePolyA, out candidatePolyB, randomValue, 1.0f - randomValue);
                }
                else
                {
                    polyToSplit.RandomSplitPoly(square,  out candidatePolyA, out candidatePolyB, Random.Range(minRandomRange, maxRandomRange), Random.Range(minRandomRange, maxRandomRange));
                }

                if (Mathf.Min(candidatePolyA.Area, candidatePolyB.Area) >= minimumArea &&
                    candidatePolyA.ShortestEdgeLength >= minimumEdgeLength &&
                    candidatePolyB.ShortestEdgeLength >= minimumEdgeLength)
                {
                    polyToSplit.Points = candidatePolyA.Points;
                    convexPolys.Add(candidatePolyB);
                    break;
                }

                attemptCount--;
            }
        }

        if (attemptCount > 0)
        {
            return convexPolys;
        }

        return null;
    }
}
