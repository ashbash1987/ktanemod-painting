using System;
using UnityEngine;

public class ColorAttribute : Attribute
{
    public ColorAttribute(int r, int g, int b)
    {
        Color = new Color(r / 255.0f, g / 255.0f, b / 255.0f);
    }

    public readonly Color Color;
}

public static class ColorAttributeExtensions
{
    public static Color GetColor(this Enum enumVal)
    {
        return enumVal.GetAttributeOfType<ColorAttribute>().Color;
    }
}
