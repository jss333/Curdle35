using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

[System.Serializable]
public class UnitActors
{
    public UnitController[] actors;
}
public class TurnControlScript : MonoBehaviour
{
    //this is a new syntax for me, the , indicates the array is 2 dimensional
    //the 2 is for each player, assuming its two player, the 5 is for each unit controlled.
    public UnitActors[] players;
    int currentActor = 0;
    int currentPlayer = 0;

    [SerializeField] private int playerCount = 2;
    [SerializeField] private int actorCount = 5; 
    [SerializeField] private TilemapManager tmManager;
    [SerializeField] public UIBridge uiBridge;

    bool finishedUpdatingTurn = false;

    private void OnValidate()
    {
        // Ensure grid is initialized
        if (players == null || players.Length != playerCount)
        {
            players = new UnitActors[playerCount];
        }

        for (int i = 0; i < playerCount; i++)
        {
            if (players[i] == null)
            {
                players[i] = new UnitActors();
            }

            if (players[i].actors == null || players[i].actors.Length != actorCount)
            {
                players[i].actors = new UnitController[actorCount];
            }
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (players == null || players.Length != playerCount)
        {
            players = new UnitActors[playerCount];
        }

        for (int i = 0; i < playerCount; i++)
        {
            if (players[i] == null)
            {
                Debug.Log("player " + i + " was invalid");
            }

            if (players[i].actors == null || players[i].actors.Length != actorCount)
            {
                Debug.Log("player " + i + " has invalid actors");
            }
        }
        if(uiBridge == null){
            Debug.Log("uiBridge wasn't set ub turn control script");
        }
    }

    void SwitchActor(int whichPlayer, int whichActor){
        if(currentPlayer < 0 || currentPlayer > players.Length){
            Debug.Log("trying to swap to an invalid player : " + whichPlayer);
        }
        else{
            if(whichActor < 0 || whichActor >= players[currentPlayer].actors.Length){
                Debug.Log("trying to swap to an invalid actor : " + whichActor);
            }
            else{
                currentPlayer = whichPlayer;
                currentActor = whichActor;

                UnitController tempUnit = players[currentPlayer].actors[currentActor];

                //if i could do a god damn array of objects in the unity editor i wouldnt have to fuck with this dog shit layout
                //fucking unity
                if(tempUnit.abilityCount >= 1){
                    uiBridge.ability0_button.SetActive(true);
                    TextMeshProUGUI tmpGUI0 = uiBridge.ability1_button.GetComponentInChildren<TextMeshProUGUI>();
                    tmpGUI0.text = tempUnit.abilityNames[0];
                    
                    if(tempUnit.abilityCount >= 2){
                        uiBridge.ability1_button.SetActive(true);
                        TextMeshProUGUI tmpGUI1 = uiBridge.ability1_button.GetComponentInChildren<TextMeshProUGUI>();
                        tmpGUI1.text = tempUnit.abilityNames[1];

                        
                        if(tempUnit.abilityCount >= 3){
                            uiBridge.ability2_button.SetActive(true);
                            TextMeshProUGUI tmpGUI2 = uiBridge.ability2_button.GetComponentInChildren<TextMeshProUGUI>();
                            tmpGUI2.text = tempUnit.abilityNames[2];
                        }
                        else{
                            uiBridge.ability2_button.SetActive(false);
                        }
                    }
                    else{
                        uiBridge.ability1_button.SetActive(false);
                        uiBridge.ability2_button.SetActive(false);
                    }
                }
                else{
                    uiBridge.ability0_button.SetActive(false);
                    uiBridge.ability1_button.SetActive(false);
                    uiBridge.ability2_button.SetActive(false);
                }
                
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
        bool lmbDown = Input.GetMouseButtonDown(0);
        bool rmbDown = Input.GetMouseButtonDown(1);
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0.0f;
        Vector3Int mouseTilePos = tmManager.tilemap.WorldToCell(mouseWorldPos);

        UnitController unitCont = null;
        if(currentPlayer >= 0 && currentPlayer < players.Length){
            if(currentActor >= 0 && currentActor < players[currentPlayer].actors.Length){
                unitCont = players[currentPlayer].actors[currentActor];
            }
        }
        if(unitCont != null){
            if(lmbDown) {
                if(unitCont.showMovement) {
                    Debug.Log("checking if movement possible : " + mouseWorldPos);
                    if(tmManager.CheckIfMovementPossible(mouseWorldPos)){
                        Debug.Log("movment was possible");
                        unitCont.MoveTo(mouseWorldPos);
                    }
                    else{
                        Debug.Log("failed to move");
                    }
                }
                else if(unitCont.showingAbility >= 0){
                    switch(unitCont.showingAbility){
                        case 0:{
                            unitCont.PerformAbility0(mouseTilePos);
                            break;
                        }
                        case 1:{
                            
                            unitCont.PerformAbility1(mouseTilePos);
                            break;
                        }
                        case 2:{

                            unitCont.PerformAbility1(mouseTilePos);
                            break;
                        }
                    }
                }
                else {
                    //Debug.Log("mouse button down while controlled actor is not showing movement");
                    //try to select a creature
                    for(int i = 0; i < players[currentPlayer].actors.Length; i++){
                        Bounds spriteBounds = players[currentPlayer].actors[i].GetComponent<SpriteRenderer>().bounds;
                        //Debug.Log("sprite bounds : mouse pos - " + spriteBounds + " - " + mouseWorldPos);
                        if(spriteBounds.Contains(mouseWorldPos)){
                            Debug.Log("selected new actor : " + i);
                            SwitchActor(currentPlayer, i);
                            break;
                        }
                    }
                }
            }
            else if(rmbDown){
                players[currentPlayer].actors[currentActor].showMovement = false;
                tmManager.ClearMovement();
            }
        }
        else{
            Debug.Log("controlled player or actor is nuLL? currentPlayer : currentActor -  " + currentPlayer + " : " + currentActor);
        }
    }

    void TurnPreparationUpdate(){
        if(players[currentPlayer].actors[currentActor].TurnControlUpdate()){
            currentActor++;
            if(currentActor >= 5){
                currentPlayer++;
                currentActor = 0;
                if(currentPlayer >= 2){
                    currentPlayer = 0;
                    finishedUpdatingTurn = true;
                }
            }
        }
    }

    [SerializeField] public void ShowMovementOnControlledActor(){
        if(currentPlayer >= 0 && currentPlayer < players.Length){
            if(currentActor >= 0 && currentActor < players[currentPlayer].actors.Length){
                Debug.Log("showing movement path for player[" + currentPlayer + "] and actor [" + currentActor + "]");
                players[currentPlayer].actors[currentActor].CalculatePossibleDestinations();
            }
        }
    }


    //this is when the turn finishes, all actions will be performed
    void FinishTurn(){
        
    }
}
