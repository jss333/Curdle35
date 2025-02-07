using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapManager : MonoBehaviour
{
    [SerializeField] public TileBase selectedTile;

    public Tilemap movementTilemap;
    public Tilemap tilemap;  // Assign your Tilemap in the Inspector
    public Tilemap towerMap;

    List<Vector3Int> drawnTiles = new List<Vector3Int>();

    [SerializeField] private List<TileData> tileDatas;
    private Dictionary<TileBase, TileData> dataFromTiles;

    private void Awake(){
        dataFromTiles = new Dictionary<TileBase, TileData>();
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
            if(dataFromTiles.TryGetValue(tile, out TileData outTile)){
                if(outTile.occupants.Count > 0){
                    return;
                }
            }
            TileData.Type tileType = dataFromTiles[tile].type;

            //Debug.Log("Clicked on tile at: " + tilePosition + " with type : " + tileType.ToString());
            movementTilemap.SetTile(tilePos, selectedTile);
            drawnTiles.Add(tilePos);
        }
    }
    public void ClearMovement(){
        foreach(Vector3Int tile in drawnTiles){
            movementTilemap.SetTile(tile, null);
        }
        drawnTiles.Clear();
    }
    public void SetOccupancy(UnitController unit){
        Vector3Int tilePosition = tilemap.WorldToCell(unit.transform.position);
        
        TileBase tile = movementTilemap.GetTile<TileBase>(tilePosition);
        if(tile == null){
            Debug.Log("FATAL ERROR : player is trying to set occupancy in a null tile");
        }
        if(dataFromTiles.TryGetValue(tile, out TileData outTile)){
            outTile.occupants.Add(unit);
        }
    }
    public void RemoveOccupancy(UnitController unit){
        Vector3Int tilePosition = tilemap.WorldToCell(unit.transform.position);
        
        TileBase tile = movementTilemap.GetTile<TileBase>(tilePosition);
        if(dataFromTiles.TryGetValue(tile, out TileData outTile)){
//#if DEBUG
            if(!outTile.occupants.Contains(unit)){
                Debug.Log("trying to remove an occupant that isn't occupying tile");
            }
            else{
                outTile.occupants.Remove(unit);
            }
//#else
            //outTile.occupants.Remove(unit);
//#endif
        }
    }

    void Update()
    {

    }
}