using UnityEngine;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;


public enum NextSpawnRateToIncrease
{
    OUTER_SPAWN_RATE,
    INNER_SPAWN_RATE
}

public class HyenasSpawnManager : MonoBehaviour, IGameStateProvider
{
    private HyenasSpawnPointAlgorithm spawnAlgorithm;

    [Header("Config params")]
    [SerializeField] private int startOuterSpawnRate = 3;
    [SerializeField] private int startInnerSpawnRate = 1;
    [SerializeField] private int minDistBetOuterSpawnPoints = 4;
    [SerializeField] private int minDistBetInnerSpawnPointAndHQ = 2;
    [SerializeField] private int corruptionNeededForSpawnRateIncrease = 80;
    [SerializeField] private NextSpawnRateToIncrease firstSpawnRateToIncrease = NextSpawnRateToIncrease.INNER_SPAWN_RATE;
    //[SerializeField] public Tilemap spawnPointTilemap;
    //[SerializeField] public TileBase spawnPointIndicatorTile;

    [Header("State")]
    [SerializeField] private int currentOuterSpawnRate;
    [SerializeField] private int currentInnerSpawnRate;
    [SerializeField] private NextSpawnRateToIncrease nextSpawnRateToIncrease;


    void Start()
    {
        // set up some fake state for testing
        InitializeInternalBoardRepresentationFromPredefinedArray();
        InitializeClaimedCells();

        this.spawnAlgorithm = new HyenasSpawnPointAlgorithm(this, minDistBetInnerSpawnPointAndHQ);
        this.currentInnerSpawnRate = startInnerSpawnRate;
        this.currentOuterSpawnRate = startOuterSpawnRate;
        this.nextSpawnRateToIncrease = firstSpawnRateToIncrease;

        // Testing
        GenerateNewSpawnPointsBasedOnSpawnRates();
        IncreaseSpawnRate();
        GenerateNewSpawnPointsBasedOnSpawnRates();
    }

    public void GenerateNewSpawnPointsBasedOnSpawnRates()
    {
        HashSet<Tuple<int, int>> generatedOuterSpawnPoints = spawnAlgorithm.GenerateRandomOuterSpawnPoints(currentOuterSpawnRate, minDistBetOuterSpawnPoints);
        LogUtils.LogEnumerable("generated outer spawn points", generatedOuterSpawnPoints);

        HashSet<Tuple<int, int>> generatedInnerSpawnPoints = spawnAlgorithm.GenerateRandomInnerSpawnPoints(currentInnerSpawnRate);
        LogUtils.LogEnumerable("generated inner spawn points", generatedInnerSpawnPoints);
    }

    public void IncreaseSpawnRate()
    {
        if(nextSpawnRateToIncrease == NextSpawnRateToIncrease.OUTER_SPAWN_RATE)
        {
            currentOuterSpawnRate++;
            nextSpawnRateToIncrease = NextSpawnRateToIncrease.INNER_SPAWN_RATE;
        }
        else
        {
            currentInnerSpawnRate++;
            nextSpawnRateToIncrease = NextSpawnRateToIncrease.OUTER_SPAWN_RATE;
        }

        Debug.Log("Hyenaes spawn rate increased! outer=" + currentOuterSpawnRate + ", inner=" + currentInnerSpawnRate);
    }


    // ================= FAKE STATE ====================

    int[,] board;
    HashSet<Tuple<int, int>> cellsClaimedByHyenas;
    HashSet<Tuple<int, int>> cellsClaimedByCats;

