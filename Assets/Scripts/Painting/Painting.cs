using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Painting : MonoBehaviour
{
    public PaintingCell cellPrefab;
    public FluidSelectionGrid selectionGrid;

    [Range(0.0f, 1.0f)]
    public float shapeBorderWidth = 0.01f;

    [Range(0, 20)]
    public int cellCount = 8;

    public bool square = false;

    [Range(0.0f, 1.0f)]
    public float minimumArea = 0.0f;

    [Range(0.0f, 1.0f)]
    public float minimumEdgeLength = 0.0f;

    [Range(0.0f, 1.0f)]
    public float minRandomRange = 0.0f;

    [Range(0.0f, 1.0f)]
    public float maxRandomRange = 1.0f;

    [Range(1, 10000)]
    public int attempts = 100;

    [Range(1, 5)]
    public int maxColorRepetitionCount = 2;

    public ColorOption[] colorOptions;

    public List<PaintingCell> Cells
    {
        get;
        private set;
    }

    public void Repaint()
    {
        ClearOldPainting();

        List<ConvexPoly> polys = GeneratePolys();
        ColorOption[] pickedColors = GenerateColorPicks(polys.Count);

        InstantiateCells(polys, pickedColors);

        GenerateFluidGrid();
    }

    private void ClearOldPainting()
    {
        if (Cells == null)
        {
            Cells = new List<PaintingCell>();
        }

        foreach (PaintingCell cell in Cells)
        {
            DestroyImmediate(cell.gameObject);
        }
        Cells.Clear();
    }

    private List<ConvexPoly> GeneratePolys()
    {
        List<ConvexPoly> polys = null;
        while (polys == null)
        {
            polys = PaintingGenerator.GeneratePainting(new Vector2(1.0f, 1.0f), cellCount, square, minimumArea, minimumEdgeLength, minRandomRange, maxRandomRange, attempts);
        }

        return polys;
    }

    private ColorOption[] GenerateColorPicks(int polyCount)
    {
        List<ColorOption> restrictedColorOptions = new List<ColorOption>();
        for (int colorRepetition = 0; colorRepetition < maxColorRepetitionCount; ++colorRepetition)
        {
            restrictedColorOptions.AddRange(colorOptions);
        }

        ColorOption[] pickedColors = restrictedColorOptions.RandomPick<ColorOption>(polyCount, true).ToArray();
        return pickedColors;
    }

    private void InstantiateCells(List<ConvexPoly> polys, ColorOption[] pickedColors)
    {
        for (int polyIndex = 0; polyIndex < polys.Count; ++polyIndex)
        {
            ColorOption colorOption = pickedColors[polyIndex];

            PaintingCell cell = Instantiate<PaintingCell>(cellPrefab);
            cell.name = string.Format("Cell #{0}", polyIndex + 1);
            cell.transform.SetParent(transform, false);
            cell.gameObject.SetActive(true);
            cell.Generate(colorOption, polys[polyIndex].GetInsetPoints(shapeBorderWidth * 0.5f));

            Cells.Add(cell);
        }
    }

    private void GenerateFluidGrid()
    {
        Vector2 tempIntersection = Vector2.zero;

        foreach (PaintingCell cell in Cells)
        {
            Vector2 centerPoint = cell.visiblePolyExtrude.CenterPoint;
            float bestUpDelta = float.PositiveInfinity;
            float bestDownDelta = float.PositiveInfinity;
            float bestLeftDelta = float.PositiveInfinity;
            float bestRightDelta = float.PositiveInfinity;

            FluidSelectable fluidSelectable = cell.fluidSelectable;
            fluidSelectable.up = null;
            fluidSelectable.down = null;
            fluidSelectable.left = null;
            fluidSelectable.right = null;

            foreach (PaintingCell otherCell in Cells)
            {
                if (otherCell == cell)
                {
                    continue;
                }

                if (Vector2Extensions.GetLinePolyIntersection(out tempIntersection, centerPoint, Vector2.up, otherCell.visiblePolyExtrude.points))
                {
                    float delta = (tempIntersection - centerPoint).sqrMagnitude;
                    if (delta < bestUpDelta)
                    {
                        bestUpDelta = delta;
                        fluidSelectable.up = otherCell.fluidSelectable;
                    }
                }

                if (Vector2Extensions.GetLinePolyIntersection(out tempIntersection, centerPoint, Vector2.down, otherCell.visiblePolyExtrude.points))
                {
                    float delta = (tempIntersection - centerPoint).sqrMagnitude;
                    if (delta < bestDownDelta)
                    {
                        bestDownDelta = delta;
                        fluidSelectable.down = otherCell.fluidSelectable;
                    }
                }

                if (Vector2Extensions.GetLinePolyIntersection(out tempIntersection, centerPoint, Vector2.left, otherCell.visiblePolyExtrude.points))
                {
                    float delta = (tempIntersection - centerPoint).sqrMagnitude;
                    if (delta < bestLeftDelta)
                    {
                        bestLeftDelta = delta;
                        fluidSelectable.left = otherCell.fluidSelectable;
                    }
                }

                if (Vector2Extensions.GetLinePolyIntersection(out tempIntersection, centerPoint, Vector2.right, otherCell.visiblePolyExtrude.points))
                {
                    float delta = (tempIntersection - centerPoint).sqrMagnitude;
                    if (delta < bestRightDelta)
                    {
                        bestRightDelta = delta;
                        fluidSelectable.right = otherCell.fluidSelectable;
                    }
                }
            }
        }
    }
}
