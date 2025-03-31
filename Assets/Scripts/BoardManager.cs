using System.Threading;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BoardManager : MonoBehaviour
{
    [Header("Config - Tilemap references")]
    [SerializeField] private Tilemap terrainTilemap;
    [SerializeField] private Tilemap resourceTilemap;

    [Header("Config - Terrain tile references")]
    [SerializeField] private TileBase neutralTerrainTile;
    [SerializeField] private TileBase catTerrainTile;
    [SerializeField] private TileBase hyenaTerrainTile;

    [Header("Config - Resource tile references")]
    [SerializeField] private TileBase[] resourceTiles;

    [Header("State")]
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private CellData[,] board;

    private int[,] predefinedBoard = new int[,]
    {
        { -1,  1,  2,  3, -1, -1 },
        { -1,  2,  3,  4,  1,  1 },
        {  2,  3, -1,  3,  2,  1 },
        { -1,  4,  3,  2,  1, -1 },
        { -1,  1,  2, -1, -1, -1 }
    };

    void Start()
    {
        InitializeBoardAndTilemapsFromPredefinedBoard();
    }

    private void InitializeBoardAndTilemapsFromPredefinedBoard()
    {
        width = predefinedBoard.GetLength(1);
        height = predefinedBoard.GetLength(0);

        board = new CellData[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int resourceValue = predefinedBoard[height - y - 1, x];

                board[x, y] = new CellData(resourceValue);

                if (resourceValue != -1)
                {
                    Vector3Int pos = new Vector3Int(x, y, 0);
                    terrainTilemap.SetTile(pos, neutralTerrainTile);
                    resourceTilemap.SetTile(pos, GetResourceTileForResourceValue(resourceValue));
                }
            }
        }
    }

    private TileBase GetResourceTileForResourceValue(int resourceValue)
    {
        switch (resourceValue)
        {
            case 2: return resourceTiles[0];
            case 3: return resourceTiles[1];
            case 4: return resourceTiles[2];
            default: return null;
        }
    }

    void Update()
    {
        
    }
}
