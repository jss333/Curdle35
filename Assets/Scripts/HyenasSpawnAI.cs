using UnityEngine;
using System.Collections.Generic;

public class HyenasSpawnAI
{
    public List<Vector2Int> GetSpawnPoints(int spawnRate)
    {
        List<Vector2Int> spawnPoints = new List<Vector2Int>();

        // Generate random spawn points based on the spawn rate
        while (spawnPoints.Count < spawnRate)
        {
            var candidate = new Vector2Int(Random.Range(0, BoardManager.Instance.GetWidth()), Random.Range(0, BoardManager.Instance.GetHeight()));

            if (!BoardManager.Instance.IsValidCellForUnitMovement(candidate))
            {
                continue;
            }

            Unit unit = BoardManager.Instance.GetUnitAt(candidate);
            if (unit != null && unit.GetFaction() == Faction.Hyenas)
            {
                continue;
            }

            spawnPoints.Add(candidate);
        }

        return spawnPoints;
    }
}
