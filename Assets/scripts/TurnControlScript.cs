using System.Collections.Generic;
using System.Data;
using TMPro;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using System;

[System.Serializable]
public class TurnControlScript : MonoBehaviour
{
    public CinemachineCamera cam;

    //this is a new syntax for me, the , indicates the array is 2 dimensional
    //the 2 is for each player, assuming its two player, the 5 is for each unit controlled.

    public CatController[] cats;
    int currentCat = 0;

    [SerializeField] private TilemapManager tmManager;
    [SerializeField] private HyenasSpawnManager hyenasSpawnManager;
    [SerializeField] public UIBridge uiBridge;
    [SerializeField] private SoundManager soundManager;

    [SerializeField] private Sprite[] clock_sprites;

    [SerializeField] public List<HyenaController> hyenas = new List<HyenaController>();
    [SerializeField] public List<Vector3Int> minorTowers = new List<Vector3Int>();

    private Dictionary<GameObject, CatController> hyenaCatTargets = new Dictionary<GameObject, CatController>();
    private Dictionary<GameObject, Vector3Int> hyenaTowerTargets = new Dictionary<GameObject, Vector3Int>();

    int currentHyena = 0;

    [SerializeField] int winConditionScore = 350;

    bool committedToAnAction = false;
    float turnChangeTime = 0.0f;

    public GridManager gridManager;

    enum DayPhase : int{
        Dawn,
        Day,
        Dusk,
        Night,

    }
    [SerializeField] DayPhase dayPhase = DayPhase.Dawn;

    void WinConditionAchieved(){
        soundManager.StopAllMusic();
        soundManager.PlayMusic(SoundManager.Music.WinMusic);
    }
    void LossConditionReached(){
        soundManager.StopAllMusic();
        soundManager.PlayMusic(SoundManager.Music.LossMusic);
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gridManager = new GridManager();
        gridManager.tmManager = tmManager;
        gridManager.bounds = tmManager.tilemapArray[(int)TilemapManager.MapType.ground].cellBounds;
        gridManager.Initialize();

        uiBridge.towerPlacementText.text = "Place Tower (" + tmManager.towerPlacementResourceCost + ")";
    }

    void PerformCatAttack(){
        soundManager.PlayEffect(SoundManager.Effects.CatAttack);
        //cats[currentCat];
        //prefab here, Instantiate<ClawAttack>(transform, ClawPrefab);

        
    }
    void KillOffCat(CatController cat){
        soundManager.PlayEffect(SoundManager.Effects.CatDeath);
        cat.isAlive = false;

        bool allDead = true;
        for(int i = 0; i < cats.Length; i++){
            if(cats[i].isAlive){
                allDead = false;
                break;
            }
        }
        if(allDead){
            LossConditionReached();
        }
    }

    [SerializeField] public void SwitchActor(int whichCat){

        bool fromRoster = whichCat >= cats.Length;
        if(fromRoster){
            whichCat -= cats.Length;
        }
        if(whichCat != currentCat){
            soundManager.PlayEffect(SoundManager.Effects.UnitSelection);
            tmManager.ClearMovement();
        }

        if(committedToAnAction){
            //Debug.Log("cant switch actor, commited to action");
            return;
        }
        if(whichCat < 0 || whichCat >= cats.Length){
            //Debug.Log("trying to swap to an invalid actor : " + whichCat);
        }
        else{
            currentCat = whichCat;

            CatController tempCat = cats[whichCat];
            //uiBridge.ChangeFaceSprite(tempUnit.face_sprite);

            //Debug.Log("before and after cam pos - " + cam.transform.position + " : " + cats[currentCat].transform.position);

            //cam.ResolveLookAt(cats[currentCat].transform);
            if(fromRoster){
                cam.transform.position = new Vector3(cats[currentCat].transform.position.x, cats[currentCat].transform.position.y, cam.transform.position.z);
            }

            uiBridge.SetMoveButtonActivity(!tempCat.movedThisTurn);

            uiBridge.SetTowerPlacementButtonActivity((!tempCat.placedTowerThisTurn) && (tmManager.team_scores[UnitController.Team.cats].x >= tmManager.towerPlacementResourceCost));

            uiBridge.buttons[1].SetActive(cats[currentCat].canPlaceTower);
        }
        
        
    }

