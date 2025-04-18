using UnityEngine;
using System.Collections.Generic;

public class RandomHyenaSpawnStrategy : MonoBehaviour, IHyenaSpawnStrategy
{
    public List<Vector2Int> GetSpawnPoints(int numSpawnPoints)
    {
        Debug.Log("RandomHyenaSpawnStrategy: Generating spawn points.");

        var spawnPoints = new List<Vector2Int>();
        var boardMngr = BoardManager.Instance;
        int curIteration = 0;
        int maxIterations = 100;

        while (spawnPoints.Count < numSpawnPoints && curIteration++ < maxIterations)
        {
            var candidate = new Vector2Int(
                Random.Range(0, boardMngr.GetBoardWidth()),
                Random.Range(0, boardMngr.GetBoardHeight())
            );

            if (!boardMngr.IsValidCellForUnitMovement(candidate) || spawnPoints.Contains(candidate))
            {
                continue;
            }

            if (boardMngr.TryGetUnitAt(candidate, out Unit unit))
            {
                if (unit.GetFaction() == Faction.Hyenas || unit.IsStructure())
                {
                    continue;
                }
            }

            spawnPoints.Add(candidate);
        }

        if (spawnPoints.Count < numSpawnPoints)
        {
            Debug.LogWarning($"RandomHyenaSpawnStrategy: Not able to generate {numSpawnPoints} spawn point after {curIteration} iterations. Generated {spawnPoints.Count}.");
        }

        return spawnPoints;
    }
}
