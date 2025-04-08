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
            var boardMngr = BoardManager.Instance;
            var candidate = new Vector2Int(
                Random.Range(0, boardMngr.GetWidth()),
                Random.Range(0, boardMngr.GetHeight())
            );

            if (!boardMngr.IsValidCellForUnitMovement(candidate))
            {
                continue;
            }

            if (spawnPoints.Contains(candidate))
            {
                continue;
            }

            Unit unit = boardMngr.GetUnitAt(candidate);
            if (unit != null && (unit.GetFaction() == Faction.Hyenas || unit.IsStructure()))
            {
                continue;
            }

            spawnPoints.Add(candidate);
        }

        return spawnPoints;
    }
}
