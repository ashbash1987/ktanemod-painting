using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public class PaintingModule : MonoBehaviour
{
    private readonly ColorBlindSet[] ColorBlindSets = new ColorBlindSet[]
    {
        new ColorBlindSet(
            "Protanomaly",
            new ColorBlindSet.ColorSwap(ColorOption.Black, ColorOption.Red),
            new ColorBlindSet.ColorSwap(ColorOption.Brown, ColorOption.Green),
            new ColorBlindSet.ColorSwap(ColorOption.Orange, ColorOption.Red),
            new ColorBlindSet.ColorSwap(ColorOption.Blue, ColorOption.Red),
            new ColorBlindSet.ColorSwap(ColorOption.Green, ColorOption.Orange),
            new ColorBlindSet.ColorSwap(ColorOption.Purple, ColorOption.Pink),
            new ColorBlindSet.ColorSwap(ColorOption.Pink, ColorOption.Purple)
        ),
        new ColorBlindSet(
            "Deuteranomaly",
            new ColorBlindSet.ColorSwap(ColorOption.Red, ColorOption.Green),
            new ColorBlindSet.ColorSwap(ColorOption.Blue, ColorOption.Pink),
            new ColorBlindSet.ColorSwap(ColorOption.Green, ColorOption.Yellow),
            new ColorBlindSet.ColorSwap(ColorOption.Yellow, ColorOption.Green),
            new ColorBlindSet.ColorSwap(ColorOption.Pink, ColorOption.Grey),
            new ColorBlindSet.ColorSwap(ColorOption.Purple, ColorOption.Brown),
            new ColorBlindSet.ColorSwap(ColorOption.Brown, ColorOption.Purple)
        ),
        new ColorBlindSet(
            "Tritanopia",
            new ColorBlindSet.ColorSwap(ColorOption.Blue, ColorOption.Grey),
            new ColorBlindSet.ColorSwap(ColorOption.Grey, ColorOption.Blue),
            new ColorBlindSet.ColorSwap(ColorOption.Purple, ColorOption.Black),
            new ColorBlindSet.ColorSwap(ColorOption.Black, ColorOption.Purple),
            new ColorBlindSet.ColorSwap(ColorOption.Green, ColorOption.Blue),
            new ColorBlindSet.ColorSwap(ColorOption.Orange, ColorOption.Red),
            new ColorBlindSet.ColorSwap(ColorOption.Red, ColorOption.Orange)
        )
    };

    private KMBombModule _bombModule = null;
    private KMBombInfo _bombInfo = null;
    private KMSelectable _selectable = null;
    private Painting _painting = null;
    private PaletteColor[] _paletteColors = null;
    private ColorOption? _activeColor = null;

    private void Awake()
    {
        _bombModule = GetComponent<KMBombModule>();
        _bombInfo = GetComponent<KMBombInfo>();
        _selectable = GetComponent<KMSelectable>();
        _painting = GetComponentInChildren<Painting>();

        _bombModule.GenerateLogFriendlyName();
        _bombModule.OnActivate += OnActivate;

        _paletteColors = GetComponentsInChildren<PaletteColor>();
        foreach (PaletteColor paletteColor in _paletteColors)
        {
            PaletteColor closurePaletteColor = paletteColor;
            closurePaletteColor.fluidSelectable.selectable.OnInteract += delegate ()
            {
                _activeColor = closurePaletteColor.colorOption;
                return true;
            };
        }

        Repaint();
    }

    private void OnActivate()
    {
        ColorBlindSet activeSet = DetermineActiveColorBlindSet();
        SetupPaintingFinalColors(activeSet);

        if (TwitchPlaysActive)
        {
            TextMesh cellLabel = _painting.Cells[0].twitchPlaysLabel;
            cellLabel.GetComponent<Renderer>().sharedMaterial.mainTexture = cellLabel.font.material.mainTexture;

            ShuffleTwitchPlayLabels();

            foreach (PaintingCell paintingCell in _painting.Cells)
            {
                GameObject gameObject = paintingCell.twitchPlaysLabel.gameObject;
                Vector2[] points = paintingCell.visiblePolyExtrude.points;
                Vector2 center = points.Aggregate((a, b) => a + b) / points.Length;
                gameObject.transform.localPosition = new Vector3(center.x, 0.025f, center.y);

                gameObject.SetActive(true);
            }
        }
    }

    private void Repaint()
    {
        _bombModule.Log("Generating painting...");

        _painting.Repaint();

        SetupPaletteToPaintingLinks();

        _painting.selectionGrid.SetChildren(GetComponentsInChildren<FluidSelectable>());

        foreach (PaintingCell cell in _painting.Cells)
        {
            PaintingCell closureCell = cell;

            _bombModule.LogFormat("{0} => {1}.", closureCell.name, cell.ColorOption);

            closureCell.fluidSelectable.selectable.Parent = _selectable;
            closureCell.fluidSelectable.selectable.OnInteract += () => OnCellInteract(closureCell);
        }
    }

    private bool SetupPaletteToPaintingLinks()
    {
        Vector2 tempIntersection = Vector2.zero;

        foreach (PaletteColor paletteColor in _paletteColors)
        {
            Vector2 direction = Vector2.zero;
            switch (paletteColor.paintingDirection)
            {
                case PaletteColor.Direction.Down:
                    direction = Vector2.down;
                    break;

                case PaletteColor.Direction.Left:
                    direction = Vector2.left;
                    break;

                default:
                    continue;
            }

            FluidSelectable paletteFluidSelectable = paletteColor.fluidSelectable;

            float bestDelta = float.PositiveInfinity;
            FluidSelectable bestCellFluidSelectable = null;

            foreach (PaintingCell cell in _painting.Cells)
            {
                if (Vector2Extensions.GetLinePolyIntersection(out tempIntersection, paletteColor.paintingPoint, direction, cell.visiblePolyExtrude.points))
                {
                    float delta = (tempIntersection - paletteColor.paintingPoint).sqrMagnitude;
                    if (delta < bestDelta)
                    {
                        bestDelta = delta;
                        bestCellFluidSelectable = cell.fluidSelectable;
                    }
                }
            }

            if (bestCellFluidSelectable != null)
            {
                switch (paletteColor.paintingDirection)
                {
                    case PaletteColor.Direction.Down:
                        paletteFluidSelectable.down = bestCellFluidSelectable;
                        if (bestCellFluidSelectable.up == null)
                        {
                            bestCellFluidSelectable.up = paletteFluidSelectable;
                        }
                        break;
                    case PaletteColor.Direction.Left:
                        paletteFluidSelectable.left = bestCellFluidSelectable;
                        if (bestCellFluidSelectable.right == null)
                        {
                            bestCellFluidSelectable.right = paletteFluidSelectable;
                        }
                        break;

                    default:
                        break;
                }
            }
        }

        return true;
    }

    private ColorBlindSet DetermineActiveColorBlindSet()
    {
        _bombModule.Log("Determining active color-blind set...");

        bool specialRuleMatch = _bombInfo.GetPortCount(KMBombInfoExtensions.KnownPortType.DVI) == 2 && _bombInfo.GetPortCount(KMBombInfoExtensions.KnownPortType.RJ45) == 1 && _bombInfo.GetOnIndicators().Contains("CLR");
        if (specialRuleMatch)
        {
            _bombModule.Log("Special rule takes effect -- express your creativity!");
            return null;
        }

        int ruleATotal = _bombInfo.GetBatteryCount() + _bombInfo.GetIndicators().Count() + _bombInfo.GetPorts().Count() + 2;
        _bombModule.LogFormat("Rule A total = {0} (battery count ({1}) + indicator count ({2}) + port count ({3}) + {4}.", ruleATotal, _bombInfo.GetBatteryCount(), _bombInfo.GetIndicators().Count(), _bombInfo.GetPorts().Count(), 2);
        ColorBlindSet ruleASet = ColorBlindSets.FirstOrDefault((x) => x.Name.Length == ruleATotal);
        if (ruleASet != null)
        {
            _bombModule.LogFormat("Rule A matched against color-blind set {0}.", ruleASet.Name);
            return ruleASet;
        }
        else
        {
            _bombModule.Log("No match for rule A.");
        }

        char[] ruleBIndicatorCharacters = _bombInfo.GetIndicators().SelectMany((x) => x.ToUpperInvariant().ToCharArray()).Distinct().ToArray();
        _bombModule.LogFormat("Rule B unique character set from indicators = {{{0}}}.", string.Join(",", Array.ConvertAll(ruleBIndicatorCharacters, (x) => x.ToString())));

        ColorBlindSet ruleBSet = null;
        int ruleBCharacterCount = -1;
        bool ruleBDuplicate = false;
        foreach (ColorBlindSet set in ColorBlindSets)
        {
            int characterCount = set.Name.ToUpperInvariant().Where((x) => ruleBIndicatorCharacters.Contains(x)).Count();
            _bombModule.LogFormat("{0} scores {1} on rule B.", set.Name, characterCount);

            if (characterCount > ruleBCharacterCount)
            {
                ruleBSet = set;
                ruleBCharacterCount = characterCount;
                ruleBDuplicate = false;
            }
            else if (characterCount == ruleBCharacterCount)
            {
                ruleBDuplicate = true;
            }
        }

        if (ruleBSet != null && !ruleBDuplicate)
        {
            _bombModule.LogFormat("Rule B matched against color-blind set {0}.", ruleBSet.Name);
            return ruleBSet;
        }
        else
        {
            _bombModule.Log("No match for rule B (duplicate scores found).");
        }

        ColorBlindSet ruleCSet = ColorBlindSets.FirstOrDefault((x) => x.Name.Equals("Protanomaly", StringComparison.InvariantCultureIgnoreCase));
        _bombModule.LogFormat("Rule C matched against color-blind set {0}.", ruleCSet.Name);

        return ruleCSet;
    }

    private void SetupPaintingFinalColors(ColorBlindSet activeSet)
    {
        foreach (PaintingCell cell in _painting.Cells)
        {
            if (activeSet != null)
            {
                ColorBlindSet.ColorSwap swap = activeSet.Swaps.FirstOrDefault((x) => x.From == cell.ColorOption);
                if (swap != null)
                {
                    _bombModule.LogFormat("{0} must swap from {1} to {2}.", cell.name, swap.From, swap.To);
                    cell.FinalColorOption = swap.To;
                }
                else
                {
                    _bombModule.LogFormat("{0} must remain as {1}.", cell.name, cell.ColorOption);
                    cell.FinalColorOption = cell.ColorOption;
                }
            }
            else
            {
                cell.FinalColorOption = null;
                _bombModule.LogFormat("{0} must swap from {1} to any other color (special rule).", cell.name, cell.ColorOption);
            }
        }
    }

    private bool OnCellInteract(PaintingCell cell)
    {
        if (!_activeColor.HasValue)
        {
            _bombModule.LogFormat("Tried to paint {0} with no palette color!", cell.name);
            return true;
        }

        if (!cell.FinalColorOption.HasValue)
        {
            if (_activeColor != cell.ColorOption)
            {
                cell.ColorOption = _activeColor.Value;
                cell.FinalColorOption = cell.ColorOption;
                _bombModule.LogFormat("Painting {0} with {1}.", cell.name, _activeColor.Value);
            }
            else
            {
                _bombModule.LogFormat("Tried to paint {0}, but that cell is already complete.", cell.name);
                _bombModule.HandleStrike();
            }

            if (IsAllSolved())
            {
                _bombModule.Log("All cells are now at a final color.");
                _bombModule.HandlePass();
            }

            return true;
        }

        if (cell.ColorOption == cell.FinalColorOption)
        {
            _bombModule.LogFormat("Tried to paint {0}, but that cell is already complete.", cell.name);
            _bombModule.HandleStrike();
            return true;
        }

        if (_activeColor.Value != cell.FinalColorOption)
        {
            _bombModule.LogFormat("Tried to paint {0} with {1}, but that's not the correct final color; expected {2}.", cell.name, _activeColor.Value, cell.FinalColorOption);
            _bombModule.HandleStrike();
            return true;
        }

        _bombModule.LogFormat("Painting {0} with {1}.", cell.name, _activeColor.Value);
        cell.ColorOption = _activeColor.Value;

        if (IsAllSolved())
        {
            _bombModule.Log("All cells are now at their final color.");
            _bombModule.HandlePass();
        }

        return true;
    }

    private bool IsAllSolved()
    {
        return _painting.Cells.All((x) => x.ColorOption == x.FinalColorOption);
    }

    bool TwitchPlaysActive = false;

    public readonly string TwitchHelpMessage = "To paint a cell use !{0} paint <label> <color>.";

    public IEnumerator ProcessTwitchCommand(string command)
    {
        string[] split = command.ToLowerInvariant().Replace("gray", "grey").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (split.Length == 3 && (split[0] == "paint" || split[0] == "p"))
        {
            PaletteColor targetColor = _paletteColors.FirstOrDefault(paletteColor => paletteColor.colorOption.ToString().ToLowerInvariant() == split[2]);
            PaintingCell targetCell = _painting.Cells.FirstOrDefault(paintingCell => paintingCell.twitchPlaysLabel.text == split[1]);

            if (targetColor != null && targetCell.ColorOption != targetColor.colorOption)
            {
                yield return null;

                targetColor.fluidSelectable.selectable.OnInteract();
                yield return new WaitForSeconds(0.1f);

                targetCell.fluidSelectable.selectable.OnInteract();
                yield return new WaitForSeconds(0.1f);

                ShuffleTwitchPlayLabels();
            }
        }
    }

    public IEnumerator TwitchHandleForcedSolve()
    {
        foreach (PaintingCell paintingCell in _painting.Cells)
        {
            ColorOption targetColor;
            if (paintingCell.FinalColorOption.HasValue)
            {
                targetColor = (ColorOption) paintingCell.FinalColorOption;
            }
            else
            {
                targetColor = _painting.colorOptions.Shuffle().FirstOrDefault(color => color != paintingCell.ColorOption);
            }

            yield return ProcessTwitchCommand("paint " + paintingCell.twitchPlaysLabel.text + " " + targetColor.ToString());
        }
    }

    void ShuffleTwitchPlayLabels()
    {
        var labelNumbers = Enumerable.Range(1, _painting.cellCount).Shuffle().ToArray();
        int labelIndex = 0;

        foreach (PaintingCell paintingCell in _painting.Cells)
        {
            TextMesh textMesh = paintingCell.twitchPlaysLabel;

            textMesh.text = labelNumbers[labelIndex++].ToString();
            textMesh.color = paintingCell.ColorOption.GetColor().grayscale > 0.5f ? Color.black : Color.white;
        }
    }
}
