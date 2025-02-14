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

    public Dictionary<UnitController.Team, int> team_scores = new Dictionary<UnitController.Team, int>();
    public Dictionary<Vector3Int, int> tower_healths = new Dictionary<Vector3Int, int>();

    public Vector3Int hqTowerPosition;
    public List<Vector3Int> minorTowers = new List<Vector3Int>();
    [SerializeField] private float noiseScale = 5.0f;
    [SerializeField] private float tier3NoiseGate = 0.1f;
    [SerializeField] private float tier2NoiseGate = 0.35f;
    [SerializeField] private float tier1NoiseGate = 0.7f;

    [SerializeField] private Vector2 noiseSeed = Vector2.zero;

    [SerializeField] private int noiseOctaves;
    [SerializeField] private float noisePersistence;
    [SerializeField] private float noiseLacunarity;

    public int[,] board;
    public int worldToArrayXOffset;
    public int worldToArrayYOffset;
    
    public BoundsInt groundBounds;

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

    float PerlinNoiseWithOctaves(float x, float y, float scale, Vector2 seed, int octaves, float persistence, float lacunarity)
{
    float total = 0f;
    float amplitude = 1f;
    float frequency = 1f;
    float maxValue = 0f;  // Used to normalize the result between 0 and 1

    for (int i = 0; i < octaves; i++)
    {
        total += Mathf.PerlinNoise((x / scale + seed.x) * frequency, (y / scale + seed.y) * frequency) * amplitude;

        maxValue += amplitude; // Track total amplitude to normalize values
        amplitude *= persistence; // Decrease amplitude (controls roughness)
        frequency *= lacunarity; // Increase frequency (controls detail)
    }

    return total / maxValue; // Normalize to keep values between 0-1
}


    void Start()
    {

        team_scores.Add(UnitController.Team.cats, 0);
        team_scores.Add(UnitController.Team.hyena, 0);

        tilemapArray[(int)MapType.ground].CompressBounds();
        groundBounds = tilemapArray[(int)MapType.ground].cellBounds;

        Vector3Int checkTowerPosition = Vector3Int.zero;

        bool foundHQTower = false;
        for(checkTowerPosition.x = groundBounds.xMin; checkTowerPosition.x < groundBounds.xMax; checkTowerPosition.x++){
            for(checkTowerPosition.y = groundBounds.yMin; checkTowerPosition.y < groundBounds.yMax; checkTowerPosition.y++){
                TileBase tile = tilemapArray[(int)MapType.tower].GetTile(checkTowerPosition);
                if(tile != null){

                    hqTowerPosition = tilemapArray[(int)MapType.ground].WorldToCell(tilemapArray[(int)MapType.tower].CellToWorld(checkTowerPosition));
                    Debug.Log("hq tower position");
                    foundHQTower = true;
                    break;
                }
            }
            if(foundHQTower){
                break;
            }
        }
        if(!foundHQTower){
            Debug.Log("failed to find hq tower - groundBounds - " + groundBounds.xMin + ":" + groundBounds.xMax + ":" + groundBounds.yMin + ":" + groundBounds.yMax);
            hqTowerPosition.x = 1;
            hqTowerPosition.y = -1;
        }

        //int noiseIter = 0;
        foreach (Vector3Int pos in groundBounds.allPositionsWithin)
        {
            if(hqTowerPosition == pos){
                continue;
            }
            if (tilemapArray[(int)MapType.ground].HasTile(pos)) // Check if a tile exists at this position
            {
                if(!tilemapArray[(int)MapType.resource].HasTile(pos)){

                    Vector3 noisePos = pos;
                    noisePos.x /= groundBounds.size.x; //might need to double this, idk if it matters
                    noisePos.y /= groundBounds.size.y;
                    float noiseVal = PerlinNoiseWithOctaves(noisePos.x, noisePos.y, noiseScale, noiseSeed, noiseOctaves, noisePersistence, noiseLacunarity);
                    //Debug.Log("noie val - " + noiseIter++ + " : " + noiseVal);
                    if(noiseVal < tier3NoiseGate){
                        tilemapArray[(int)MapType.resource].SetTile(pos, (TileBase)rockTiles[2]);
                    }
                    else if(noiseVal < tier2NoiseGate){
                        tilemapArray[(int)MapType.resource].SetTile(pos, (TileBase)rockTiles[1]);
                    }
                    else if(noiseVal < tier1NoiseGate){
                        tilemapArray[(int)MapType.resource].SetTile(pos, (TileBase)rockTiles[0]);
                    }
                }
                else{
                    tilemapArray[(int)MapType.resource].SetTile(pos, (TileBase)rockTiles[2]);
                }
            }
        }
        
        ClaimAreaAroundTower(hqTowerPosition);

        // After we're done creating the map we need to initialize the hyenasSpawnAlgorithm with the data from the same tilemap
        // We also need to pass in the location of the HQ
        hyenasSpawnManager.InitializeInternalBoardRepresentationFromTilemap(tilemapArray[(int)MapType.ground], checkTowerPosition);
    }

    public void OnClick(){
        
    }

    public bool GetValidGroundTile(Vector3Int tilePos){
        return tilemapArray[(int)MapType.ground].GetTile<TileBase>(tilePos) != null;
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
            //if(occupancy.TryGetValue(tilePos, out UnitController.Team occupantTeam)){
                    //Debug.Log("aborting activating movement because tile was occupied : " + tilePos);
            //}
            //else{
            //    occupancy.Add(tilePos, team);
            //}
            
            //TileData.Type tileType = dataFromTiles[tile].type;
            //Debug.Log("Clicked on tile at: " + tilePosition + " with type : " + tileType.ToString());
            tilemapArray[(int)MapType.movement].SetTile(tilePos, movementBaseTile);
            drawnTiles.Add(tilePos);
        }
    }

    public bool TryActivateMovementTile(Vector3Int tilePos, UnitController.Team team){
        if(!GetValidGroundTile(tilePos)){
            return false;
        }
        else{
            tilemapArray[(int)MapType.movement].SetTile(tilePos, movementBaseTile);
            drawnTiles.Add(tilePos);
            return true;
        }
    }

    public void AddResourceToScore(Vector3Int resPos, UnitController.Team team, int multiplier = 1){ //set multiplier to negative to subtract
        TileBase resourceTile = tilemapArray[(int)MapType.resource].GetTile<TileBase>(resPos);
        if (resourceTile != null)  // Check if a tile exists at that position
        {
            
            Debug.Log("resource tile existed");
            if(resourcesFromTiles.TryGetValue(resourceTile, out TileResourceData resTile)) {
                Debug.Log("res tile existed");
                team_scores[team] += resTile.score * multiplier;
            }
        }
    }
    public void CollectResourcesFromTowers() {
        
        Vector3Int offset = new Vector3Int(-1, -1, 0);
        for(offset.x = -1; offset.x <= 1; offset.x++) {
            for(offset.y = -1; offset.y <= 1; offset.y++) {
                foreach(var tower in minorTowers){
                    Debug.Log("checking resource map : " + (tower + offset));
                    AddResourceToScore(tower + offset, UnitController.Team.cats, 2);
                }
                    AddResourceToScore(hqTowerPosition + offset, UnitController.Team.cats, 2);
            }
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

    private void TryClaimTileWithTilePos(Vector3Int tilePos, UnitController.Team team){
        TileBase groundTile = tilemapArray[(int)MapType.ground].GetTile<TileBase>(tilePos);
        if(groundTile != null){
            tilemapArray[(int)MapType.ground].SetTile(tilePos, groundTiles[(int)team]);
            if(dataFromTiles.TryGetValue(groundTile, out TileTerritoryData tilesTeam)){
                
                UnitController.Team tileTeam;

                if(tilesTeam.type == TileTerritoryData.Type.Cats){
                    tileTeam = UnitController.Team.cats;
                
                }
                else if(tilesTeam.type == TileTerritoryData.Type.Hyena){
                    tileTeam = UnitController.Team.hyena;
                }
                else if(tilesTeam.type == TileTerritoryData.Type.None){
                    tileTeam = UnitController.Team.neutral;
                }
                else{
                    //Debug.Log("territory tile is not on a valid team while trying to claim? : " + tilePos);
                    return;
                }
                if(tileTeam == team){
                    return;
                }

                AddResourceToScore(tilePos, team, 1);
                if((tileTeam == UnitController.Team.hyena) || (tileTeam == UnitController.Team.cats)){
                    AddResourceToScore(tilePos, tileTeam, -1);
                }
            }
            else{
                Debug.Log("tile is not on a valid team while trying to claim? : " + tilePos);
                return;
            }

 
        }
    }

    public void TryClaimTile(Vector3 transformPos, UnitController.Team team){

        //Debug.Log("attempting to claim tile");
        Vector3 myPos = transformPos;
        myPos.x -= Mathf.Floor(myPos.x) + 0.5f;
        myPos.y -= Mathf.Floor(myPos.y) + 0.5f;
        myPos.x = Mathf.Abs(myPos.x);
        myPos.y = Mathf.Abs(myPos.y);
        if(myPos.x > 0.1f && myPos.y > 0.1f){
            //Debug.Log("failed to claim, not in the center of the tile");
            return;
        }
        Vector3Int tilePos = tilemapArray[(int)TilemapManager.MapType.ground].WorldToCell(transformPos);
        
        TryClaimTileWithTilePos(tilePos, team);
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
                //Debug.Log("aborting setting occupancy because tile was occupied : " + tilePosition);
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
    
    public void ClaimAreaAroundTower(Vector3Int towerPos){
        Vector3Int offset = new Vector3Int(-1, -1, 0);
        for(offset.x = -1; offset.x <= 1; offset.x++) {
            for(offset.y = -1; offset.y <= 1; offset.y++) {
                TryClaimTileWithTilePos(towerPos + offset, UnitController.Team.cats);
            }
        }
    }
    public void PlaceTower(Vector3Int towerTilePos){
        TileBase tile = tilemapArray[(int)MapType.ground].GetTile<TileBase>(towerTilePos);
        if(tile != null){
            tilemapArray[(int)MapType.tower].SetTile(towerTilePos, starterTowerTile);
            minorTowers.Add(towerTilePos);
            ClaimAreaAroundTower(towerTilePos);
        }
        ClearMovement();
    }
}