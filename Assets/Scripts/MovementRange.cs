using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Unit))]
public class MovementRange : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private int movementRange = 2;

    private Unit unit;

    public void Start()
    {
        unit = GetComponent<Unit>();
    }

    public List<Vector2Int> GetValidMovementCells()
    {
        List<Vector2Int> directions = new List<Vector2Int>();
        AddAllOrthogonalDirectionsOfLengthUpTo(directions, movementRange);
        AddAllDiagonalDirectionsOfLengthUpTo(directions, movementRange - 1);

        List<Vector2Int> targetCells = new List<Vector2Int>();

        Vector2Int currentPos = unit.GetBoardPosition();
        foreach (var dir in directions)
        {
            Vector2Int candidatePos = currentPos + dir;
            if (BoardManager.Instance.IsValidCellForUnitMovement(candidatePos))
            {
                targetCells.Add(candidatePos);
            }
        }

        return targetCells;
    }

    private void AddAllOrthogonalDirectionsOfLengthUpTo(List<Vector2Int> directions, int maxLength)
    {
        for (int i = 1; i <= maxLength; i++)
        {
            directions.Add(new Vector2Int( 0,  i));
            directions.Add(new Vector2Int( 0, -i));
            directions.Add(new Vector2Int( i,  0));
            directions.Add(new Vector2Int(-i,  0));
        }
    }

    private void AddAllDiagonalDirectionsOfLengthUpTo(List<Vector2Int> directions, int maxLength)
    {
        for (int i = 1; i <= maxLength; i++)
        {
            directions.Add(new Vector2Int( i,  i));
            directions.Add(new Vector2Int(-i, -i));
            directions.Add(new Vector2Int( i, -i));
            directions.Add(new Vector2Int(-i,  i));
        }
    }

    public bool IsCellInMoveRange(Vector2Int pos)
    {
        return GetValidMovementCells().Contains(pos);
    }

    public Unit GetUnit()
    {
        return unit;
    }
}