    void SpawnHyenas(){

        var returnedHyenas = hyenasSpawnManager.SpawnHyenasAtSpawnPoints();
        if(returnedHyenas.Count > 0){
            soundManager.PlayEffect(SoundManager.Effects.HyenaSpawn);
        }
        foreach(HyenaController hy in returnedHyenas){
            tmManager.TryClaimTile(hy.transform.position, hy.team);
            hyenas.Add(hy);
            
        }
        returnedHyenas.Clear();
    }

    void UpdateOneHyena(){
        HyenaController controlledHyena = hyenas[currentHyena];


        if(controlledHyena.movedThisTurn){
            currentHyena++;
            return;
        }
        if(controlledHyena.attackedTarget){
            soundManager.PlayEffect(SoundManager.Effects.HyenaAttack);

            if(hyenaCatTargets.TryGetValue(controlledHyena.gameObject, out var cat)){
                cat.health--;
                if(cat.health <= 0){
                    KillOffCat(cat);
                }
                //else{
                    hyenaCatTargets.Remove(controlledHyena.gameObject);
                    hyenas.Remove(controlledHyena);
                    Destroy(controlledHyena.gameObject);
                //}
            }
            else if (hyenaTowerTargets.TryGetValue(controlledHyena.gameObject, out var tower)){
                hyenaCatTargets.Remove(controlledHyena.gameObject);
                hyenas.Remove(controlledHyena);
                if(tower == tmManager.hqTowerPosition){

                }
            }
            currentHyena++;
            return;
        }
        if(!controlledHyena.moving && !controlledHyena.attacking){
            //Debug.Log("hyena was not moving");
            for(int i = 0; i < cats.Length; i++) {
                Vector3 distanceToCat = cats[i].transform.position - controlledHyena.transform.position;
                if((Mathf.Abs(distanceToCat.x) + Mathf.Abs(distanceToCat.y)) <= 3.0){
                    Debug.Log("hyena was close to cat - " + distanceToCat);
                    distanceToCat.Normalize();
                    if(Mathf.Abs(distanceToCat.x) < Mathf.Abs(distanceToCat.y)){
                        distanceToCat.x = 0.0f;
                        if(distanceToCat.y < 0.0f){
                            distanceToCat.y = -1.0f;
                            
                        }
                        else{
                            distanceToCat.y = 1.0f;
                        }
                    }
                    else{
                        distanceToCat.y = 0.0f;
                        if(distanceToCat.x < 0.0f){
                            distanceToCat.x = -1.0f;
                            
                        }
                        else{
                            distanceToCat.x = 1.0f;
                        }
                        controlledHyena.attacking = true;
                        hyenaCatTargets.Add(controlledHyena.gameObject, cats[i]);
                        controlledHyena.attackPosition = cats[i].transform.position - distanceToCat;
                        return;
                    }
                }
            }

            Vector3Int tempPos = tmManager.tilemapArray[(int)TilemapManager.MapType.ground].WorldToCell(controlledHyena.transform.position);

            controlledHyena.movementPath = gridManager.DetermineOptimalPath(tempPos, 2);
            if(controlledHyena.movementPath != null){
                soundManager.PlayEffect(SoundManager.Effects.HyenaMovement);
                //Debug.Log("current hyena - first point and position and count - " + currentHyena + " : " + controlledHyena.movementPath[0] + " : " + controlledHyena.transform.position  + ":" + controlledHyena.movementPath.Count);
                
                /* //i was trying to rework hyena diagonal movement but i couldnt get it right, need to move on
                    for(int i = 0; i < controlledHyena.movementPath.Count; i++){
                        Debug.Log("full movement list - " + currentHyena + " : " + i + " : " + controlledHyena.movementPath[i]);
                    }
                    //Debug.Log("finna move hyena, OG pos : path length - " + controlledHyena.transform.position + controlledHyena.movementPath.Count);
                    if((controlledHyena.movementPath.Count >= 2) && ((controlledHyena.movementPath[0] - controlledHyena.transform.position).magnitude > 1.1f)){
                        Debug.Log("removing index 1 because 0 was diagonal");
                        controlledHyena.movementPath.RemoveAt(1);
                        for(int i = 0; i < controlledHyena.movementPath.Count; i++){
                            Debug.Log("post trimming0 movement list - " + currentHyena + " : " + i + " : " + controlledHyena.movementPath[i]);
                        }
                    }
                    else if((controlledHyena.movementPath.Count >= 2) && ((controlledHyena.movementPath[1] - controlledHyena.movementPath[0]).magnitude > 1.1f)){
                        controlledHyena.movementPath.RemoveAt(1);
                        Debug.Log("removing index 1 because 0 was diagonal");
                        for(int i = 0; i < controlledHyena.movementPath.Count; i++){
                            Debug.Log("post trimming1 movement list - " + currentHyena + " : " + i + " : " + controlledHyena.movementPath[i]);
                        }
                    }
                */

                controlledHyena.moving = true;
            }
            else{
                controlledHyena.movedThisTurn = true;
            }
        }
        else{
            //Debug.Log("hyena was moving");
        }
        

    }
    void UpdateUI(){
        int[] healths = new int[]{ //theres probably ab etter way to do this
            cats[0].health,
            cats[1].health,
            cats[2].health
        };
        if(!tmManager.team_scores.TryGetValue(UnitController.Team.cats, out Vector2Int catScore)){
            Debug.Log("failed to find cat score");
        }
        if(!tmManager.team_scores.TryGetValue(UnitController.Team.hyena, out Vector2Int hyenaScore)){
            Debug.Log("failed to find hyena score");
        }
        uiBridge.UpdateUI(healths, catScore, hyenaScore);

        if(catScore.x > winConditionScore){
            WinConditionAchieved();
            //do win condition
        }
    }
    
