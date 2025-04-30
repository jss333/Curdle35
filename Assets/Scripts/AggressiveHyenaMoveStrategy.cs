using UnityEngine;
using System.Collections.Generic;
using System.Linq;

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

        // define sets to track future free and future occupied cells
        HashSet<Vector2Int> newlyOccupiedCells = new(); // Cells that a hyena will move to
        HashSet<Vector2Int> newlyFreeCells = new(); // Cells that will be vacated by a moving hyena

        BoardManager boardMngr = BoardManager.Instance;

        // sort hyenas by distance to hq
        hyenas.Sort((a, b) => boardMngr.GetCellDataOccupiedByUnit(a).minDistToHQ - boardMngr.GetCellDataOccupiedByUnit(b).minDistToHQ);
        LogUtils.LogEnumerable("sorted hyenas by distance to HQ", hyenas);

        foreach (var hyena in hyenas)
        {
            // find all cells reachable by this hyena within orthogonal distance 2
            Vector2Int hyenaPos = hyena.GetBoardPosition();
            HashSet<ReachableCell> reachableCells = GetOrthogonallyReachableCells(hyenaPos, 2);

            // remove from reachableCells any cells that are already occupied by a hyena or are present in newlyOccupiedCells
            reachableCells.RemoveWhere(cell =>
                (cell.HasFaction(Faction.Hyenas) && !newlyFreeCells.Contains(cell.cell)) 
                || newlyOccupiedCells.Contains(cell.cell));

            LogUtils.LogEnumerable($"{hyena.name} at {hyenaPos} can reach", reachableCells);

            if (!reachableCells.Any())
            {
                Debug.Log($"Hyena {hyena.name} at {hyenaPos} has no reachable cells. It will not move.");
                continue;
            }

            // get a subset of all the reachable cells that are occupied by a unit of the Cats faction
            List<ReachableCell> occupiedByCatFaction = reachableCells
                .Where(cell => cell.HasFaction(Faction.Cats))
                .ToList();
            LogUtils.LogEnumerable($"Of those cells, these are occupied by Cats faction", occupiedByCatFaction);

            if (occupiedByCatFaction.Any())
            {
                // if there are any occupied cells, pick one at random to move the hyena there
                ReachableCell targetCell = occupiedByCatFaction.ElementAt(Random.Range(0, occupiedByCatFaction.Count));
                Debug.Log($"Hyena {hyena.name} will attack at {targetCell}");

                // TODO handle case when unit in targetCell will die and thus the cell will become free
                newlyOccupiedCells.Add(targetCell.cell);
                newlyFreeCells.Add(hyenaPos);

                moveOrders.Add(new HyenaMoveOrder(hyena.GetComponent<MovableUnit>(), hyena.GetBoardPosition(), targetCell.pathToCell));
            }
            else
            {
                // pick a subset of the reachable cells that have the lowest distance to HQ
                List<ReachableCell> closestToHQ = reachableCells
                    .Where(cell => cell.distToHQ == reachableCells.Min(c => c.distToHQ))
                    .ToList();
                LogUtils.LogEnumerable($"Hyena {hyena.name} will choose one of those closest to HQ", closestToHQ);

                // pick one at random to move the hyena there
                ReachableCell targetCell = closestToHQ.ElementAt(Random.Range(0, closestToHQ.Count));
                Debug.Log($"Hyena {hyena.name} will move to {targetCell}");

                newlyOccupiedCells.Add(targetCell.cell);
                newlyFreeCells.Add(hyenaPos);

                moveOrders.Add(new HyenaMoveOrder(hyena.GetComponent<MovableUnit>(), hyena.GetBoardPosition(),targetCell.pathToCell));
            }
        }

        return moveOrders;
    }

    private static HashSet<ReachableCell> GetOrthogonallyReachableCells(Vector2Int origin, int orthogonalDistance)
    {
        BoardManager boardMngr = BoardManager.Instance;

        Dictionary<Vector2Int, ReachableCell> reachable = new Dictionary<Vector2Int, ReachableCell>();
        reachable.Add(origin, new ReachableCell(origin, new List<Vector2Int>(), null, 0));

        HashSet<Vector2Int> frontier = new HashSet<Vector2Int>();
        frontier.Add(origin);
        HashSet<Vector2Int> nextFrontier = new HashSet<Vector2Int>();

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
