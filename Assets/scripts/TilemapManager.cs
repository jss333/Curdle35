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

        player,
        occupant,

        MAX_SIZE
    }

    public Tile[] groundTiles;


    public Tilemap[] tilemapArray;

    List<Vector3Int> drawnTiles = new List<Vector3Int>();

    [SerializeField] private List<TileData> tileDatas;
    private Dictionary<TileBase, TileData> dataFromTiles;

    private Dictionary<Vector3Int, UnitController.Team> occupancy; //shitty solution but the old tilebase was aggregrate for every tile of that type or something. this would be better as an array but an array would be signifcantly more complicated

    private void Awake(){
        dataFromTiles = new Dictionary<TileBase, TileData>();
        occupancy = new Dictionary<Vector3Int, UnitController.Team>();
        foreach(var tileData in tileDatas){
            foreach(var tile in tileData.tiles){
                dataFromTiles.Add(tile, tileData);
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
        }
        ClearMovement();
    }

    void Update()
    {

    }
}