using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class InnerOuterHyenaSpawnStrategy : MonoBehaviour, IHyenaSpawnStrategy
{
    [Header("Config")]
    [SerializeField] private HQUnit hqUnit;
    [SerializeField] private int minDistFromHQ = 2;
    [SerializeField] private int minDistBetweenOuterCells = 3;
    [SerializeField] private bool prioritizeInnerCellsOwnedByCats = true;

    [Header("State")]
    [SerializeField] bool cellsCalculated = false;
    [SerializeField] List<Vector2Int> innerCells;
    [SerializeField] List<Vector2Int> outerCells;

    public List<Vector2Int> GetSpawnCells(int numSpawnCells)
    {
        InitializeCalculatorAndCalculateInnerAndOuterCells();

        int numInnerSpawnCells = numSpawnCells / 2; // rounded down
        int numOuterSpawnCells = numSpawnCells - numInnerSpawnCells;

        Debug.Log($"Picking {numSpawnCells} spawn cells: {numInnerSpawnCells} from inner cells and {numOuterSpawnCells} from outer cells)");
        HashSet<Vector2Int> selectedOuterSpawnPoints = PickRandomOuterSpawnCells(numOuterSpawnCells);
        HashSet<Vector2Int> selectedInnerSpawnPoints = PickRandomInnerSpawnCells(numInnerSpawnCells);
        LogUtils.LogEnumerable("Picked inner cells", selectedInnerSpawnPoints);
        LogUtils.LogEnumerable("Picked outer cells", selectedOuterSpawnPoints);

        return selectedOuterSpawnPoints.Union(selectedInnerSpawnPoints).ToList();
    }

    private void InitializeCalculatorAndCalculateInnerAndOuterCells()
    {
        if (cellsCalculated) return;

        BoardOuterInnerCellsCalculator innerOuterCellCalc = new BoardOuterInnerCellsCalculator(ResolveHQUnitCell(), minDistFromHQ);
        innerCells = innerOuterCellCalc.GetInnerCellsAwayFromHQ();
        outerCells = innerOuterCellCalc.GetOuterCellsAwayFromHQ();

        cellsCalculated = true;
    }

    private Vector2Int ResolveHQUnitCell()
    {
        HQUnit hqUnit = FindAnyObjectByType<HQUnit>();

        if (hqUnit == null)
        {
            Vector2Int centerCell = BoardManager.Instance.GetCenterCell();
            Debug.LogWarning($"[InnerOuterHyenaSpawnStrategy] Initializing strategy. No HQUnit found in scene, so will use board center cell at {centerCell}.");
            return centerCell;
        }
        else
        {
            Vector2Int hqCell = hqUnit.GetBoardPosition();
            Debug.Log($"[InnerOuterHyenaSpawnStrategy] Initializing strategy. HQUnit found on cell {hqCell}.");
            return hqCell;
        }
    }

    private HashSet<Vector2Int> PickRandomOuterSpawnCells(int numCellsToPick)
    {
        HashSet<Vector2Int> pickedCells = new();

        // First try picking cells that are not already claimed and not near each other
        PickAndAccumulateOuterCells(numCellsToPick, false, false, pickedCells);

        // If we can't pick enough cells then allow picking cells that are already claimed by hyenas
        int numCellsMissing = numCellsToPick - pickedCells.Count;
        if (numCellsMissing > 0)
        {
            PickAndAccumulateOuterCells(numCellsMissing, true, false, pickedCells);
        }

        // If we still are not able to pick enough cells then allow neighboring cells to be picked
        numCellsMissing = numCellsToPick - pickedCells.Count;
        if (numCellsMissing > 0)
        {
            PickAndAccumulateOuterCells(numCellsMissing, true, true, pickedCells);
        }

        return pickedCells;
    }

    // Helper class to manage outer cell candidates
    public class CandidateOuterCellEntry
    {
        public Vector2Int cell;
        public bool candidate;
        public bool picked;

        public CandidateOuterCellEntry(Vector2Int cell, bool candidate, bool picked)
        {
            this.cell = cell;
            this.picked = picked;
            this.candidate = !picked && candidate;
        }

        public override string ToString()
        {
            return $"{cell}={candidate}/{picked}";
        }
    }

    private void PickAndAccumulateOuterCells(int numCellsToPick, bool allowAlreadyClaimed, bool allowNeighboringCells, HashSet<Vector2Int> cellsAlreadyPicked)
    {
        //LogUtils.LogEnumerable($"START numCellsToPick:{numCellsToPick} allowAlreadyClaimed:{allowAlreadyClaimed} allowNeighboringCells:{allowNeighboringCells} ==== cellsAlreadyPicked", cellsAlreadyPicked);

        // Initializes a list with all outer cells, marking them accoridng to whether they are valid candidates or have already been picked
        List<CandidateOuterCellEntry> allEntries = outerCells.Select(CreatedCandidateCellEntry).ToList();
        //LogUtils.LogEnumerable("candidates", allEntries.Where(c => c.candidate));

        // Local helper functions
        CandidateOuterCellEntry CreatedCandidateCellEntry(Vector2Int cell)
        {
            bool isAlreadyPicked = cellsAlreadyPicked.Contains(cell);
            bool hasNoHyenaOrStructure = CellHasNoHyenaOrStructure(cell);
            bool claimedByHyenas = CellOwnedByHyenas(cell);

            bool isCandidate = !isAlreadyPicked && hasNoHyenaOrStructure && (allowAlreadyClaimed ? true : !claimedByHyenas);
            return new CandidateOuterCellEntry(cell, isCandidate, isAlreadyPicked);
        }

        void RemoveNeighborsFromCandidatePool(int index, int minDistBetweenOuterPoints)
        {
            for (int j = -minDistBetweenOuterPoints; j <= minDistBetweenOuterPoints; j++)
            {
                int neighborIndex = (allEntries.Count + index + j) % allEntries.Count;
                allEntries[neighborIndex].candidate = false;
            }
        }

        // remove neighbors of already picked cells from the list of candidates
        if (!allowNeighboringCells)
        { 
            for (int i = 0; i < allEntries.Count(); i++)
            {
                if (allEntries[i].picked)
                {
                    RemoveNeighborsFromCandidatePool(i, minDistBetweenOuterCells);
                }
            }
        }

        // randomly pick numCellsToPick cells from the list, removing them and their neighbors from the candidate pool as we go
        for (int i = 0; i < numCellsToPick; i++)
        {
            int numCandidates = allEntries.Count(c => c.candidate == true);

            if (numCandidates <= 0)
            {
                break;
            }

            // Randomly pick among the candidate=true cells
            int randomOrdinal = Random.Range(0, numCandidates);
            int pickedIndex = allEntries
                .Select((entry, index) => (index, entry))
                .Where(indexEntryTuple => indexEntryTuple.entry.candidate == true)
                .Skip(randomOrdinal)
                .First().index;

            allEntries[pickedIndex].picked = true;
            allEntries[pickedIndex].candidate = false;
            //Debug.Log($"picked: {allEntries[pickedIndex]}");

            // Find all the other cells with indices within minDistBetweenOuterPoints and also remove them from list of candidates
            if (!allowNeighboringCells)
            {
                RemoveNeighborsFromCandidatePool(pickedIndex, minDistBetweenOuterCells);
            }
            //LogUtils.LogEnumerable("all entries", allEntries);
        }

        // accumulate the picked cells into the output set
        cellsAlreadyPicked.UnionWith(
            allEntries
            .Where(entry => entry.picked)
            .Select(entry => entry.cell));
    }

    public HashSet<Vector2Int> PickRandomInnerSpawnCells(int numCellsToPick)
    {
        HashSet<Vector2Int> pickedCells = new HashSet<Vector2Int>();

        List<Vector2Int> candidateCells = innerCells.Where(CellHasNoHyenaOrStructure).ToList();
        
        if (prioritizeInnerCellsOwnedByCats)
        {
            List<Vector2Int> candidateCellsClaimedByCats = candidateCells.Where(CellOwnedByCats).ToList();
            pickedCells.UnionWith(RemoveRandomElements(numCellsToPick, candidateCellsClaimedByCats));
        }
        
        pickedCells.UnionWith(RemoveRandomElements(numCellsToPick - pickedCells.Count, candidateCells));

        return pickedCells;
    }

    private static bool CellHasNoHyenaOrStructure(Vector2Int cell)
    {
        return !(BoardManager.Instance.TryGetUnitAt(cell, out Unit unit) && ((unit.GetFaction() == Faction.Hyenas) || (unit.IsStructure())));
    }

    private static bool CellOwnedByHyenas(Vector2Int cell)
    {
        return BoardManager.Instance.IsOwnedBy(cell, Faction.Hyenas);
    }

    private static bool CellOwnedByCats(Vector2Int cell)
    {
        return BoardManager.Instance.IsOwnedBy(cell, Faction.Cats);
    }

    private static HashSet<Vector2Int> RemoveRandomElements(int numElements, List<Vector2Int> sourceList)
    {
        HashSet<Vector2Int> picked = new HashSet<Vector2Int>();

        // Randomly remove elements from the source list
        for (int i = 0; i < numElements && sourceList.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, sourceList.Count);
            var element = sourceList.ElementAt(randomIndex);
            picked.Add(element);
            sourceList.RemoveAt(randomIndex);
        }
        return picked;
    }

    [ContextMenu("DEBUG - Calculate and paint inner and outer cells")]
    private void PaintInnerAndOuterCells()
    {
        InitializeCalculatorAndCalculateInnerAndOuterCells();

        foreach (var cell in outerCells)
        {
            BoardManager.Instance.ClaimCellForFaction(cell, Faction.Hyenas);
        }

        foreach (var cell in innerCells)
        {
            BoardManager.Instance.ClaimCellForFaction(cell, Faction.Cats);
            if (outerCells.Contains(cell))
            {
                Debug.LogWarning($"Inner cell {cell} is also in outer cells. This should not happen! Please check the logic.");
            }
        }
    }
}
