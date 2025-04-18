using UnityEngine;
using System.Collections.Generic;
using System;

public class HardCodedHyenaSpawnStrategy : MonoBehaviour, IHyenaSpawnStrategy
{
    [Header("Config - Next ordered spawn points to generate")]
    [SerializeField] bool removeFromArrayAfterUse = true;
    [SerializeField] private Vector2Int[] next = {
        new Vector2Int(4, 1),
        new Vector2Int(4, 2),
        new Vector2Int(4, 3),
        new Vector2Int(4, 4),
        new Vector2Int(4, 5)
    };

    public List<Vector2Int> GetSpawnPoints(int numSpawnPoints)
    {
        Debug.Log("HardCodedHyenaSpawnStrategy: Generating spawn points.");

        var spawnPoints = new List<Vector2Int>();

        // fill the list with items from the next array, in order
        for (int i = 0; i < numSpawnPoints && i < next.Length; i++)
        {
            spawnPoints.Add(next[i]);
        }

        // remove the used items from the next array if removeFromArrayAfterUse is true
        if (removeFromArrayAfterUse)
        {
            if (numSpawnPoints <= next.Length)
            {
                next = next[numSpawnPoints..];
            }
            else
            {
                next = Array.Empty<Vector2Int>();
            }
        }

        return spawnPoints;
    }
}
