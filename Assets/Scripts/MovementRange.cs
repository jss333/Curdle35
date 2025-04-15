using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(Unit))]
public class MovementRange : MonoBehaviour, CellRange
{
    [Header("Config")]
    [SerializeField] private int movementRange = 2;

    private Unit unit;

    private static readonly List<Vector2Int> orthogonalDirs = new()
    {
        new Vector2Int( 0,  1),
        new Vector2Int( 0, -1),
        new Vector2Int( 1,  0),
        new Vector2Int(-1,  0)
    };

    private static readonly List<Vector2Int> diagonalDirs = new()
    {
        new Vector2Int( 1,  1),
        new Vector2Int( 1, -1),
        new Vector2Int(-1, -1),
        new Vector2Int(-1,  1)
    };

    public void Start()
    {
        unit = GetComponent<Unit>();
    }

    public List<Vector2Int> GetCellsInRange()
    {
        List<Vector2Int> targetCells = new();

        targetCells.AddRange(CellsInDirsUpToLengthThatAreNotVoidAndNotOccupied(orthogonalDirs, movementRange));
        targetCells.AddRange(CellsInDirsUpToLengthThatAreNotVoidAndNotOccupied(diagonalDirs, movementRange - 1));

        return targetCells;
    }

    private List<Vector2Int> CellsInDirsUpToLengthThatAreNotVoidAndNotOccupied(IEnumerable<Vector2Int> dirs, int maxLength)
    {
        List<Vector2Int> targetCells = new();

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

    public bool IsCellInRange(Vector2Int pos)
    {
        return GetCellsInRange().Contains(pos);
    }

    public Vector2Int GetUnitBoardPosition()
    {
        return unit.GetBoardPosition();
    }

    public IEnumerable<Vector2Int> BuildPathToOrthogonalOrDiagonalDestination(Vector2Int destination)
    {
        Vector2Int origin = unit.GetBoardPosition();

        List<Vector2Int> path = new();

        int xVariation = Math.Sign(destination.x - origin.x);
        int yVariation = Math.Sign(destination.y - origin.y);

        Vector2Int nextStep = new(origin.x + xVariation, origin.y + yVariation);
        while (nextStep != destination)
        {
            path.Add(nextStep);
            nextStep = new Vector2Int(nextStep.x + xVariation, nextStep.y + yVariation);
        }
        path.Add(nextStep);

        return path;
    }
}
