using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using static LinqExtensions;

public struct ReachableCell
{
    public Vector2Int cell;
    public List<Vector2Int> pathToCell;
    public Unit originalOccupier;
    public int distToHQ;

    public ReachableCell(Vector2Int cell, List<Vector2Int> pathToCell, Unit originalOccupier, int distToHQ)
    {
        this.cell = cell;
        this.pathToCell = pathToCell;
        this.originalOccupier = originalOccupier;
        this.distToHQ = distToHQ;
    }

    public bool HasFaction(Faction faction)
    {
        return originalOccupier != null && originalOccupier.GetFaction() == faction;
    }

    public bool HasAliveCatUnit(Dictionary<Unit, int> futureCatUnitHP)
    {
        return HasFaction(Faction.Cats) && futureCatUnitHP.TryGetValue(originalOccupier, out int hp) && hp > 0;
    }

    public override string ToString()
    {
        return $"ReachableCell(cell:{cell}, path:[{string.Join(",", pathToCell)}], occupier:{originalOccupier?.name ?? "None"}, distToHQ:{distToHQ})";
    }

    public override bool Equals(object obj)
    {
        if (obj is ReachableCell other)
        {
            return cell == other.cell;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return cell.GetHashCode();
    }
}

public class AggressiveHyenaMoveStrategy : MonoBehaviour, IHyenaMoveStrategy
{
    private static readonly Vector2Int[] orthoDirs = new Vector2Int[] { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };

    public List<HyenaMoveOrder> CalculateMovementPathForHyenas(List<HyenaUnit> hyenas)
    {
        List<HyenaMoveOrder> moveOrders = new();

        HashSet<Vector2Int> newlyOccupiedCells = new(); // Cells that a hyena will move to
        HashSet<Vector2Int> newlyFreeCells = new(); // Cells that will be vacated by a moving hyena
        Dictionary<Unit, int> futureCatUnitsHP = FindAllCatFactionUnitsAndTheirHP(); // Track future HP of units that will be attacked by hyenas

        BoardManager boardMngr = BoardManager.Instance;

        int SortByDistToHQ(HyenaUnit a, HyenaUnit b)
        {
            return boardMngr.GetCellDataOccupiedByUnit(a).minDistToHQ - boardMngr.GetCellDataOccupiedByUnit(b).minDistToHQ;
        }
        hyenas.Sort(SortByDistToHQ);
        LogUtils.LogEnumerable("sorted hyenas by distance to HQ", hyenas);

        foreach (var hyena in hyenas)
        {
            // Find all cells reachable by this hyena within some orthogonal distance
            Vector2Int hyenaPos = hyena.GetBoardPosition();
            HashSet<ReachableCell> reachableCells = GetOrthogonallyReachableCells(hyenaPos, HyenasManager.Instance.HyenaMoveDistance);

            // Remove from reachableCells any cells that are (or will be) already occupied by a hyena
            reachableCells.RemoveWhere(cell =>
                (cell.HasFaction(Faction.Hyenas) && !newlyFreeCells.Contains(cell.cell)) 
                || newlyOccupiedCells.Contains(cell.cell));
            //LogUtils.LogEnumerable($"{hyena.name} at {hyenaPos} can reach", reachableCells);

            if (!reachableCells.Any())
            {
                //Debug.Log($"Hyena {hyena.name} at {hyenaPos} has no reachable cells. No move order will generated for it.");
                continue;
            }

            // Get a subset of all the reachable cells that are occupied by a unit of the Cats faction
            List<ReachableCell> occupiedByCatFaction = reachableCells
                .Where(cell => cell.HasAliveCatUnit(futureCatUnitsHP))
                .ToList();
            //LogUtils.LogEnumerable($"Of those cells, these are occupied by Cats faction", occupiedByCatFaction);

            if (occupiedByCatFaction.Any())
            {
                // Randomly pick between the closest cats/structures to attack
                ReachableCell targetCell = occupiedByCatFaction
                    .WhereMin(cell => cell.originalOccupier.GetOrthogonalDistance(hyenaPos))
                    .TakeRandom();

                int futureHP = futureCatUnitsHP[targetCell.originalOccupier];
                //Debug.Log($"Hyena {hyena.name} will attack at {targetCell} which will have {futureHP} before the attack");

                futureCatUnitsHP[targetCell.originalOccupier] = futureHP - 1;
                //Debug.Log($"After {hyena.name} attacks the hp will be {futureCatUnitsHP[targetCell.originalOccupier]}");

                newlyFreeCells.Add(hyenaPos);
                // Hyenas always die in combat so we don't mark the target cell as newly occupied by the hyena

                moveOrders.Add(new HyenaMoveOrder(hyena, targetCell.pathToCell));
            }
            else
            {
                // Remove reachable cells that are further away from HQ than the current one
                reachableCells.RemoveWhere(cell => cell.distToHQ >= boardMngr.GetCellDataAt(hyenaPos).minDistToHQ);

                if (!reachableCells.Any())
                {
                    //Debug.Log($"Hyena {hyena.name} at {hyenaPos} has no reachable cells after removing those that are further from HQ. It will not move.");
                    continue;
                }

                // Randomly pick a subset of the reachable cells that have the lowest distance to HQ
                ReachableCell targetCell = reachableCells
                    .WhereMin(cell => cell.distToHQ)
                    .TakeRandom();

                //Debug.Log($"Hyena {hyena.name} at {hyenaPos} will move to {targetCell}");

                newlyOccupiedCells.Add(targetCell.cell);
                newlyFreeCells.Add(hyenaPos);

                moveOrders.Add(new HyenaMoveOrder(hyena, targetCell.pathToCell));
            }
        }

        return moveOrders;
    }

    private Dictionary<Unit, int> FindAllCatFactionUnitsAndTheirHP()
    {
        return BoardManager.Instance.GetAllUnits(Faction.Cats).ToDictionary(
            unit => unit,
            unit => unit.GetCurrentHealth()
        );
    }

    private static HashSet<ReachableCell> GetOrthogonallyReachableCells(Vector2Int origin, int orthogonalDistance)
    {
        BoardManager boardMngr = BoardManager.Instance;

        Dictionary<Vector2Int, ReachableCell> reachable = new Dictionary<Vector2Int, ReachableCell>();
        reachable.Add(origin, new ReachableCell(origin, new List<Vector2Int>(), null, 0));

        HashSet<Vector2Int> frontier = new();
        frontier.Add(origin);

        HashSet<Vector2Int> nextFrontier = new();

        for (int i = 0; i < orthogonalDistance; i++)
        {
            nextFrontier.Clear();

            foreach (var cell in frontier)
            {
                foreach (var dir in orthoDirs)
                {
                    Vector2Int nextCell = cell + dir;
                    if (!reachable.ContainsKey(nextCell) && boardMngr.IsValidCellForUnitMovement(nextCell))
                    {
                        List<Vector2Int> pathToNextCell = new List<Vector2Int>(reachable[cell].pathToCell);
                        pathToNextCell.Add(nextCell);
                        Unit unitInCell = boardMngr.GetUnitAt(nextCell);
                        int distToHQ = boardMngr.GetCellDataAt(nextCell).minDistToHQ;
                        reachable.Add(nextCell, new ReachableCell(nextCell, pathToNextCell, unitInCell, distToHQ));
                        nextFrontier.Add(nextCell);
                    }
                }
            }

            frontier = new HashSet<Vector2Int>(nextFrontier);
        }

        reachable.Remove(origin);

        return new HashSet<ReachableCell>(reachable.Values);
    }
}
