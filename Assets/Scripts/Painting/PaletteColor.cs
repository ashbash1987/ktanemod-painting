using UnityEngine;

public sealed class PaletteColor : MonoBehaviour
{
    public enum Direction
    {
        Down,
        Left,
    }

    public ColorOption colorOption = ColorOption.Black;
    public FluidSelectable fluidSelectable = null;

    public Vector2 paintingPoint = Vector2.zero;
    public Direction paintingDirection = Direction.Down;

    private MeshRenderer _renderer = null;
    private MaterialPropertyBlock _materialBlock = null;

    private static int _colorPropertyID = -1;

    private void Awake()
    {
        UpdateRenderer();
    }

    private void UpdateRenderer()
    {
        if (_renderer == null)
        {
            _renderer = GetComponent<MeshRenderer>();
            _materialBlock = new MaterialPropertyBlock();
        }

        if (_colorPropertyID == -1)
        {
            _colorPropertyID = Shader.PropertyToID("_Color");
        }

        _materialBlock.SetColor(_colorPropertyID, colorOption.GetColor());
        _renderer.SetPropertyBlock(_materialBlock);
    }
}
