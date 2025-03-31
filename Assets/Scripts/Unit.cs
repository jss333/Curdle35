using UnityEngine;

public class Unit : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    [Header("Config")]
    [SerializeField] private Color highlightColor = Color.yellow;

    [Header("State")]
    [SerializeField] private Color originalColor;
    [SerializeField] private Vector2Int boardPosition;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;

        // Determine logical board position given current world position
        boardPosition = GridHelper.Instance.WorldToGrid(transform.position);

        // Set unit's workd position to be the correct one for the given board position
        this.transform.position = GridHelper.Instance.GridToWorld(boardPosition);
    }

    public void ShowSelected()
    {
        spriteRenderer.color = highlightColor;
    }

    public void ShowDeselected()
    {
        spriteRenderer.color = originalColor;
    }
}
