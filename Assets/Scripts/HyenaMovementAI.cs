using UnityEngine;
using System.Collections.Generic;
using System;

public class HyenaMovementAI
{
    static private List<Vector2Int> ALL_DIRS = new List<Vector2Int>();
    static private bool debug = false;

    public HyenaMovementAI()
    {
        for(int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue; // Skip the zero vector
                ALL_DIRS.Add(new Vector2Int(x, y));
            }
        }
    }

    public IEnumerable<Vector2Int> CalculateMovementPath(Vector2Int origin)
    {
        List<Vector2Int> path = new List<Vector2Int>();

        if(debug)
        {
            path.Add(origin + new Vector2Int(1, 0));
            path.Add(origin + new Vector2Int(2, 0));
        }
        else
        {
            path.Add(RandomlyPickCellAroundUnit(origin));
        }
        
        return path;
    }

    private Vector2Int RandomlyPickCellAroundUnit(Vector2Int origin)
    {
        List<Vector2Int> candidateCells = new List<Vector2Int>();
        foreach (var dir in ALL_DIRS)
        {
            Vector2Int candidateCell = origin + dir;

            if (!BoardManager.Instance.IsValidCellForUnitMovement(candidateCell)) continue;

            Unit unitAtPos = BoardManager.Instance.GetUnitAt(candidateCell);
            if (unitAtPos == null || unitAtPos.GetFaction() != Faction.Hyenas)
            {
                candidateCells.Add(candidateCell);
            }
        }

        return candidateCells[UnityEngine.Random.Range(0, candidateCells.Count)]; // Randomly select one of the candidate cells as the target cell
    }
}
