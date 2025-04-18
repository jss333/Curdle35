using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

public struct HyenaDist
{
    public Unit hyena;
    public int distanceToTurret;

    public HyenaDist(Unit hyena, int distanceToTurret)
    {
        this.hyena = hyena;
        this.distanceToTurret = distanceToTurret;
    }
}

[RequireComponent(typeof(Unit))]
public class Turret : MonoBehaviour, CellRange
{
    [Header("Config")]
    [SerializeField] int shootingRange = 2;
    [SerializeField] bool preventShooting = false;

    private Unit unit;

    private void Awake()
    {
        unit = GetComponent<Unit>();
    }

    public Unit GetUnit()
    {
        return unit;
    }

    public bool TryAcquireTarget(List<Unit> allHyenas, out Unit hyena)
    {
        if (preventShooting)
        {
            Debug.LogError($"{name} is not allowed to shoot. Will return false.");
            hyena = null;
            return false;
        }

        var closestHyenas = allHyenas
            .Where(h => h.IsAlive()) // Some hyenas may have already been killed by other towers
            .Select(h => new HyenaDist(h, unit.GetOrthogonalDistance(h)))
            .Where(hd => hd.distanceToTurret <= shootingRange)
            .GroupBy(hd => hd.distanceToTurret)
            .OrderBy(g => g.Key) // `g.Key` is the distance
            .FirstOrDefault()   // Closest group
            ?.Select(hd => hd.hyena) // Select hyenas in that group, if any
            .ToList() ?? new List<Unit>(); // Default to empty list

        if (closestHyenas.Count > 0)
        {
            hyena = closestHyenas[UnityEngine.Random.Range(0, closestHyenas.Count)];
            Debug.Log($"{name} acquired {hyena.name} at {hyena.GetBoardPosition()} as target.");
            return true;
        }
        else
        {
            hyena = null;
            Debug.Log($"No target in range for {name}.");
            return false;
        }
    }

    public List<Vector2Int> GetCellsInRange()
    {
        Vector2Int turretPos = unit.GetBoardPosition();
        List<Vector2Int> cellsInRange = new();

        for (int x = -shootingRange; x <= shootingRange; x++)
        {
            int yRange = shootingRange - Math.Abs(x); // Calculate the y range based on x
            for (int y = -yRange; y <= yRange; y++)
            {
                if(x == 0 && y == 0) continue; // Skip the turret's own position

                Vector2Int candidate = turretPos + new Vector2Int(x, y);
                if(BoardManager.Instance.IsValidCellForUnitMovement(candidate))
                {
                    cellsInRange.Add(candidate);
                }
            }
        }

        return cellsInRange;
    }

    public bool IsCellInRange(Vector2Int cell)
    {
        int dist = unit.GetOrthogonalDistance(cell);
        return dist > 0 && dist <= shootingRange;
    }
}
