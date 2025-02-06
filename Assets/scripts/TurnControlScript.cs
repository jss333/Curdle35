using System.Collections.Generic;
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
    [SerializeField] private TilemapClick tmClick;

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
                players[i] = new UnitActors();
            }

            if (players[i].actors == null || players[i].actors.Length != actorCount)
            {
                players[i].actors = new UnitController[actorCount];
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
        
        if(players[currentPlayer].actors[currentActor].showMovement){
            if(lmbDown){

                Debug.Log("checking if movement possible : " + mouseWorldPos);
                if(tmClick.CheckIfMovementPossible(mouseWorldPos)){
                    Debug.Log("movment was possible");
                    players[currentPlayer].actors[currentActor].MoveTo(mouseWorldPos);
                }
                else{
                    Debug.Log("failed to move");
                }
            }
            if(rmbDown){
                players[currentPlayer].actors[currentActor].showMovement = false;
                tmClick.ClearMovement();
            }

        }
        else{
            if(lmbDown){
                //Debug.Log("mouse button down while controlled actor is not showing movement");
                //try to select a creature
                for(int i = 0; i < players[currentPlayer].actors.Length; i++){
                    Bounds spriteBounds = players[currentPlayer].actors[i].GetComponent<SpriteRenderer>().bounds;
                    //Debug.Log("sprite bounds : mouse pos - " + spriteBounds + " - " + mouseWorldPos);
                    if(spriteBounds.Contains(mouseWorldPos)){
                        Debug.Log("selected new actor : " + i);
                        currentActor = i;
                        break;
                    }
                }
            }
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
