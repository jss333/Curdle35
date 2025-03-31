using UnityEngine;

public class GridHelper : MonoBehaviour
{
    public static GridHelper Instance { get; private set; }

    [Header("Config")]
    [SerializeField] private Grid grid;
    [Tooltip("Offset for rendering units in the middle of a tile")]
    [SerializeField] private Vector2 renderingOffset = new(0.5f, 0.5f);

    void Awake()
    {
        Instance = this;
    }

    public Vector3 GridToWorld(Vector2Int cell)
    {
        Vector3 basePos = grid.CellToWorld((Vector3Int)cell);
        return basePos + (Vector3)renderingOffset;
    }

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        Vector3Int cell = grid.WorldToCell(worldPos);
        return new Vector2Int(cell.x, cell.y);
    }
}
