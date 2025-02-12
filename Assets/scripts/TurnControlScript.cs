using System.Collections.Generic;
using System.Data;
using Microsoft.Unity.VisualStudio.Editor;
using TMPro;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

[System.Serializable]
public class TurnControlScript : MonoBehaviour
{
    public CinemachineCamera cam;

    //this is a new syntax for me, the , indicates the array is 2 dimensional
    //the 2 is for each player, assuming its two player, the 5 is for each unit controlled.

    public CatController[] cats;
    int currentCat = 0;

    [SerializeField] private TilemapManager tmManager;
    [SerializeField] public UIBridge uiBridge;

    [SerializeField] private Color buttonDisabledColor;
    [SerializeField] private Color buttonEnabledColor;
    [SerializeField] private Sprite[] clock_sprites;

    [SerializeField] public List<HyenaController> hyenas = new List<HyenaController>();
    int currentHyena = 0;

    bool finishedUpdatingTurn = false;

    bool committedToAnAction = false;

    bool hyenaTurn = false;
    bool endTurn = false;
    float turnChangeTime = 0.0f;

    private void OnValidate()
    {
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(uiBridge == null){
            Debug.Log("uiBridge wasn't set ub turn control script");
        }
    }

    public void SwitchActor(int whichCat){
        if(committedToAnAction){
            Debug.Log("cant switch actor, commited to action");
            return;
        }
        if(whichCat < 0 || whichCat >= cats.Length){
            Debug.Log("trying to swap to an invalid actor : " + whichCat);
        }
        else{
            currentCat = whichCat;

            CatController tempCat = cats[whichCat];
            //uiBridge.ChangeFaceSprite(tempUnit.face_sprite);

            Debug.Log("before and after cam pos - " + cam.transform.position + " : " + cats[currentCat].transform.position);

            //cam.ResolveLookAt(cats[currentCat].transform);
            cam.transform.position = new Vector3(cats[currentCat].transform.position.x, cats[currentCat].transform.position.y, cam.transform.position.z);

            if(tempCat.movedThisTurn){
                uiBridge.buttons[0].GetComponent<UnityEngine.UI.Image>().color = buttonDisabledColor;

            }
            else{
                uiBridge.buttons[0].GetComponent<UnityEngine.UI.Image>().color = buttonEnabledColor;
            }
            if(tempCat.placedTowerThisTurn){
                uiBridge.buttons[1].GetComponent<UnityEngine.UI.Image>().color = buttonDisabledColor;
            }
            else{
                uiBridge.buttons[1].GetComponent<UnityEngine.UI.Image>().color = buttonEnabledColor;
            }

        }
        
        
    }

    void SpawnHyenas(){
        //james has a function allegedly
    }

    void MoveOneHyena(){
        HyenaController controlledHyena = hyenas[currentHyena];

        Vector3Int mvPos = Vector3Int.zero; //most valuable position
        //check value for each cat with A*
        //check value for each tower with A*
        //multiply tower values by some amount wit hdistance
        //multiply cat values by some amount iwth distance
        //put the A* functions into tilemapmanager



        controlledHyena.MoveTo(tmManager.tilemapArray[(int)TilemapManager.MapType.ground].CellToWorld(mvPos));

        currentHyena++;
    }

    // Update is called once per frame
    void Update()
    {
        
        if(endTurn){
            turnChangeTime += Time.deltaTime;
            int clockSpriteIter = Mathf.FloorToInt(turnChangeTime * 14.0f);
            if(hyenaTurn){
                clockSpriteIter += 14;
                if(clockSpriteIter > 27){
                    clockSpriteIter = 0;
                }
            }
            uiBridge.clock.GetComponent<UnityEngine.UI.Image>().sprite = clock_sprites[clockSpriteIter];

            if(turnChangeTime >= 1.0f){
                turnChangeTime = 0.0f;
                endTurn = false;
                hyenaTurn = !hyenaTurn;
                if(!hyenaTurn){
                    uiBridge.buttons[0].GetComponent<UnityEngine.UI.Image>().color = buttonEnabledColor;
                    uiBridge.buttons[1].GetComponent<UnityEngine.UI.Image>().color = buttonEnabledColor;
                    for(int i = 0; i < cats.Length; i++){
                        uiBridge.rosterUISetup[i].moveImage.GetComponent<UnityEngine.UI.Image>().color = buttonEnabledColor;
                        uiBridge.rosterUISetup[i].towerImage.GetComponent<UnityEngine.UI.Image>().color = buttonEnabledColor;
                        cats[i].RefreshTurn();
                    }   
                }
            }
        }
        else if(hyenaTurn){
            SpawnHyenas();


            if(currentHyena >= hyenas.Count){
                endTurn = true;
            }
            else{
                Debug.Log("currentHyena : count - " + currentHyena + " : " + hyenas.Count);
                MoveOneHyena();
            }

        }
        else{

            if(committedToAnAction){
                if(!cats[currentCat].moving){
                    committedToAnAction = false;
                }
                else{
                    return;
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
                        Debug.Log("checking if movement possible : " + mouseWorldPos);
                        if(tmManager.CheckIfMovementPossible(mouseWorldPos)){
                            Debug.Log("movment was possible");
                            committedToAnAction = true;
                            catCont.MoveTo(mouseWorldPos);
                            catCont.DisableMovement();
                            uiBridge.rosterUISetup[currentCat].moveImage.GetComponent<UnityEngine.UI.Image>().color = buttonDisabledColor;
                            uiBridge.buttons[0].GetComponent<UnityEngine.UI.Image>().color = buttonDisabledColor;
                            //uiBridge.buttons[0]. = new Color(0.2f, 0.2f, 0.2f, 1.0f);
                        }
                        else{
                            Debug.Log("failed to move");
                        }
                    }
                    else if(catCont.showTowerPlacement){
                        catCont.PlaceTower(mouseTilePos);
                        //uiBridge.abilityButtons[0];
                        catCont.DisablePlaceTower();
                        uiBridge.rosterUISetup[currentCat].towerImage.GetComponent<UnityEngine.UI.Image>().color = buttonDisabledColor;
                        uiBridge.buttons[1].GetComponent<UnityEngine.UI.Image>().color = buttonDisabledColor;
                        
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
                Debug.Log("controlled player or actor is nuLL? currentPlayer : currentActor -  " + currentCat);
            }
        }
    }

    public void EndTurn(){
        if(!hyenaTurn){
            if(committedToAnAction){
                return;
            }
            foreach(CatController cat in cats){
                cat.TurnEnding();
            }
            endTurn = true;
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
            Debug.Log("showing movement path for player[" + currentCat + "]");
            cats[currentCat].CalculatePossibleDestinations();
        }
        
    }
    [SerializeField] public void ShowTowerPlacement(){
        Debug.Log("attempting showing tower placement for player[" + currentCat + "]");
        if(committedToAnAction || cats[currentCat].placedTowerThisTurn){
            return;
        }
        if(!cats[currentCat].moving){
            cats[currentCat].ShowTowerPlacement();
        }
    }
}
