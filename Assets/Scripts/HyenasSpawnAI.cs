using UnityEngine;
using System.Collections.Generic;

public class HyenasSpawnAI
{
    private readonly bool debug = false;

    public List<Vector2Int> GetSpawnPoints(int spawnRate)
    {
        List<Vector2Int> spawnPoints = new();

        if(debug)
        {
            spawnPoints.Add(new Vector2Int(0, 0));
            spawnPoints.Add(new Vector2Int(1, 0));
            spawnPoints.Add(new Vector2Int(2, 0));
            spawnPoints.Add(new Vector2Int(3, 0));
            spawnPoints.Add(new Vector2Int(4, 0));
            spawnPoints.Add(new Vector2Int(5, 0));
            spawnPoints.Add(new Vector2Int(6, 0));
            spawnPoints.Add(new Vector2Int(7, 0));
            return spawnPoints;
        }

        // Generate random spawn points based on the spawn rate
        int curIteration = 0;
        int maxIterations = 100;
        while (spawnPoints.Count < spawnRate)
        {
            if (curIteration++ > maxIterations) break; // Prevent infinite loop

            var boardMngr = BoardManager.Instance;
            var candidate = new Vector2Int(
                Random.Range(0, boardMngr.GetBoardWidth()),
                Random.Range(0, boardMngr.GetBoardHeight())
            );

            if (!boardMngr.IsValidCellForUnitMovement(candidate))
            {
                continue;
            }

            if (spawnPoints.Contains(candidate))
            {
                continue;
            }

            if(boardMngr.TryGetUnitAt(candidate, out Unit unit))
            {
                if(unit.GetFaction() == Faction.Hyenas || unit.IsStructure())
                {
                    continue;
                }
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
