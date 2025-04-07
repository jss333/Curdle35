using UnityEngine;
using System.Collections.Generic;

public class MovementAI : MonoBehaviour
{
    static private List<Vector2Int> ALL_DIRS = new List<Vector2Int>();
    private Unit unit;


    void Start()
    {
        unit = GetComponent<Unit>();

        for(int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue; // Skip the zero vector
                ALL_DIRS.Add(new Vector2Int(x, y));
            }
        }
    }

    public Vector2Int CalculateTargetCell()
    {
        Vector2Int origin = unit.GetBoardPosition();
        
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

        return candidateCells[Random.Range(0, candidateCells.Count)]; // Randomly select one of the candidate cells as the target cell
    }
}
