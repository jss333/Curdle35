using UnityEngine;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using UnityEngine.Tilemaps;


public enum NextSpawnRateToIncrease
{
    OUTER_SPAWN_RATE,
    INNER_SPAWN_RATE
}

public class HyenasSpawnManager : MonoBehaviour, IGameStateProvider
{
    [Header("Config params")]
    [SerializeField] private int startOuterSpawnRate = 3;
    [SerializeField] private int startInnerSpawnRate = 1;
    [SerializeField] private int minDistBetOuterSpawnPoints = 4;
    [SerializeField] private int minDistBetInnerSpawnPointAndHQ = 2;
    [SerializeField] private int corruptionNeededForSpawnRateIncrease = 80;
    [SerializeField] private NextSpawnRateToIncrease firstSpawnRateToIncrease = NextSpawnRateToIncrease.INNER_SPAWN_RATE;

    [Header("References")]
    [SerializeField] private TilemapManager tilemapManager;
    [SerializeField] private GameObject spawnMarkerPrefab;
    [SerializeField] private HyenaController hyenaPrefab;
    [SerializeField] private GameObject dustCloudPrefab;
    [SerializeField] private HyenaController hyenasParent;

    [Header("State")]
    [SerializeField] private int currentOuterSpawnRate;
    [SerializeField] private int currentInnerSpawnRate;
    [SerializeField] private NextSpawnRateToIncrease nextSpawnRateToIncrease;

    private HyenasSpawnPointAlgorithm spawnAlgorithm;
    public List<GameObject> spawnMarkers = new List<GameObject>();

    Tuple<int, int> hqPos;

    public void InitializeInternalBoardRepresentationFromTilemap(Tilemap tilemap, Vector3Int hqPosWorld)
    {
        Vector3Int? leftmostTile = null;
        Vector3Int? rightmostTile = null;
        Vector3Int? bottommostTile = null;
        Vector3Int? topmostTile = null;

        BoundsInt bounds = tilemap.cellBounds;

        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            if (tilemap.HasTile(pos)) // Check if a tile exists at this position
            {
                // First found tile becomes initial reference
                if (leftmostTile == null || pos.x < leftmostTile.Value.x) leftmostTile = pos;
                if (rightmostTile == null || pos.x > rightmostTile.Value.x) rightmostTile = pos;
                if (bottommostTile == null || pos.y < bottommostTile.Value.y) bottommostTile = pos;
                if (topmostTile == null || pos.y > topmostTile.Value.y) topmostTile = pos;
            }
        }

        int minX = leftmostTile.Value.x;
        int maxX = rightmostTile.Value.x;
        int width = maxX - minX + 1;
        tilemapManager.worldToArrayXOffset = Math.Abs(minX);

        int minY = bottommostTile.Value.y;
        int maxY = topmostTile.Value.y;
        int height = maxY - minY + 1;
        tilemapManager.worldToArrayYOffset = Math.Abs(minY);

        //Debug.Log($"Determined min and max coordinates from tilemap = minX:{minX} maxX:{maxX} width:{width} --- minY:{minY} maxY:{maxY} height:{hegith}");

