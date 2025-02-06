using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

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


    //this is when the turn finishes, all actions will be performed
    void FinishTurn(){
        
    }
}
