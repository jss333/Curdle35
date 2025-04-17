using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

[RequireComponent(typeof(Unit))]
public class Builder : MonoBehaviour, CellRange
{
    private static List<Vector2Int> dirs = new();

    static Builder()
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

    private Unit unit;

    public void Start()
    {
        unit = GetComponent<Unit>();
    }

    public List<Vector2Int> GetCellsInRange()
    {
        BoardManager boardMngr = BoardManager.Instance;

        return dirs
            .Select(dir => dir + unit.GetBoardPosition())
            .Where(candidate => boardMngr.IsValidCellForUnitMovement(candidate))
            .Where(candidate => !boardMngr.CellHasUnit(candidate))
            .ToList();
    }

    public bool IsCellInRange(Vector2Int pos)
    {
        return GetCellsInRange().Contains(pos);
    }
}
