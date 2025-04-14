using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(Unit))]
public class BuildRange : MonoBehaviour
{
    private static List<Vector2Int> dirs = new();

    private Unit unit;

    static BuildRange()
    {
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;

                dirs.Add(new Vector2Int(x, y));
            }
        }
    }

    public void Start()
    {
        unit = GetComponent<Unit>();
    }

    public List<Vector2Int> GetValidCells()
    {
        BoardManager boardMngr = BoardManager.Instance;

        List<Vector2Int> targetCells = new();

        foreach (var dir in dirs)
        {
            Vector2Int candidatePos = unit.GetBoardPosition() + dir;

            // Ignore cells already occupied by a unit
            if (boardMngr.IsValidCellForUnitMovement(candidatePos) && !boardMngr.CellHasUnit(candidatePos))
            {
                targetCells.Add(candidatePos);
            }
        }

        return targetCells;
    }

    public bool IsCellInsideRange(Vector2Int pos)
    {
        return GetValidCells().Contains(pos);
    }
}