    void ChangeDayPhase(DayPhase phase){
        tmManager.ClearMovement();
        switch(phase){
            case DayPhase.Dawn:{
                soundManager.StopMusic(SoundManager.Music.NightBegin);
                soundManager.PlayMusic(SoundManager.Music.DawnBegin);
                break;
            }
            case DayPhase.Day:{
                soundManager.PlayDayMusic();
                break;
            }
            case DayPhase.Dusk:{
                soundManager.StopMusic(SoundManager.Music.DayBegin);
                soundManager.PlayMusic(SoundManager.Music.DuskBegin);
                break;
            }
            case DayPhase.Night:{
                soundManager.PlayNightMusic();
                break;
            }
        }
        dayPhase = phase;
    }
    
    // Update is called once per frame
    void Update()
    {
        //Debug.Log("Mouse pos in world space : " + Camera.main.ScreenToWorldPoint(Input.mousePosition));
        //if(Input.GetMouseButtonDown(0)){
            //Debug.Log("mouse pos in tile space : " + tmManager.tilemapArray[(int)TilemapManager.MapType.ground].WorldToCell(Camera.main.ScreenToWorldPoint(Input.mousePosition)));
        //}

        UpdateUI();

        switch(dayPhase){
            case DayPhase.Dawn:{
                //Debug.Log("dawn");
                turnChangeTime += Time.deltaTime;
                int clockSpriteIter = Mathf.FloorToInt(turnChangeTime * 14.0f) + 14;
                if(clockSpriteIter > 27){
                    Debug.Log("dawn turn change time should never be over 14 : " + clockSpriteIter);
                    clockSpriteIter = 0;
                }
                uiBridge.clock.GetComponent<UnityEngine.UI.Image>().sprite = clock_sprites[clockSpriteIter];
            
                
                if(turnChangeTime >= 1.0f){
                    turnChangeTime = 0.0f;

                    tmManager.CollectResourcesFromTowers();
                    //Create Spawn Markers
                    hyenasSpawnManager.GenerateNewSpawnPointsBasedOnSpawnRates();

                    uiBridge.SetMoveButtonActivity(true);
                    uiBridge.SetTowerPlacementButtonActivity(tmManager.team_scores[UnitController.Team.cats].x >= tmManager.towerPlacementResourceCost);
                    for(int i = 0; i < cats.Length; i++){
                        cats[i].RefreshTurn();
                    }
                    tmManager.CollectTeamResources(UnitController.Team.hyena);
                    soundManager.PlayEffect(SoundManager.Effects.HyenaResourceCollection);
                    Debug.Log("changing day phase to day");
                    ChangeDayPhase(DayPhase.Day);
                }
                

                break;
            }
            case DayPhase.Day:{
                //Debug.Log("day");
                if(committedToAnAction){
                    if(!cats[currentCat].moving){
                        committedToAnAction = false;
                    }
                    else{
                        Vector3Int catTilePos = tmManager.tilemapArray[(int)TilemapManager.MapType.ground].WorldToCell(cats[currentCat].transform.position);

                        hyenas.RemoveAll(hyena =>
                            {
                                Vector3Int hyenaPos = tmManager.tilemapArray[(int)TilemapManager.MapType.ground].WorldToCell(hyena.transform.position);
                                if(catTilePos == hyenaPos){
                                    Debug.Log("ran over hyena");
                                    
                                    soundManager.PlayEffect(SoundManager.Effects.CatAttack);
                                    Destroy(hyena.gameObject);

                                    PerformCatAttack();

                                    return true;
                                }
                                return false;
                            }
                        );

                        for(int i = 0; i < hyenas.Count; i++){
                            Vector3Int hyenaPos = tmManager.tilemapArray[(int)TilemapManager.MapType.ground].WorldToCell(hyenas[i].transform.position);
                            if(catTilePos == hyenaPos){
                                Debug.Log("ran over hyena");
                                Destroy(hyenas[i]);
                                hyenas.RemoveAt(i);
                                i--;

                                PerformCatAttack();
                            }
                        }
                        break;
                    }
                }
                
                bool lmbDown = Input.GetMouseButtonDown(0);
                bool rmbDown = Input.GetMouseButtonDown(1);
                Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mouseWorldPos.z = 0.0f;
                Vector3Int mouseTilePos = tmManager.tilemapArray[(int)TilemapManager.MapType.ground].WorldToCell(mouseWorldPos);

                CatController catCont = null;
                if(currentCat >= 0 && currentCat < cats.Length){
                    catCont = cats[currentCat];
                    
                }
                if(catCont != null){
                    if(lmbDown) {
                        if(catCont.showMovement) {
                            //Debug.Log("checking if movement possible : " + mouseWorldPos);
                            if(tmManager.CheckIfMovementPossible(mouseWorldPos)){
                                if(currentCat == 0){
                                    soundManager.PlayEffect(SoundManager.Effects.LionMovement);
                                }
                                else{
                                    soundManager.PlayEffect(SoundManager.Effects.JaguarMovement);
                                }
                                //Debug.Log("movment was possible");
                                committedToAnAction = true;
                                catCont.MoveTo(mouseWorldPos);
                                catCont.DisableMovement();
                                uiBridge.SetMoveButtonActivity(false);
                            }
                            else{
                                //Debug.Log("failed to move");
                            }
                        }
                        else if(catCont.showTowerPlacement){
                            if(tmManager.PlaceTower(mouseTilePos)){
                                soundManager.PlayEffect(SoundManager.Effects.LionTower);
                            }

                            catCont.showTowerPlacement = false;
                            minorTowers.Add(mouseTilePos);
                            //uiBridge.abilityButtons[0];
                            catCont.DisablePlaceTower();
                            uiBridge.SetTowerPlacementButtonActivity(false);
                            
                        }
                        else {
                            //Debug.Log("mouse button down while controlled actor is not showing movement");
                            //try to select a creature
                            for(int i = 0; i < cats.Length; i++){
                                Bounds spriteBounds = cats[i].GetComponent<SpriteRenderer>().bounds;
                                //Debug.Log("sprite bounds : mouse pos - " + spriteBounds + " - " + mouseWorldPos);
                                if(spriteBounds.Contains(mouseWorldPos)){
                                    Debug.Log("selected new actor : " + i);
                                    SwitchActor(i);
                                    break;
                                }
                            }
                        }
                    }
                    else if(rmbDown){
                        cats[currentCat].showMovement = false;
                        cats[currentCat].showTowerPlacement = false;
                        tmManager.ClearMovement();
                    }
                }
                else{
                    //Debug.Log("controlled player or actor is nuLL? currentCat -  " + currentCat);
                }
                break;
            }
            case DayPhase.Dusk:{
                //Debug.Log("dusk");
                turnChangeTime += Time.deltaTime;
                int clockSpriteIter = Mathf.FloorToInt(turnChangeTime * 14.0f);
                if(clockSpriteIter > 27){
                    clockSpriteIter = 0;
                }
                uiBridge.clock.GetComponent<UnityEngine.UI.Image>().sprite = clock_sprites[clockSpriteIter];
            

                
                if(turnChangeTime >= 1.0f){
                    turnChangeTime = 0.0f;
                    SpawnHyenas();  
                    foreach(var hyena in hyenas){
                        hyena.movedThisTurn = false;
                    }
                    Debug.Log("changing day phase to night");
                    ChangeDayPhase(DayPhase.Night);
                    tmManager.CollectTeamResources(UnitController.Team.cats);
                    soundManager.PlayEffect(SoundManager.Effects.CatResourceCollection);
                }
                break;
            }
            case DayPhase.Night:{
                
                //Debug.Log("night");
            
                    //currentHyena = hyenas.Count; //this turns off movement for debuggign
                    if(currentHyena >= hyenas.Count){
                        //Debug.Log("all hyenas moved - " + currentHyena + " : " + hyenas.Count);
                        Debug.Log("changing day phase to dawn");
                        ChangeDayPhase(DayPhase.Dawn);
                        currentHyena = 0;
                    }
                    else{
                        //Debug.Log("currentHyena : count - " + currentHyena + " : " + hyenas.Count);
                        UpdateOneHyena();
                    }
                break;
            }
            default:{
                Debug.Log("invalid day phase : " + dayPhase);
                break;
            }
        }
    }

    public void EndTurn(){
        if(dayPhase == DayPhase.Day){
            if(committedToAnAction){
                return;
            }
            foreach(CatController cat in cats){
                cat.TurnEnding();
            }
            //Debug.Log("changing day phase to dusk");
            ChangeDayPhase(DayPhase.Dusk);
        }
    }

    [SerializeField] public void ShowMovementOnControlledActor(){
        if(committedToAnAction){
            return;
        }

        if(currentCat >= 0 && currentCat < cats.Length){
            if(cats[currentCat].movedThisTurn){
                return;
            }
            //Debug.Log("showing movement path for player[" + currentCat + "]");
            cats[currentCat].CalculatePossibleDestinations();
        }
        
    }
    [SerializeField] public void ShowTowerPlacement(){
        if(cats[currentCat].canPlaceTower){
            //Debug.Log("attempting showing tower placement for player[" + currentCat + "]");
            if(committedToAnAction || cats[currentCat].placedTowerThisTurn){
                return;
            }
            if(!cats[currentCat].moving){
                cats[currentCat].ShowTowerPlacement();
            }
        }
    }
}
