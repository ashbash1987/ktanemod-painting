using System.Collections;
using UnityEngine;

public class PaintingCell : MonoBehaviour
{
    public FluidSelectable fluidSelectable = null;

    public PolyExtrude visiblePolyExtrude = null;
    public PolyExtrude highlightablePolyExtrude = null;

	public TextMesh twitchPlaysLabel = null;

    public ColorOption ColorOption
    {
        get
        {
            return _colorOption;
        }
        set
        {
            if (_colorOption != value)
            {
                UpdateRendererColors(_colorOption.GetColor(), value.GetColor());
                _colorOption = value;

                StartCoroutine(ColorChangeCoroutine());
            }
        }
    }

    public ColorOption? FinalColorOption
    {
        get;
        set;
    }

    private MeshFilter _filter = null;
    private MeshCollider _collider = null;

    private MeshRenderer _renderer = null;
    private MaterialPropertyBlock _materialBlock = null;

    private ColorOption _colorOption = ColorOption.Black;

    private static int _colorAPropertyID = -1;
    private static int _colorBPropertyID = -1;
    private static int _cutoffPropertyID = -1;
    private static int _cutoffRampPropertyID = -1;

    private void Awake()
    {
        _filter = GetComponent<MeshFilter>();
        _renderer = GetComponent<MeshRenderer>();
        _collider = GetComponent<MeshCollider>();

        _materialBlock = new MaterialPropertyBlock();
        _renderer.SetPropertyBlock(_materialBlock);
    }

    public void Generate(ColorOption colorOption, Vector2[] points)
    {
        RebuildMeshes(points);
        EnsureInternalHighlightable();

        UpdateCollider();

        ColorOption = colorOption;
        UpdateRendererColors(ColorOption.GetColor(), ColorOption.GetColor());
        UpdateRendererCutoff(0.0f, 0.0f);
    }

    private void RebuildMeshes(Vector2[] points)
    {
        visiblePolyExtrude.points = points;
        visiblePolyExtrude.Rebuild();

        highlightablePolyExtrude.points = visiblePolyExtrude.points;
        highlightablePolyExtrude.Rebuild();
    }

    private void EnsureInternalHighlightable()
    {
        //This is required magic to get the game-side highlightable to recognise the dynamically-generated mesh.
        //Super clunky with the find-by-name stuff, but it works.
        KMHighlightable highlight = fluidSelectable.selectable.Highlight;
        Transform internalHighlight = highlight.transform.Find("Highlight(Clone)");
        if (internalHighlight != null)
        {
            MeshFilter modHighlightMeshFilter = highlight.GetComponent<MeshFilter>();
            MeshFilter internalHighlightMeshFilter = internalHighlight.GetComponent<MeshFilter>();
            internalHighlightMeshFilter.mesh = modHighlightMeshFilter.mesh;
        }
    }

    private void UpdateCollider()
    {
        if (_filter == null)
        {
            _filter = GetComponent<MeshFilter>();
        }

        if (_collider == null)
        {
            _collider = GetComponent<MeshCollider>();
        }

        _collider.sharedMesh = _filter.mesh;
    }

    private void UpdateRendererColors(Color startColor, Color endColor)
    {
        EnsureMaterialBlock();
        EnsureShaderProperties();

        _materialBlock.SetColor(_colorAPropertyID, startColor);
        _materialBlock.SetColor(_colorBPropertyID, endColor);
        _renderer.SetPropertyBlock(_materialBlock);
    }

    private void UpdateRendererCutoff(float cutoff, float cutoffRamp)
    {
        EnsureMaterialBlock();
        EnsureShaderProperties();

        _materialBlock.SetFloat(_cutoffPropertyID, cutoff);
        _materialBlock.SetFloat(_cutoffRampPropertyID, cutoffRamp);
        _renderer.SetPropertyBlock(_materialBlock);
    }

    private void EnsureMaterialBlock()
    {
        if (_renderer == null)
        {
            _renderer = GetComponent<MeshRenderer>();
            _materialBlock = new MaterialPropertyBlock();
        }
    }

    private void EnsureShaderProperties()
    {
        if (_colorAPropertyID == -1)
        {
            _colorAPropertyID = Shader.PropertyToID("_ColorA");
        }
        if (_colorBPropertyID == -1)
        {
            _colorBPropertyID = Shader.PropertyToID("_ColorB");
        }
        if (_cutoffPropertyID == -1)
        {
            _cutoffPropertyID = Shader.PropertyToID("_Cutoff");
        }
        if (_cutoffRampPropertyID == -1)
        {
            _cutoffRampPropertyID = Shader.PropertyToID("_CutoffRamp");
        }
    }

    private IEnumerator ColorChangeCoroutine()
    {
        const float changeTime = 0.5f;

        float startTime = Time.time;

        while (Time.time - startTime < changeTime)
        {
            float cutoff = (Time.time - startTime) / changeTime;
            UpdateRendererCutoff(cutoff, Mathf.Lerp(0.1f, 0.0f, cutoff));

            yield return null;
        }

        UpdateRendererCutoff(1.0f, 0.0f);
    }
}
