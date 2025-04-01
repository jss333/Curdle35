#nullable enable

using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SelectableUnit : MonoBehaviour
{
    private SpriteRenderer spriteRenderer = null!;

    [Header("Config")]
    [SerializeField] private Color highlightColor = Color.yellow;

    [Header("State")]
    [SerializeField] private Color originalColor;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
    }

    public void DoUnitSelection()
    {
        spriteRenderer.color = highlightColor;

        // TODO use notification?
        MovementRange? unitWithMoveRange = GetMovementRange();
        if (unitWithMoveRange != null)
        {
            BoardManager.Instance.ShowMovementRangeForUnit(unitWithMoveRange);
        }
    }

    public void DoUnitDeselection()
    {
        spriteRenderer.color = originalColor;

        // TODO use notification?
        BoardManager.Instance.ClearMovementRange();
    }

    public MovementRange? GetMovementRange()
    {
        return GetComponent<MovementRange>();
    }
}
