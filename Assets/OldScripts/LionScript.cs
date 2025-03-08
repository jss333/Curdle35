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
        base.health = 3;

        base.Start();
        PrecalculateMovementOffsets();
    } 

    void PrecalculateMovementOffsets(){

    }
}
