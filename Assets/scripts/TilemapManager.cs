using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapManager : MonoBehaviour
{
    [SerializeField] public TileBase movementBaseTile;
    [SerializeField] public TileBase starterTowerTile;


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

    [SerializeField] private List<TileData> tileDatas;
    private Dictionary<TileBase, TileData> dataFromTiles;

    private Dictionary<Vector3Int, UnitController.Team> occupancy; //shitty solution but the old tilebase was aggregrate for every tile of that type or something. this would be better as an array but an array would be signifcantly more complicated

    public Vector3Int hqTowerPosition;
    public List<Vector3Int> minorTowers = new List<Vector3Int>();


    private void Awake(){
        dataFromTiles = new Dictionary<TileBase, TileData>();
        occupancy = new Dictionary<Vector3Int, UnitController.Team>();
        foreach(var tileData in tileDatas){
            foreach(var tile in tileData.tiles){
                dataFromTiles.Add(tile, tileData);
            }
        }
    }

    void Start()
    {

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

    //return -1 for empty tile(non traversable)
    // 0 for neutral
    // 1 for cats
    // 2 for hyenas
    public int CheckTileForHyenaSpawn(Vector3Int tilePos){
        TileBase tile = tilemapArray[(int)MapType.ground].GetTile<TileBase>(tilePos);
        if(tile != null){
            if(dataFromTiles.TryGetValue(tile, out TileData val)){
                return (int)val.type;
            }
            else{
                return -1;
            }
        }
        else{
            return -1;
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