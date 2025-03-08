using UnityEngine;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class HyenasSpawnPointAlgorithm
{
    private IGameStateProvider gameStateProvider;
    private List<Tuple<int, int>> outerCells;
    private List<Tuple<int, int>> innerCellsAwayFromHQ;

    public HyenasSpawnPointAlgorithm(IGameStateProvider gameStateProvider, int minDistFromHQ)
    {
        this.gameStateProvider = gameStateProvider;

        BoardOuterInnerCellsCalculator cellCalculator = new BoardOuterInnerCellsCalculator(gameStateProvider.GetBoard(), gameStateProvider.GetHQCell(), minDistFromHQ);
        this.outerCells = cellCalculator.GetOuterCells();
        this.innerCellsAwayFromHQ = cellCalculator.GetInnerCellsAwayFromHQ();
        
        //LogUtils.LogEnumerable("outer Cells", outerCells);
        //LogUtils.LogEnumerable("inner Cells Away From HQ", innerCellsAwayFromHQ);
    }

    public HashSet<Tuple<int, int>> GenerateRandomOuterSpawnPoints(int numOuterSpawnPoints, int minDistBetweenPoints)
    {
        HashSet<Tuple<int, int>> selectedOuterSpawnPoints = new HashSet<Tuple<int, int>>();

        // create a parallel list to mark edge tiles that can be potentially picked
        // remove from consideration all the tiles that are already claimed by hyenas or that have another unit present
        List<bool> potentialSpawnPoints = new List<bool>();
        int numPotentialSpawnPoints = outerCells.Count;

        foreach (Tuple<int,int> outerCell in outerCells)
        {
            if (gameStateProvider.IsClaimedByHyenas(outerCell) || gameStateProvider.HasHyena(outerCell))
            {
                potentialSpawnPoints.Add(false);
                numPotentialSpawnPoints--;
            }
            else
            {
                potentialSpawnPoints.Add(true);
            }
        }
        //LogUtils.LogEnumerable("potentialSpawnPoints after removing already claimed", potentialSpawnPoints);

        // randomly pick numOuterSpawnPoints cells from the list
        for (int i = 0; i < numOuterSpawnPoints; i++)
        {
            if (numPotentialSpawnPoints <= 0)
            {
                break;
            }

            // advance randomIndex to the first available spawn point
            int randomIndex = potentialSpawnPoints.FindIndex(b => b);
            // randomly pick an ordinal from list of potential
            int randomOrdinal = Random.Range(0, numPotentialSpawnPoints);
            // count from start until we find the randomOrdinal-nth element
            for (int j = 0; j < randomOrdinal; j++)
            {
                randomIndex++;
                while (potentialSpawnPoints[randomIndex] != true)
                {
                    randomIndex++;
                }
            }

            selectedOuterSpawnPoints.Add(outerCells[randomIndex]);
            potentialSpawnPoints[randomIndex] = false;
            numPotentialSpawnPoints--;
            //LogUtils.LogEnumerable(i + ": selectedSpawnPoints", selectedSpawnPoints);
            //LogUtils.LogEnumerable(i + ": potentialSpawnPoints", potentialSpawnPoints);

            // find all the other points within minDistBetweenPoints and also remove them from list of potentials
            for (int j = -minDistBetweenPoints; j <= minDistBetweenPoints; j++)
            {
                int neighborIndex = (potentialSpawnPoints.Count + randomIndex + j) % potentialSpawnPoints.Count;
                potentialSpawnPoints[neighborIndex] = false;
                numPotentialSpawnPoints--;
            }
            //LogUtils.LogEnumerable(i + ": potentialSpawnPoints after removing neighbors", potentialSpawnPoints);
        }

        return selectedOuterSpawnPoints;
    }

    public HashSet<Tuple<int, int>> GenerateRandomInnerSpawnPoints(int numInnerSpawnPoints)
    {
        HashSet<Tuple<int, int>> selectedInnerSpawnPoints = new HashSet<Tuple<int, int>>();

        // build a list of all possible inner points (and another with only those that are claimed by cats)
        List<Tuple<int, int>> potentialSpawnPoints = new List<Tuple<int, int>>();
        List<Tuple<int, int>> potentialSpawnPointsClaimedByCats = new List<Tuple<int, int>>();

        foreach(Tuple<int, int> innerCell in innerCellsAwayFromHQ)
        {
            if(gameStateProvider.HasHyena(innerCell))
            {
                continue;
            }

            potentialSpawnPoints.Add(innerCell);
            if(gameStateProvider.IsClaimedByCats(innerCell))
            {
                potentialSpawnPointsClaimedByCats.Add(innerCell);
            }
        }

        // prioritize spawning in cells already claimed by cats if there are enough of them
        if(potentialSpawnPointsClaimedByCats.Count >= numInnerSpawnPoints)
        {
            potentialSpawnPoints = potentialSpawnPointsClaimedByCats;
        }

        // choose randomly from potential inner spawn points
        for (int i = 0; i < numInnerSpawnPoints; i++)
        {
            int randomIndex = Random.Range(0, potentialSpawnPoints.Count);
            selectedInnerSpawnPoints.Add(potentialSpawnPoints[randomIndex]);
            potentialSpawnPoints.RemoveAt(randomIndex);
        }

        return selectedInnerSpawnPoints;
    }
}