        tilemapManager.board = new int[width, height];

        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            if (tilemap.HasTile(pos))
            {
                tilemapManager.board[pos.x + tilemapManager.worldToArrayXOffset, pos.y + tilemapManager.worldToArrayYOffset] = 1;
            }
            else
            {
                tilemapManager.board[pos.x + tilemapManager.worldToArrayXOffset, pos.y + tilemapManager.worldToArrayYOffset] = -1;
            }
        }

        hqPos = ConvertFromWorldPos(hqPosWorld);

        //Debug.Log($"Internal representation of world created 0-length:{tilemapManager.board.GetLength(0)}   1-length:{tilemapManager.board.GetLength(1)}");
        //LogUtils.Print2DArray(tilemapManager.board);
        //Debug.Log($"HQ is positioned at {hqPosWorld} --> {hqPos}");

        DoStart();
    }

    void DoStart()
    {
        this.spawnAlgorithm = new HyenasSpawnPointAlgorithm(this, minDistBetInnerSpawnPointAndHQ);
        this.currentInnerSpawnRate = startInnerSpawnRate;
        this.currentOuterSpawnRate = startOuterSpawnRate;
        this.nextSpawnRateToIncrease = firstSpawnRateToIncrease;

        // Testing
        //GenerateNewSpawnPointsBasedOnSpawnRates();
        //SpawnHyenasAtSpawnPoints();
        //IncreaseSpawnRate();
        //GenerateNewSpawnPointsBasedOnSpawnRates();
    }

    public void GenerateNewSpawnPointsBasedOnSpawnRates()
    {
        HashSet<Tuple<int, int>> allSpawnPoints = spawnAlgorithm.GenerateRandomOuterSpawnPoints(currentOuterSpawnRate, minDistBetOuterSpawnPoints);
        allSpawnPoints.UnionWith(spawnAlgorithm.GenerateRandomInnerSpawnPoints(currentInnerSpawnRate));
        //LogUtils.LogEnumerable("generated spawn points", allSpawnPoints);

        foreach (Tuple<int, int> spawnPoint in allSpawnPoints)
        {
            Vector3Int worldPosition = ConvertFromTuple(spawnPoint);
            //Debug.Log($"new spawn marker ${spawnPoint} -> ${worldPosition}");
            GameObject spawnMarkerObj = Instantiate(spawnMarkerPrefab, worldPosition, Quaternion.identity, transform);
            spawnMarkers.Add(spawnMarkerObj);
        }
    }

    public List<HyenaController> SpawnHyenasAtSpawnPoints()
    {
        //Debug.Log("Spawning hyenas where the spawn markers are");

        List<HyenaController> ret = new List<HyenaController>();
        foreach(GameObject spawnMarker in spawnMarkers)
        {
            Vector3 spawnMarkerPos = spawnMarker.transform.position;
            //Transform transform = new Transform();
            var tempHyena = Instantiate(hyenaPrefab, spawnMarkerPos, Quaternion.identity);
            if(tempHyena != null){
                ret.Add(tempHyena);
            }
            else{
                Debug.Log("failed to create hyena");
            }
            GameObject dustCloudObj = Instantiate(dustCloudPrefab, spawnMarkerPos, Quaternion.identity);
            Destroy(spawnMarker);
        }

        spawnMarkers.Clear();
        return ret;
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


    public int[,] GetBoard()
    {
        return tilemapManager.board;
    }

    public Tuple<int, int> GetHQCell()
    {
        return hqPos;
    }

    public bool IsClaimedByHyenas(Tuple<int, int> cell)
    {
        return tilemapManager.CheckTileForHyenaSpawn(ConvertFromTuple(cell)) == TileTerritoryData.Type.Hyena;
    }

    public bool IsClaimedByCats(Tuple<int, int> cell)
    {
        return tilemapManager.CheckTileForHyenaSpawn(ConvertFromTuple(cell)) == TileTerritoryData.Type.Cats;
    }

    public bool HasHyena(Tuple<int, int> cell)
    {
        return false; //TODO
    }

    private Tuple<int, int> ConvertFromWorldPos(Vector3Int worldPos)
    {
        return Tuple.Create(worldPos.x + tilemapManager.worldToArrayXOffset, worldPos.y + tilemapManager.worldToArrayYOffset);
    }

    private Vector3Int ConvertFromTuple(Tuple<int, int> arrayPos)
    {
        Vector3Int worldPos = Vector3Int.zero;
        worldPos.x = arrayPos.Item1 - tilemapManager.worldToArrayXOffset;
        worldPos.y = arrayPos.Item2 - tilemapManager.worldToArrayYOffset;
        return worldPos;
    }


    // ================= FAKE STATE ====================

    //int[,] board;
    //HashSet<Tuple<int, int>> cellsClaimedByHyenas;
    //HashSet<Tuple<int, int>> cellsClaimedByCats;

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

    //int[,] predefinedBoard = new int[,]
    //{
    //    {-1,-1,-1, 1, 1, 1, 1, 1},
    //    {-1, 1,-1, 1, 1,-1,-1, 1},
    //    {-1, 1, 1, 1, 1, 1, 1, 1},
    //    { 1, 1, 1, 1, 1, 1, 1,-1},
    //    { 1, 1,-1, 1, 1, 1, 1,-1},
    //    { 1, 1, 1, 1, 1, 1, 1, 1},
    //    {-1, 1,-1, 1, 1,-1, 1, 1}
    //};

    //void InitializeInternalBoardRepresentationFromPredefinedArray()
    //{
    //    Print2DArray(predefinedBoard);

    //    int boardHeight = predefinedBoard.GetLength(0);
    //    int boardWidth = predefinedBoard.GetLength(1);
    //    Debug.Log("board height: " + boardHeight + ", width: " + boardWidth);

    //    int numRows = boardWidth;
    //    int numCols = boardHeight;
    //    Debug.Log("internal representation num rows " + numRows + ", cols: " + numCols);

    //    board = new int[numRows, numCols];

    //    for (int x = 0; x < numRows; x++)
    //    {
    //        for (int y = 0; y < numCols; y++)
    //        {
    //            board[x, y] = predefinedBoard[(boardHeight - 1) - y, x];
    //        }
    //    }

    //    Print2DArray(board);
    //}

    //void InitializeClaimedCells()
    //{
    //    cellsClaimedByHyenas = new HashSet<Tuple<int, int>>();
    //    cellsClaimedByHyenas.Add(Tuple.Create(0, 2));
    //    cellsClaimedByHyenas.Add(Tuple.Create(7, 1));
    //    LogUtils.LogEnumerable("cells claimed by hyenas", cellsClaimedByHyenas);

    //    cellsClaimedByCats = new HashSet<Tuple<int, int>>();
    //    cellsClaimedByCats.Add(Tuple.Create(3, 2));
    //    cellsClaimedByCats.Add(Tuple.Create(3, 1));
    //    cellsClaimedByCats.Add(Tuple.Create(4, 1));
    //    LogUtils.LogEnumerable("cells claimed by cats", cellsClaimedByCats);
    //}
}

public interface IGameStateProvider
{
    public int[,] GetBoard();
    public Tuple<int, int> GetHQCell();
    public bool IsClaimedByHyenas(Tuple<int, int> cell);
    public bool IsClaimedByCats(Tuple<int, int> cell);
    public bool HasHyena(Tuple<int, int> cell);
}
