using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SelectableUnit : MonoBehaviour
{
    private SpriteRenderer spriteRenderer = null!;

    [Header("Config")]
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private Sprite unitPortrait;

    [Header("State")]
    [SerializeField] private Color originalColor;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
    }

    public void ShowSelectedEffect()
    {
        spriteRenderer.color = highlightColor;
    }

    public void RemoveSelectedEffect()
    {
        spriteRenderer.color = originalColor;
    }

    public Sprite GetUnitPortrait()
    {
        return unitPortrait;
    }
}
