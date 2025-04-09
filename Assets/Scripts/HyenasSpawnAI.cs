using UnityEngine;
using System.Collections.Generic;

public class HyenasSpawnAI
{
    public List<Vector2Int> GetSpawnPoints(int spawnRate)
    {
        List<Vector2Int> spawnPoints = new List<Vector2Int>();

        // Generate random spawn points based on the spawn rate
        int curIteration = 0;
        int maxIterations = 100;
        while (spawnPoints.Count < spawnRate)
        {
            if (curIteration++ > maxIterations) break; // Prevent infinite loop

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

        if (spawnPoints.Count < spawnRate)
        {
            Debug.LogWarning($"HyenasSpawnAI: Not able to generate {spawnRate} spawn point after {curIteration} iterations. Generated {spawnPoints.Count}.");
        }

        return spawnPoints;
    }
}
