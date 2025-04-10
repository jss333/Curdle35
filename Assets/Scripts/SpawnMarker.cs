using UnityEngine;

public class SpawnMarker : MonoBehaviour
{
    [Header("State")]
    [SerializeField] private Vector2Int boardCell;

    public void SetBoardCell(Vector2Int cell)
    {
        boardCell = cell;
    }

    public Vector2Int GetBoardCell()
    {
        return boardCell;
    }
}
