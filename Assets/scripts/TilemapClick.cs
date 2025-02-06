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

    public void OnClick(Vector3 sourcePos, Movement.Type movementType){
            Vector3Int sourceTilePos = tilemap.WorldToCell(sourcePos);
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int tilePosition = tilemap.WorldToCell(mouseWorldPos);
            Debug.Log("source - tile pos : " + sourceTilePos + ':' + tilePosition);

            int yDiff = sourceTilePos.y - tilePosition.y;
            int xDiff = sourceTilePos.x - tilePosition.x;
            
            RemovePath();

            


            Vector3Int vecStart = Vector3Int.zero;
            Vector3Int vecEnd = Vector3Int.zero;
            if(yDiff > xDiff){
                if(sourceTilePos.y > tilePosition.y){
                    sourceTilePos.y -= 1;
                }
                else if(sourceTilePos.y < tilePosition.y){
                    sourceTilePos.y += 1;
                }
                vecStart.y = Math.Min(sourceTilePos.y, tilePosition.y);
                vecEnd.y = Math.Max(sourceTilePos.y, tilePosition.y);

                vecStart.x = Math.Min(sourceTilePos.x, tilePosition.x);
                vecEnd.x = Math.Max(sourceTilePos.x, tilePosition.x);

                for(; vecStart.y < vecEnd.y; vecStart.y++){
                    for(; vecStart.x < vecEnd.x; vecStart.x++){
                        TileBase tile = tilemap.GetTile<TileBase>(vecStart);
                        if (tile != null)  // Check if a tile exists at that position
                        {
                            TileData.Type tileType = dataFromTiles[tile].type;

                            //Debug.Log("Clicked on tile at: " + tilePosition + " with type : " + tileType.ToString());
                            movementTilemap.SetTile(vecStart, selectedTile);
                            drawnTiles.Add(vecStart);
                        }
                    }
                }
            }
            else{
                if(sourceTilePos.x > tilePosition.x){
                    sourceTilePos.x -= 1;
                }
                else if (sourceTilePos.x < tilePosition.x){
                    sourceTilePos.x += 1;
                }
                vecStart.y = Math.Min(sourceTilePos.y, tilePosition.y);
                vecEnd.y = Math.Max(sourceTilePos.y, tilePosition.y);

                vecStart.x = Math.Min(sourceTilePos.x, tilePosition.x);
                vecEnd.x = Math.Max(sourceTilePos.x, tilePosition.x);

                for(; vecStart.x < vecEnd.x; vecStart.x++){
                    for(; vecStart.y < vecEnd.y; vecStart.y++){
                        TileBase tile = tilemap.GetTile<TileBase>(vecStart);
                        if (tile != null)  // Check if a tile exists at that position
                        {
                            TileData.Type tileType = dataFromTiles[tile].type;

                            //Debug.Log("Clicked on tile at: " + tilePosition + " with type : " + tileType.ToString());
                            movementTilemap.SetTile(vecStart, selectedTile);
                            drawnTiles.Add(vecStart);
                        }
                    }
                }
            }




    }
    public void RemovePath(){
        foreach(Vector3Int tile in drawnTiles){
            movementTilemap.SetTile(tile, null);
        }
        drawnTiles.Clear();
    }

    void Update()
    {

    }
}