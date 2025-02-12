using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapManager : MonoBehaviour
{
    [SerializeField] public TileBase movementBaseTile;
    [SerializeField] public TileBase starterTowerTile;

    [SerializeField] private HyenasSpawnManager hyenasSpawnManager;


    public enum MapType : int{
        ground,
        resource,
        tower,
        movement,

        MAX_SIZE
    }

    int groundWidth;
    int groundHeight;

    public Tile[] groundTiles;
    public TileBase[] rockTiles;


    public Tilemap[] tilemapArray;

    List<Vector3Int> drawnTiles = new List<Vector3Int>();

    [SerializeField] private List<TileTerritoryData> territoryTiles;
    private Dictionary<TileBase, TileTerritoryData> dataFromTiles;

    [SerializeField] private List<TileResourceData> resourceTiles;
    private Dictionary<TileBase, TileResourceData> resourcesFromTiles;

    private Dictionary<Vector3Int, UnitController.Team> occupancy; //shitty solution but the old tilebase was aggregrate for every tile of that type or something. this would be better as an array but an array would be signifcantly more complicated

    private Dictionary<UnitController.Team, int> team_scores = new Dictionary<UnitController.Team, int>();

    public Vector3Int hqTowerPosition;
    public List<Vector3Int> minorTowers = new List<Vector3Int>();


    private void Awake(){
        dataFromTiles = new Dictionary<TileBase, TileTerritoryData>();
        occupancy = new Dictionary<Vector3Int, UnitController.Team>();
        foreach(var tileData in territoryTiles){
            foreach(var tile in tileData.tiles){
                dataFromTiles.Add(tile, tileData);
            }
        }

        resourcesFromTiles = new Dictionary<TileBase, TileResourceData>();
        foreach(var tileData in resourceTiles){
            foreach(var tile in tileData.tiles){
                resourcesFromTiles.Add(tile, tileData);
            }
        }
    }

    void Start()
    {

        team_scores.Add(UnitController.Team.cats, 0);
        team_scores.Add(UnitController.Team.hyena, 0);

        tilemapArray[(int)MapType.ground].ClearAllTiles();
        int[,] predefinedBoard = new int[,]
        {
            {-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,1,1,-1,-1,-1},
            {-1,-1,-1,-1,-1,-1,1,1,1,1,2,1,2,1,1,1,1,1,-1,-1,-1},
            {-1,-1,-1,-1,1,1,1,1,1,1,1,1,2,1,1,1,3,3,-1,-1,-1},
            {-1,-1,-1,-1,1,1,1,1,1,1,1,2,1,2,3,2,2,2,1,-1,-1},
            {-1,-1,-1,1,1,1,1,-1,-1,2,1,2,2,2,1,2,1,1,1,3,-1},
            {-1,-1,-1,-1,1,3,1,-1,-1,2,1,1,2,2,1,2,1,1,1,2,1},
            {-1,-1,-1,-1,3,2,1,1,1,1,1,2,2,3,2,-1,-1,2,3,2,1},
            {-1,-1,-1,1,1,1,1,1,1,2,2,1,2,2,3,-1,-1,2,1,1,-1},
            {-1,-1,-1,-1,1,1,2,1,1,2,1,1,3,1,2,1,1,2,1,-1,-1},
            {-1,2,1,2,2,3,2,1,1,2,2,1,1,2,1,1,3,-1,-1,-1,-1},
            {-1,3,1,1,2,1,1,1,1,2,1,1,2,1,1,1,2,1,-1,-1,-1},
            {-1,3,2,1,1,1,1,-1,-1,2,3,2,1,2,2,3,2,1,-1,-1,-1},
            {1,1,1,1,1,1,2,2,1,2,2,3,1,1,2,1,1,-1,-1,-1,-1},
            {-1,1,1,2,1,1,2,1,1,3,1,2,1,1,2,1,-1,-1,-1,-1,-1},
            {-1,-1,-1,-1,1,1,2,2,1,1,2,1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
            {-1,-1,-1,-1,1,1,2,1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1}
        };
        groundHeight = predefinedBoard.GetLength(0);
        groundWidth = predefinedBoard.GetLength(1);

        Vector3Int placementPos = Vector3Int.zero;
        placementPos.y = -groundHeight / 2;

        int rightSide = groundHeight / 2;
        int bottomSide = groundWidth / 2;

        for(; placementPos.y < rightSide; placementPos.y++){
            
            for(placementPos.x = -bottomSide; placementPos.x < bottomSide; placementPos.x++){
                if(predefinedBoard[placementPos.y + rightSide, placementPos.x + bottomSide] >= 0){
                    Vector3Int tempPos = -placementPos;
                    tempPos.x = -tempPos.x;
                    tilemapArray[(int)MapType.ground].SetTile(-placementPos, groundTiles[(int)UnitController.Team.neutral]);
                }
            }
        }

        Vector3Int checkTowerPosition = Vector3Int.zero;

        bool foundHQTower = false;
        for(checkTowerPosition.x = -rightSide; checkTowerPosition.x < rightSide; checkTowerPosition.x++){
            for(checkTowerPosition.y = -bottomSide; checkTowerPosition.y < bottomSide; checkTowerPosition.y++){
                TileBase tile = tilemapArray[(int)MapType.tower].GetTile(checkTowerPosition);
                if(tile != null){
                    hqTowerPosition = checkTowerPosition;
                    foundHQTower = true;
                    break;
                }
            }
            if(foundHQTower){
                break;
            }
        }

        // After we're done creating the map we need to initialize the hyenasSpawnAlgorithm with the data from the same tilemap
        // We also need to pass in the location of the HQ
        hyenasSpawnManager.InitializeInternalBoardRepresentationFromTilemap(tilemapArray[(int)MapType.ground], checkTowerPosition);
    }

    public void OnClick(){
        
    }

    public bool CheckIfMovementPossible(Vector3 mouseWorldPos){
        Vector3Int tilePosition = tilemapArray[(int)MapType.ground].WorldToCell(mouseWorldPos);
        
        TileBase tile = tilemapArray[(int)MapType.movement].GetTile<TileBase>(tilePosition);
        return tile != null;
    }
    public void ActivateMovementTile(Vector3Int tilePos, UnitController.Team team){
        TileBase tile = tilemapArray[(int)MapType.ground].GetTile<TileBase>(tilePos);
        if (tile != null)  // Check if a tile exists at that position
        {
            if(occupancy.TryGetValue(tilePos, out UnitController.Team occupantTeam)){
                    Debug.Log("aborting activating movement because tile was occupied : " + tilePos);
            }
            else{
                occupancy.Add(tilePos, team);
            }
            
            //TileData.Type tileType = dataFromTiles[tile].type;
            //Debug.Log("Clicked on tile at: " + tilePosition + " with type : " + tileType.ToString());
            tilemapArray[(int)MapType.movement].SetTile(tilePos, movementBaseTile);
            drawnTiles.Add(tilePos);
        }
    }
    public TileTerritoryData.Type CheckTileForHyenaSpawn(Vector3Int tilePos){
        TileBase tile = tilemapArray[(int)MapType.ground].GetTile<TileBase>(tilePos);
        if(tile != null){
            if(dataFromTiles.TryGetValue(tile, out TileTerritoryData val)){
                return val.type;
            }
        }
        return TileTerritoryData.Type.None;
    }

    public void ClaimTile(Vector3Int tilePos, UnitController.Team team){
        
        TileBase groundTile = tilemapArray[(int)MapType.ground].GetTile<TileBase>(tilePos);
        if(groundTile != null){
            if(dataFromTiles.TryGetValue(groundTile, out TileTerritoryData tilesTeam)){
                
                UnitController.Team tileTeam;

                if(tilesTeam.type == TileTerritoryData.Type.Cats){
                    tileTeam = UnitController.Team.cats;
                
                }
                else if(tilesTeam.type == TileTerritoryData.Type.Hyena){
                    tileTeam = UnitController.Team.hyena;
                }
                else{
                Debug.Log("tile is not on a valid team while trying to claim? : " + tilePos);
                    return;
                }
                
                if(team_scores.TryGetValue(UnitController.Team.cats, out int score)){
                    TileBase resourceTile = tilemapArray[(int)MapType.resource].GetTile<TileBase>(tilePos);
                    if (resourceTile != null)  // Check if a tile exists at that position
                    {
                        if(resourcesFromTiles.TryGetValue(resourceTile, out TileResourceData val)) {
                            //val.score;
                        }
                    }  
                }
            }
            else{
                Debug.Log("tile is not on a valid team while trying to claim? : " + tilePos);
                return;
            }

 
        }
    }

    public void ClearMovement(){
        foreach(Vector3Int tile in drawnTiles){
            tilemapArray[(int)MapType.movement].SetTile(tile, null);
        }
        drawnTiles.Clear();
    }
    public void SetOccupancy(Vector3 position, UnitController.Team team){
        Vector3Int tilePosition = tilemapArray[(int)MapType.ground].WorldToCell(position);
        
        if(occupancy.TryGetValue(tilePosition, out UnitController.Team occupantTeam)){
                Debug.Log("aborting setting occupancy because tile was occupied : " + tilePosition);
        }
        else{
            occupancy.Add(tilePosition, team);
        }
    }
    public void RemoveOccupancy(Vector3 position){
        Vector3Int tilePosition = tilemapArray[(int)MapType.ground].WorldToCell(position);
        if(occupancy.TryGetValue(tilePosition, out UnitController.Team occupantTeam)){
            occupancy.Remove(tilePosition);
        }
        else{
            Debug.Log("failed to remove occupancy, tile was not occupied");
        }
    }
    public void PlaceTower(Vector3Int towerTilePos){
        TileBase tile = tilemapArray[(int)MapType.ground].GetTile<TileBase>(towerTilePos);
        if(tile != null){
            tilemapArray[(int)MapType.tower].SetTile(towerTilePos, starterTowerTile);
            minorTowers.Add(towerTilePos);
        }
        ClearMovement();
    }

    void Update()
    {

    }
}