using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LionController : UnitController
{
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        base.movability = 3;
        base.movementType = Movement.Type.Free;
        base.abilityCount = 1;
    }

    //id do an array of function pointers here in C++, just gonna handtype it for now. im assuming the max ability count is 3
    public void ShowRangeForAbility0(){

        tmManager.ClearMovement();
        Vector3Int myTilePos = tilemap.WorldToCell(transform.position);
        Vector3Int offset = Vector3Int.zero;
        offset.x = -1;
        tmManager.ActivateMovementTile(myTilePos + offset);
        offset.x = 1;
        tmManager.ActivateMovementTile(myTilePos + offset);
        offset.x = 0;
        offset.y = -1;
        tmManager.ActivateMovementTile(myTilePos + offset);
        offset.y = 1;
        tmManager.ActivateMovementTile(myTilePos + offset);

        showingAbility = 0;
    }
    public virtual void PerformAbility0(Vector3Int mouseTilePos){

        showingAbility = -1;
    }
}
