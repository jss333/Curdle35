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

    public List<Vector2Int> GetValidCells()
    {
        List<Vector2Int> targetCells = new List<Vector2Int>();

        targetCells.AddRange(CellsInDirsUpToLengthThatAreNotVoidAndNotOccupied(OrthogonalDirections(), movementRange));
        targetCells.AddRange(CellsInDirsUpToLengthThatAreNotVoidAndNotOccupied(DiagonalDirections(), movementRange - 1));

        return targetCells;
    }

    private List<Vector2Int> CellsInDirsUpToLengthThatAreNotVoidAndNotOccupied(IEnumerable<Vector2Int> dirs, int maxLength)
    {
        List<Vector2Int> targetCells = new List<Vector2Int>();

        foreach (var dir in dirs)
        {
            for (int length = 1; length <= maxLength; length++)
            {
                Vector2Int candidatePos = unit.GetBoardPosition() + (dir * length);

                // Stop looking in this direction if there is a void cell
                if (!BoardManager.Instance.IsValidCellForUnitMovement(candidatePos)) break;

                // Ignore cells already occupied by a cat
                Unit unitAtPos = BoardManager.Instance.GetUnitAt(candidatePos);
                if (unitAtPos == null || unitAtPos.GetFaction() != Faction.Cats)
                {
                    targetCells.Add(candidatePos);
                }
            }
        }

        return targetCells;
    }

    private IEnumerable<Vector2Int> OrthogonalDirections()
    {
        List<Vector2Int> dirs = new List<Vector2Int>();
        dirs.Add(new Vector2Int( 0,  1));
        dirs.Add(new Vector2Int( 0, -1));
        dirs.Add(new Vector2Int( 1,  0));
        dirs.Add(new Vector2Int(-1,  0));
        return dirs;
    }

    private IEnumerable<Vector2Int> DiagonalDirections()
    {
        List<Vector2Int> dirs = new List<Vector2Int>();
        dirs.Add(new Vector2Int( 1,  1));
        dirs.Add(new Vector2Int( 1, -1));
        dirs.Add(new Vector2Int(-1, -1));
        dirs.Add(new Vector2Int(-1,  1));
        return dirs;
    }

    public bool IsCellInMovementRange(Vector2Int pos)
    {
        return GetValidCells().Contains(pos);
    }

    public Unit GetUnit()
    {
        return unit;
    }
}
