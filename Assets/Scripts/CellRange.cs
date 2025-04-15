using UnityEngine;
using System.Collections.Generic;

public interface CellRange
{
    public IEnumerable<Vector2Int> GetCellsInRange();
    public bool IsCellInRange(Vector2Int cell);
}
