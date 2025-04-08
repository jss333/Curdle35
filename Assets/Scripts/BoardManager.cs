using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance { get; private set; }

    [Header("Config - Tilemap references")]
    [SerializeField] private Tilemap terrainTilemap;
    [SerializeField] private Tilemap resourceTilemap;
    [SerializeField] private Tilemap movementRangeTilemap;

    [Header("Config - Terrain tile references")]
    [SerializeField] private TileBase neutralTerrainTile;
    [SerializeField] private TileBase catTerrainTile;
    [SerializeField] private TileBase hyenaTerrainTile;

    [Header("Config - Resource tile references")]
    [SerializeField] private TileBase[] resourceTiles;

    [Header("Config - Movement range tile reference")]
    [SerializeField] private TileBase movementRangeTile;

    [Header("State")]
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private CellData[,] board;
    [SerializeField] private Dictionary<Vector2Int, Unit> units = new Dictionary<Vector2Int, Unit>();

    private int[,] predefinedBoard = new int[,]
    {
        {-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1, 1, 1,-1,-1,-1},
        {-1,-1,-1,-1,-1,-1, 1, 1, 1, 1, 2, 1, 2, 1, 1, 1, 1, 1,-1,-1,-1},
        {-1,-1,-1,-1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 1, 1, 1, 3, 3,-1,-1,-1},
        {-1,-1,-1,-1, 1, 4, 1, 1, 1, 1, 1, 2, 1, 2, 3, 2, 4, 2, 1,-1,-1},
        {-1,-1,-1, 1, 1, 1, 1,-1,-1, 2, 1, 2, 2, 2, 1, 2, 1, 1, 1, 3,-1},
        {-1,-1,-1,-1, 1, 3, 1,-1,-1, 2, 1, 1, 2, 2, 1, 2, 1, 1, 1, 2, 1},
        {-1,-1,-1,-1, 3, 2, 1, 1, 1, 1, 1, 2, 2, 3, 2,-1,-1, 2, 3, 2, 1},
        {-1,-1,-1, 1, 1, 1, 1, 1, 1, 2, 2, 1, 4, 2, 3,-1,-1, 2, 1, 1,-1},
        {-1,-1,-1,-1, 1, 1, 2, 1, 1, 2, 1, 1, 3, 1, 2, 1, 1, 2, 1,-1,-1},
        {-1, 2, 1, 2, 2, 3, 2, 1, 1, 2, 2, 1, 1, 2, 1, 1, 3,-1,-1,-1,-1},
        {-1, 3, 1, 1, 2, 1, 1, 1, 1, 2, 1, 1, 2, 1, 1, 1, 2, 1,-1,-1,-1},
        {-1, 3, 2, 1, 1, 1, 1,-1,-1, 2, 3, 2, 1, 2, 2, 3, 2, 1,-1,-1,-1},
        { 1, 1, 1, 1, 1, 1, 2, 2, 1, 2, 2, 4, 1, 1, 2, 1, 1,-1,-1,-1,-1},
        {-1, 1, 1, 2, 1, 1, 2, 1, 1, 3, 1, 2, 1, 1, 2, 1,-1,-1,-1,-1,-1},
        {-1,-1,-1,-1, 1, 1, 2, 2, 1, 1, 2, 1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
        {-1,-1,-1,-1, 1, 1, 2, 1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1}
    };

    void Awake()
    {
        Instance = this;
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
                    terrainTilemap.SetTile(pos, GetTerrainTileForFaction(Faction.None));
                    resourceTilemap.SetTile(pos, GetResourceTileForResourceValue(resourceValue));
                }
            }
        }
    }

    private TileBase GetTerrainTileForFaction(Faction faction)
    {
        switch (faction)
        {
            case Faction.Cats:
                return catTerrainTile;
            case Faction.Hyenas:
                return hyenaTerrainTile;
            case Faction.None:
            default:
                return neutralTerrainTile;
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

    public void ShowMovementRange(MovementRange mvmtRange)
    {
        IEnumerable<Vector2Int> cells = mvmtRange.GetValidCells();

        foreach (var cell in cells)
        {
            movementRangeTilemap.SetTile((Vector3Int)cell, movementRangeTile);
        }
        // Also highlight the cell the unit is standing on
        movementRangeTilemap.SetTile((Vector3Int)mvmtRange.GetUnit().GetBoardPosition(), movementRangeTile);
    }

    public void ClearMovementRange()
    {
        movementRangeTilemap.ClearAllTiles();
    }

    public Unit GetUnitAt(Vector2Int pos)
    {
        if (units.ContainsKey(pos))
        {
            return units[pos];
        }
        return null;
    }

    public void RegisterUnitPos(Unit unit, Vector2Int pos)
    {
        if (units.ContainsKey(pos))
        {
            Unit oldUnit = units[pos];
            if (oldUnit != unit)
            {
                Debug.LogWarning("Unit already registered at position " + pos + ". Replacing " + oldUnit.name + " with " + unit.name + ".");
            }
        }
            
        units[pos] = unit;
    }

    public void UpdateUnitPosRegister(Unit unit, Vector2Int oldPos, Vector2Int newPos)
    {
        if (units.ContainsKey(oldPos) && units[oldPos] == unit)
        {
            units.Remove(oldPos);
        }
        else
        {
            Debug.LogWarning("Unit " + unit.name + " is not registered at position " + oldPos + ".");
        }
        
        RegisterUnitPos(unit, newPos);
    }

    public bool IsValidCellForUnitMovement(Vector2Int pos)
    {
        return IsWithinBounds(pos) && !IsVoidCell(pos);
    }

    public bool IsVoidCell(Vector2Int pos)
    {
        return board[pos.x, pos.y].IsVoidCell();
    }

    private bool IsWithinBounds(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < width &&
               pos.y >= 0 && pos.y < height;
    }

    public void ClaimCell(Vector2Int pos, Faction faction)
    {
        if (IsValidCellForUnitMovement(pos))
        {
            board[pos.x, pos.y].owner = faction;
            terrainTilemap.SetTile((Vector3Int)pos, GetTerrainTileForFaction(faction));
        }
    }

    public int GetWidth()
    {
        return width;
    }

    public int GetHeight()
    {
        return height;
    }
}
