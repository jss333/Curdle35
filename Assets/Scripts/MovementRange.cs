using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Unit))]
public class MovementRange : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private int movementRange = 2;

    private Unit unit;

    public void Start()
    {
        unit = GetComponent<Unit>();
    }

    public List<Vector2Int> GetValidMovementCells()
    {
        List<Vector2Int> targetCells = new List<Vector2Int>();
        Vector2Int currentPos = unit.GetBoardPosition();

        foreach (var dir in OrthogonalDirections())
        {
            for(int length = 1; length <= movementRange; length++)
            {
                Vector2Int candidatePos = currentPos + (dir * length);
                if (!BoardManager.Instance.IsValidCellForUnitMovement(candidatePos)) break;
                targetCells.Add(candidatePos);
            }
        }

        foreach (var dir in DiagonalDirections())
        {
            for (int length = 1; length <= movementRange-1; length++)
            {
                Vector2Int candidatePos = currentPos + (dir * length);
                if (!BoardManager.Instance.IsValidCellForUnitMovement(candidatePos)) break;
                targetCells.Add(candidatePos);
            }
        }

        return targetCells;
    }

    private IEnumerable<Vector2Int> OrthogonalDirections()
    {
        List<Vector2Int> dirs = new List<Vector2Int>();
        dirs.Add(new Vector2Int( 0,  1));
        dirs.Add(new Vector2Int( 0, -1));
        dirs.Add(new Vector2Int( 1,  0));
        dirs.Add(new Vector2Int(-1,  0));
        return dirs;
    }

    private IEnumerable<Vector2Int> DiagonalDirections()
    {
        List<Vector2Int> dirs = new List<Vector2Int>();
        dirs.Add(new Vector2Int( 1,  1));
        dirs.Add(new Vector2Int( 1, -1));
        dirs.Add(new Vector2Int(-1, -1));
        dirs.Add(new Vector2Int(-1,  1));
        return dirs;
    }

    public bool IsCellInMovementRange(Vector2Int pos)
    {
        return GetValidMovementCells().Contains(pos);
    }

    public Unit GetUnit()
    {
        return unit;
    }
}
