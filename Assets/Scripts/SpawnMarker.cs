using UnityEngine;

public class SpawnMarker : MonoBehaviour
{
    [Header("State")]
    [SerializeField] private Vector2Int boardPosition;

    public void SetBoardPosition(Vector2Int pos)
    {
        boardPosition = pos;
    }

    public Vector2Int GetBoardPosition()
    {
        return boardPosition;
    }
}
