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
        PlayerMovableUnit? movableUnit = GetPlayerMovableUnit();
        if (movableUnit != null)
        {
            BoardManager.Instance.ShowMovementRangeForUnit(movableUnit);
        }
    }

    public void DoUnitDeselection()
    {
        spriteRenderer.color = originalColor;

        // TODO use notification?
        BoardManager.Instance.ClearMovementRange();
    }

    public PlayerMovableUnit? GetPlayerMovableUnit()
    {
        return GetComponent<PlayerMovableUnit>();
    }
}