    //private int[,] predefinedBoard = new int[,]
    //{
    //    {-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,1,1,-1,-1,-1},
    //    {-1,-1,-1,-1,-1,-1,1,1,1,1,2,1,2,1,1,1,1,1,-1,-1,-1},
    //    {-1,-1,-1,-1,1,1,1,1,1,1,1,1,2,1,1,1,3,3,-1,-1,-1},
    //    {-1,-1,-1,-1,1,1,1,1,1,1,1,2,1,2,3,2,2,2,1,-1,-1},
    //    {-1,-1,-1,1,1,1,1,-1,-1,2,1,2,2,2,1,2,1,1,1,3,-1},
    //    {-1,-1,-1,-1,1,3,1,-1,-1,2,1,1,2,2,1,2,1,1,1,2,1},
    //    {-1,-1,-1,-1,3,2,1,1,1,1,1,2,2,3,2,-1,-1,2,3,2,1},
    //    {-1,-1,-1,1,1,1,1,1,1,2,2,1,2,2,3,-1,-1,2,1,1,-1},
    //    {-1,-1,-1,-1,1,1,2,1,1,2,1,1,3,1,2,1,1,2,1,-1,-1},
    //    {-1,2,1,2,2,3,2,1,1,2,2,1,1,2,1,1,3,-1,-1,-1,-1},
    //    {-1,3,1,1,2,1,1,1,1,2,1,1,2,1,1,1,2,1,-1,-1,-1},
    //    {-1,3,2,1,1,1,1,-1,-1,2,3,2,1,2,2,3,2,1,-1,-1,-1},
    //    {1,1,1,1,1,1,2,2,1,2,2,3,1,1,2,1,1,-1,-1,-1,-1},
    //    {-1,1,1,2,1,1,2,1,1,3,1,2,1,1,2,1,-1,-1,-1,-1,-1},
    //    {-1,-1,-1,-1,1,1,2,2,1,1,2,1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
    //    {-1,-1,-1,-1,1,1,2,1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1}
    //};

    int[,] predefinedBoard = new int[,]
    {
        {-1,-1,-1, 1, 1, 1, 1, 1},
        {-1, 1,-1, 1, 1,-1,-1, 1},
        {-1, 1, 1, 1, 1, 1, 1, 1},
        { 1, 1, 1, 1, 1, 1, 1,-1},
        { 1, 1,-1, 1, 1, 1, 1,-1},
        { 1, 1, 1, 1, 1, 1, 1, 1},
        {-1, 1,-1, 1, 1,-1, 1, 1}
    };

    void Print2DArray(int[,] array)
    {
        int rows = array.GetLength(0);
        int cols = array.GetLength(1);

        string output = "";
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                output += array[i, j] + " ";
            }
            output += "\n"; // New line for rows
        }
        Debug.Log(output);
    }

    void InitializeInternalBoardRepresentationFromPredefinedArray()
    {
        Print2DArray(predefinedBoard);

        int boardHeight = predefinedBoard.GetLength(0);
        int boardWidth = predefinedBoard.GetLength(1);
        Debug.Log("board height: " + boardHeight + ", width: " + boardWidth);

        int numRows = boardWidth;
        int numCols = boardHeight;
        Debug.Log("internal representation num rows " + numRows + ", cols: " + numCols);

        board = new int[numRows, numCols];

        for (int x = 0; x < numRows; x++)
        {
            for (int y = 0; y < numCols; y++)
            {
                board[x, y] = predefinedBoard[(boardHeight - 1) - y, x];
            }
        }

        Print2DArray(board);
    }
    
    void InitializeClaimedCells()
    {
        cellsClaimedByHyenas = new HashSet<Tuple<int, int>>();
        cellsClaimedByHyenas.Add(Tuple.Create(0, 2));
        cellsClaimedByHyenas.Add(Tuple.Create(7, 1));
        LogUtils.LogEnumerable("cells claimed by hyenas", cellsClaimedByHyenas);

        cellsClaimedByCats = new HashSet<Tuple<int, int>>();
        cellsClaimedByCats.Add(Tuple.Create(3, 2));
        cellsClaimedByCats.Add(Tuple.Create(3, 1));
        cellsClaimedByCats.Add(Tuple.Create(4, 1));
        LogUtils.LogEnumerable("cells claimed by cats", cellsClaimedByCats);
    }

    public int[,] GetBoard()
    {
        return board;
    }

    public Tuple<int, int> GetHQCell()
    {
        return Tuple.Create(4, 3);
    }

    public bool IsClaimedByHyenas(Tuple<int, int> cell)
    {
        return cellsClaimedByHyenas.Contains(cell);
    }

    public bool IsClaimedByCats(Tuple<int, int> cell)
    {
        return cellsClaimedByCats.Contains(cell);
    }

    public bool HasHyena(Tuple<int, int> cell)
    {
        return false;
    }
}

public interface IGameStateProvider
{
    public int[,] GetBoard();
    public Tuple<int, int> GetHQCell();
    public bool IsClaimedByHyenas(Tuple<int, int> cell);
    public bool IsClaimedByCats(Tuple<int, int> cell);
    public bool HasHyena(Tuple<int, int> cell);
}
