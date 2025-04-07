using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

public class TilemapManager : MonoBehaviour
{
    [SerializeField] public TileBase movementBaseTile;
    [SerializeField] public TileBase starterTowerTile;
    [SerializeField] private OldHyenasSpawnManager hyenasSpawnManager;

    public int towerPlacementResourceCost = 50;

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

    [SerializeField] public Dictionary<UnitController.Team, Vector2Int> team_scores = new Dictionary<UnitController.Team, Vector2Int>(); //x is the real score, y is the score per round
    public Dictionary<Vector3Int, TowerBehavior> towerData = new Dictionary<Vector3Int, TowerBehavior>();

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
        occupancy = new Dictionary<Vector3Int, UnitController.Team>();

        dataFromTiles = new Dictionary<TileBase, TileTerritoryData>();
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

        team_scores.Add(UnitController.Team.cats, new Vector2Int(0, 0));
        team_scores.Add(UnitController.Team.hyena, new Vector2Int(0, 0));

        tilemapArray[(int)MapType.ground].CompressBounds();
        groundBounds = tilemapArray[(int)MapType.ground].cellBounds;

        Vector3Int checkTowerPosition = Vector3Int.zero;

        bool foundHQTower = false;
        for(checkTowerPosition.x = groundBounds.xMin; checkTowerPosition.x < groundBounds.xMax; checkTowerPosition.x++){
            for(checkTowerPosition.y = groundBounds.yMin; checkTowerPosition.y < groundBounds.yMax; checkTowerPosition.y++){
                TileBase tile = tilemapArray[(int)MapType.tower].GetTile(checkTowerPosition);
                if(tile != null){

                    hqTowerPosition = tilemapArray[(int)MapType.ground].WorldToCell(tilemapArray[(int)MapType.tower].CellToWorld(checkTowerPosition));
                    Debug.Log("hq tower position: " + checkTowerPosition);
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
        
        TowerBehavior hqTower = new TowerBehavior(hqTowerPosition, 3, true);
        ClaimAreaAroundTower(hqTowerPosition, hqTower);
        towerData.Add(hqTowerPosition, hqTower);


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


    public int GetResourceValueAtPos(Vector3Int resPos)
    {
        TileBase resourceTile = ResourceTilemap().GetTile<TileBase>(resPos);
        if (resourceTile != null)  // Check if a tile exists at that position
        {
            if (resourcesFromTiles.TryGetValue(resourceTile, out TileResourceData resTile))
            {
                return resTile.score;
            }
        }

        return 0; //no tile means no resource value
    }

    public void AddResourceToScorePerRound(int resourceVal, UnitController.Team team)
    {
        Vector2Int tempScore = team_scores[team];
        //Debug.Log($"Adding {resourceVal} to {team} score -- before {team_scores[team]}");
        tempScore.y += resourceVal;
        team_scores[team] = tempScore;
        //Debug.Log($"After {team_scores[team]}");
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

    private void TryClaimTileWithTilePos(Vector3Int tilePos, UnitController.Team newTeam)
    {
        // Check if tile actually exists in the board
        TileBase previousGroundTile = GroundTileMap().GetTile<TileBase>(tilePos);
        if (previousGroundTile == null)
        {
            return;
        }

        // If tile is already claimed by newTeam, then there's nothing to be done
        UnitController.Team previousTeam = GetMappingFromTileBaseToUnitControllerTeam(previousGroundTile);

        if (newTeam == previousTeam)
        {
            return;
        }

        // Claim the tile
        GroundTileMap().SetTile(tilePos, groundTiles[(int)newTeam]);

        // If the tile has a resource value we need to update score per round
        int resourceValInTile = GetResourceValueAtPos(tilePos);

        if (resourceValInTile > 0)
        {
            // Add it to the new team
            AddResourceToScorePerRound(resourceValInTile, newTeam);

            // Remove it from the previous team
            if ((previousTeam == UnitController.Team.hyena) || (previousTeam == UnitController.Team.cats))
            {
                AddResourceToScorePerRound(-resourceValInTile, previousTeam); //negative amount
            }

            // If the tile is within range of any towers, we add/remove a second time as needed
            foreach (TowerBehavior tower in towerData.Values)
            {
                if (tower.AreaIncludesCellAtPosition(tilePos))
                {
                    if (newTeam == UnitController.Team.cats)
                    {
                        AddResourceToScorePerRound(resourceValInTile, UnitController.Team.cats);
                    }
                    else if (newTeam == UnitController.Team.hyena && previousTeam == UnitController.Team.cats)
                    {
                        AddResourceToScorePerRound(-resourceValInTile, UnitController.Team.cats); //negative amount
                    }
                }
            }
        }
        
    }

    private UnitController.Team GetMappingFromTileBaseToUnitControllerTeam(TileBase tile)
    {
        if (!dataFromTiles.TryGetValue(tile, out TileTerritoryData tileTerritoryData))
        {
            Debug.LogError("Assuming neutral ownership because found no mapping for tile : " + tile);
            return UnitController.Team.neutral;
        }

        if (tileTerritoryData.type == TileTerritoryData.Type.Cats)
        {
            return UnitController.Team.cats;
        }
        else if (tileTerritoryData.type == TileTerritoryData.Type.Hyena)
        {
            return UnitController.Team.hyena;
        }
        else
        {
            return UnitController.Team.neutral;
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

    public void ClaimAreaAroundTower(Vector3Int towerPos, TowerBehavior tower)
    {
        foreach (Vector3Int cell in tower.getCellsInTowerArea())
        {
            TryClaimTileWithTilePos(cell, UnitController.Team.cats);
        }
    }

    public bool PlaceTower(Vector3Int towerTilePos)
    {
        if (towerData.ContainsKey(towerTilePos))
        {
            return false;
        }

        if (team_scores[UnitController.Team.cats].x < towerPlacementResourceCost)
        {
            return false;
        }

        TileBase tile = GroundTileMap().GetTile<TileBase>(towerTilePos);
        if (tile == null)
        {
            return false;
        }

        // Update tilemap
        TowerTilemap().SetTile(towerTilePos, starterTowerTile);
        TowerBehavior towerInfo = new TowerBehavior(towerTilePos, 2, false);
        towerData.Add(towerTilePos, towerInfo);

        // Spend the needed resources
        Vector2Int catScore = team_scores[UnitController.Team.cats];
        catScore.x -= towerPlacementResourceCost;
        team_scores[UnitController.Team.cats] = catScore;

        // First check if any of the cells around the tower were already claimed by Cats
        // If so, we update the resource collection value
        foreach(Vector3Int cell in towerInfo.getCellsInTowerArea())
        {
            TileBase groundTile = GroundTileMap().GetTile<TileBase>(cell);
            if (groundTile != null)
            {
                if (GetMappingFromTileBaseToUnitControllerTeam(groundTile) == UnitController.Team.cats)
                {
                    int resourceValInTile = GetResourceValueAtPos(cell);
                    AddResourceToScorePerRound(resourceValInTile, UnitController.Team.cats);
                }
            }
        }

        // Second we claim the area around tower, as a bonus
        // This will also add to the resource collection value, but only for the newly-claimed tiles
        ClaimAreaAroundTower(towerTilePos, towerInfo);

        ClearMovement();
        return true;
    }

    public void CollectTeamResources(UnitController.Team team)
    {

        Vector2Int tempScore = team_scores[team];
        tempScore.x += tempScore.y;

        if (team == UnitController.Team.hyena && tempScore.x >= 80)
        {
            tempScore.x -= 80;
            hyenasSpawnManager.IncreaseSpawnRate();
        }

        team_scores[team] = tempScore;
    }

    private Tilemap TowerTilemap()
    {
        return tilemapArray[(int)MapType.tower];
    }

    private Tilemap GroundTileMap()
    {
        return tilemapArray[(int)MapType.ground];
    }

    private Tilemap ResourceTilemap()
    {
        return tilemapArray[(int)MapType.resource];
    }
}