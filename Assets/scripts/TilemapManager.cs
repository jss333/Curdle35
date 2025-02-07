using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapManager : MonoBehaviour
{
    [SerializeField] public TileBase movementBaseTile;
    [SerializeField] public TileBase starterTowerTile;

    public Tilemap movementTilemap;
    public Tilemap tilemap;  // Assign your Tilemap in the Inspector
    public Tilemap towerMap;

    List<Vector3Int> drawnTiles = new List<Vector3Int>();

    [SerializeField] private List<TileData> tileDatas;
    private Dictionary<TileBase, TileData> dataFromTiles;

    private List<Vector3Int> occupancy; //shitty solution but the old tilebase was aggregrate for every tile of that type or something. this would be better as an array but an array would be signifcantly more complicated

    private void Awake(){
        dataFromTiles = new Dictionary<TileBase, TileData>();
        occupancy = new List<Vector3Int>();
        foreach(var tileData in tileDatas){
            foreach(var tile in tileData.tiles){
                dataFromTiles.Add(tile, tileData);
            }
        }
    }

    public void OnClick(){
        
    }

    public bool CheckIfMovementPossible(Vector3 mouseWorldPos){
        Vector3Int tilePosition = tilemap.WorldToCell(mouseWorldPos);
        
        TileBase tile = movementTilemap.GetTile<TileBase>(tilePosition);
        return tile != null;
    }
    public void ActivateMovementTile(Vector3Int tilePos){
        TileBase tile = tilemap.GetTile<TileBase>(tilePos);
        if (tile != null)  // Check if a tile exists at that position
        {
            if(occupancy.Contains(tilePos)){
                    Debug.Log("aborting activating movement because tile was occupied : " + tilePos);
            }
            else{
                occupancy.Add(tilePos);
            }
            
            //TileData.Type tileType = dataFromTiles[tile].type;
            //Debug.Log("Clicked on tile at: " + tilePosition + " with type : " + tileType.ToString());
            movementTilemap.SetTile(tilePos, movementBaseTile);
            drawnTiles.Add(tilePos);
        }
    }
    public void ClearMovement(){
        foreach(Vector3Int tile in drawnTiles){
            movementTilemap.SetTile(tile, null);
        }
        drawnTiles.Clear();
    }
    public void SetOccupancy(Vector3 position){
        Vector3Int tilePosition = tilemap.WorldToCell(position);
        
        if(occupancy.Contains(tilePosition)){
                Debug.Log("aborting setting occupancy because tile was occupied : " + tilePosition);
        }
        else{
            occupancy.Add(tilePosition);
        }
    }
    public void RemoveOccupancy(Vector3 position){
        Vector3Int tilePosition = tilemap.WorldToCell(position);
        if(occupancy.Contains(tilePosition)){
            occupancy.Remove(tilePosition);
        }
        else{
            Debug.Log("failed to remove occupancy, tile was not occupied");
        }
    }
    public void PlaceTower(Vector3Int towerTilePos){
        TileBase tile = movementTilemap.GetTile<TileBase>(towerTilePos);
        if(tile != null){
            towerMap.SetTile(towerTilePos, starterTowerTile);
        }
        ClearMovement();
    }

    void Update()
    {

    }
}