using UnityEngine;
using System.Collections.Generic;

public interface CellRange
{
    public List<Vector2Int> GetCellsInRange();

    public bool IsCellInRange(Vector2Int cell);
}
