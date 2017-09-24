using System;
using UnityEngine;

public class FluidSelectionGrid : MonoBehaviour
{
    public KMSelectable parentSelectable = null;

    public void SetChildren(FluidSelectable[] childSelectables)
    {
        int reserveChildCount = childSelectables.Length;
        if (reserveChildCount % 3 != 0)
        {
            reserveChildCount += 3 - (reserveChildCount % 3);
        }

        int finalChildCount = reserveChildCount + 12;

        parentSelectable.Children = new KMSelectable[finalChildCount];
        parentSelectable.ChildRowLength = 3;

        for (int childSelectableIndex = 0; childSelectableIndex < childSelectables.Length; ++childSelectableIndex)
        {
            FluidSelectable childSelectableClosure = childSelectables[childSelectableIndex];
            childSelectableClosure.selectable.OnSelect += () => OnSelect(childSelectableClosure);
            parentSelectable.Children[childSelectableIndex] = childSelectableClosure.selectable;
        }

        OnSelect(null);
    }

    private void OnSelect(FluidSelectable currentSelectable)
    {
        KMSelectable[] children = parentSelectable.Children;

        int reserveChildCount = children.Length - 12;
        Array.Clear(children, reserveChildCount, 12);

        KMSelectable selectable = currentSelectable != null ? currentSelectable.selectable : null;

        children[reserveChildCount] = selectable;
        children[reserveChildCount + 1] = selectable;
        children[reserveChildCount + 2] = selectable;
        children[reserveChildCount + 7] = selectable;

        if (currentSelectable != null)
        {
            children[reserveChildCount + 4] = currentSelectable.up != null ? currentSelectable.up.selectable : null;
            children[reserveChildCount + 6] = currentSelectable.left != null ? currentSelectable.left.selectable : null;
            children[reserveChildCount + 8] = currentSelectable.right != null ? currentSelectable.right.selectable : null;
            children[reserveChildCount + 10] = currentSelectable.down != null ? currentSelectable.down.selectable : null;
        }

        parentSelectable.UpdateChildren(selectable);
    }
}
