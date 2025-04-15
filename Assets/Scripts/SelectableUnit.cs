using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SelectableUnit : MonoBehaviour
{
    private SpriteRenderer spriteRenderer = null!;
    private Turret turret = null;

    [Header("Config")]
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private Sprite unitPortrait;

    [Header("State")]
    [SerializeField] private Color originalColor;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;

        TryGetComponent<Turret>(out turret);
    }

    public void ShowSelectedEffect()
    {
        spriteRenderer.color = highlightColor;

        if (turret != null)
        {
            BoardManager.Instance.ShowShootingRange(turret);
        }
    }

    public void RemoveSelectedEffect()
    {
        spriteRenderer.color = originalColor;

        if (turret != null)
        {
            BoardManager.Instance.ClearAllRanges();
        }
    }

    public Sprite GetUnitPortrait()
    {
        return unitPortrait;
    }
}
