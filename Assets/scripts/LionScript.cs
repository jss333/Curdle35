using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LionController : CatController
{
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void Start()
    {
        base.team = Team.cats;

        base.movementType = Movement.Type.Lion;

        base.canPlaceTower =  true;

        base.movability = 3;
        //base.movementType = Movement.Type.Free;
        base.abilityCount = 1;

        base.Start();
        base.abilityNames[0] = "Place Tower";
        PrecalculateMovementOffsets();
    } 

    void PrecalculateMovementOffsets(){
        movementOffsets = new Vector3Int[20];

        int currentIter = 0;
        Vector3Int placement = Vector3Int.zero;
        for(placement.x = -3; placement.x <= 3; placement.x++){
            if(placement.x == 0){
                continue;
            }
            movementOffsets[currentIter] = placement;
            currentIter++;
        }
        //currentIter should be 6 right now
        placement.x = 0;

        for(placement.y = -3; placement.y <= 3; placement.y++){
            if(placement.y == 0){
                continue;
            }
            movementOffsets[currentIter] = placement;
            currentIter++;
        }
        //currentIter should be 12 right now
        placement.x = -2;
        placement.y = -2;
        
        for(placement.x = -2; placement.x <= 2; placement.x++){
            if(placement.x == 0){
                continue;
            }
            placement.y = placement.x;
            movementOffsets[currentIter] = placement;
            currentIter++;
            placement.y = -placement.x;
            movementOffsets[currentIter] = placement;
            currentIter++;
        }
    }
}
