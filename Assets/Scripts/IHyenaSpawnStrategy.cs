using UnityEngine;
using System.Collections.Generic;

public interface IHyenaSpawnStrategy
{
    List<Vector2Int> GetSpawnPoints(int numSpawnPoints);
}
