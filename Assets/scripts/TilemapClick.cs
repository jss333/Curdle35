using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapClick : MonoBehaviour
{
    [SerializeField] public TileBase selectedTile;

    public Tilemap movementTilemap;
    public Tilemap tilemap;  // Assign your Tilemap in the Inspector

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

    void Update()
    {

    }
}