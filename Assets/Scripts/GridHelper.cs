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
        Vector3Int gridToArrayOffset = (Vector3Int)BoardManager.Instance.GetGridToArrayOffset();
        return basePos + (Vector3)gridToArrayOffset  + (Vector3)renderingOffset;
    }

    public Vector3Int GridToTilemapWorld(Vector2Int cell)
    {
        return (Vector3Int)cell + (Vector3Int)BoardManager.Instance.GetGridToArrayOffset();
    }

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        Vector3Int cell = grid.WorldToCell(worldPos);
        Vector2Int gridToArrayOffset = BoardManager.Instance.GetGridToArrayOffset();
        return new Vector2Int(cell.x - gridToArrayOffset.x, cell.y - gridToArrayOffset.y);
    }
}
