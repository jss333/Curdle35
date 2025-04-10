using UnityEngine;
using System.Collections.Generic;
using System;

public class HyenaMovementAI
{
    private static readonly List<Vector2Int> ALL_DIRS = new();
    private static readonly bool debug = false;

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

    public List<HyenaMoveOrder> CalculateMovementPathForHyenas(List<Unit> hyenas)
    {
        List<HyenaMoveOrder> moveOrders = new();

        HashSet<Vector2Int> newlyOccupiedCells = new(); // Cells that a hyena will move to
        HashSet<Vector2Int> newlyFreeCells = new(); // Cells that will be vacated by a moving hyena

        foreach (var unit in hyenas)
        {
            Debug.Log($"Calculating movement path for {unit.name} at {unit.GetBoardPosition()}");
            IEnumerable<Vector2Int> path = CalculateMovementPath(unit.GetBoardPosition(), newlyOccupiedCells, newlyFreeCells);
            moveOrders.Add(new HyenaMoveOrder(unit.GetComponent<MovableUnit>(), path));
        }

        return moveOrders;
    }

    private IEnumerable<Vector2Int> CalculateMovementPath(Vector2Int origin, HashSet<Vector2Int> newlyOccupiedCells, HashSet<Vector2Int> newlyFreeCells)
    {
        List<Vector2Int> path = new();

        if(debug)
        {
            path.Add(origin + new Vector2Int(1, 0));
            path.Add(origin + new Vector2Int(2, 0));
        }
        else
        {
            Vector2Int? pickedCell = RandomlyPickCellAroundUnit(origin, newlyOccupiedCells, newlyFreeCells);
            if(pickedCell is Vector2Int)
            {
                path.Add(pickedCell.Value);
            }
        }
        
        return path;
    }

    private Vector2Int? RandomlyPickCellAroundUnit(Vector2Int origin, HashSet<Vector2Int> newlyOccupiedCells, HashSet<Vector2Int> newlyFreeCells)
    {
        Debug.Log($"Randomly picking cell around {origin}");
        LogUtils.LogEnumerable("--- newlyOccupiedCells", newlyOccupiedCells);
        LogUtils.LogEnumerable("--- newlyFreeCells", newlyFreeCells);

        List<Vector2Int> candidateCells = new();

        foreach (var dir in ALL_DIRS)
        {
            Vector2Int candidateCell = origin + dir;

            if (!BoardManager.Instance.IsValidCellForUnitMovement(candidateCell)) continue;

            if (newlyOccupiedCells.Contains(candidateCell)) continue;

            Unit unitAtPos = BoardManager.Instance.GetUnitAt(candidateCell);
            if (unitAtPos == null || unitAtPos.GetFaction() != Faction.Hyenas || newlyFreeCells.Contains(candidateCell))
            {
                candidateCells.Add(candidateCell);
            }
        }

        LogUtils.LogEnumerable($"Candidate cells from origin {origin}: ", candidateCells);

        if(candidateCells.Count == 0)
        {
            return null;
        }

        Vector2Int pickedCell = candidateCells[UnityEngine.Random.Range(0, candidateCells.Count)]; // Randomly select one of the candidate cells as the target cell

        newlyOccupiedCells.Add(pickedCell);
        if (newlyFreeCells.Contains(pickedCell)) newlyFreeCells.Remove(pickedCell);
        newlyFreeCells.Add(origin);

        Debug.Log($"Picked the following cell: {pickedCell}");
        LogUtils.LogEnumerable("--- newlyOccupiedCells", newlyOccupiedCells);
        LogUtils.LogEnumerable("--- newlyFreeCells", newlyFreeCells);

        return pickedCell;
    }
}
