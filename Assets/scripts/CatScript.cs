using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public class CatController : UnitController
{
    
    public bool placedTowerThisTurn = false;

    public bool isAlive = true;

    public bool canPlaceTower;
    
    public int health = 1;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void Start() 
    {
        base.team = Team.cats;

        base.Start();
    }

    public void RefreshTurn(){
        movedThisTurn = false;
        placedTowerThisTurn = false;
        //moveButton.moveButton = enabled;
    }

    public void TurnEnding(){
        DisableMovement();
        DisablePlaceTower();
    }

    public void DisableMovement(){
        movedThisTurn = true;
        //disable movebutton, probably tint and unclickable
    }
    public void DisablePlaceTower(){
        placedTowerThisTurn = true;
        movedThisTurn = true;
    }
}
