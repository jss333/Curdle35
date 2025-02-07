using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class UnitController : MonoBehaviour
{
    Vector3 destinationPos;
    public Movement.Type movementType;
    public bool moving = false;
    [SerializeField] const float speed = 1.0f;

    public bool showMovement = false;
    

    public int startingAP = 10;
    public int remainingAP = 10;

    public int movability = 3;

    public TilemapManager tmManager;

    public int abilityCount = 0; //set this from derived class

    public int showingAbility = -1;

    public string[] abilityNames;

    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public virtual void Start()
    {
        tmManager.SetOccupancy(transform.position);
        if(abilityCount > 0){
            abilityNames = new string[abilityCount];
        }
        else{
            abilityNames = null;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(moving){
            Vector3 direction = destinationPos - transform.position;
            float distance = direction.magnitude;
            if(distance < (speed * Time.deltaTime)){
                transform.position = destinationPos;
                moving = false;
                tmManager.SetOccupancy(transform.position);
            }
            else{
                direction /= distance;
                transform.position += direction * Time.deltaTime;
            }
        }
    }

    public void MoveTo(Vector3 dest){
        tmManager.RemoveOccupancy(transform.position);
        tmManager.ClearMovement();
        destinationPos.x = Mathf.Floor(dest.x) + 0.5f;
        destinationPos.y = Mathf.Floor(dest.y) + 0.5f;
        destinationPos.z = transform.position.z;
        moving = true;
        showMovement = false;
    }
    
    public void CalculatePossibleDestinations(){
        showMovement = true;

        Vector3Int myTilePos = tmManager.tilemap.WorldToCell(transform.position);
        tmManager.ClearMovement();
        switch(movementType){
            case Movement.Type.Straight:{
                    //need to do both y and X, like a cross
                    {
                        Vector3Int horizontalPath = myTilePos;
                        int rightBound = horizontalPath.x + remainingAP;
                        horizontalPath.x -= remainingAP;
                        for(; horizontalPath.x < rightBound; horizontalPath.x++){
                            tmManager.ActivateMovementTile(horizontalPath);
                        }
                    }
                    {
                        Vector3Int verticalPath = myTilePos;
                        int highBound = verticalPath.y + remainingAP;
                        verticalPath.y -= remainingAP;
                        for(; verticalPath.y < highBound; verticalPath.y++){
                            tmManager.ActivateMovementTile(verticalPath);
                        }
                    }
                break;
            }
            case Movement.Type.Free:{
                //negative offsets
                Vector3Int offsets = Vector3Int.zero;
                offsets.x = -movability;
                offsets.y = -movability;
                offsets.z = 0;
                for(;offsets.x <= movability; offsets.x++){
                    offsets.y = -movability;
                    for(;offsets.y <= movability; offsets.y++){
                        //Debug.Log("offset iter : " + offsets);
                        if(((Math.Abs(offsets.x) + Math.Abs(offsets.y)) <= movability) && !(offsets.x == 0 && offsets.y == 0)){
                            //Debug.Log("activating tile : " + offsets);
                            tmManager.ActivateMovementTile(myTilePos + offsets);
                        }
                    }
                }


                break;
            }
        }
    }
    public virtual bool TurnControlUpdate(){
        return remainingAP == 0;
    }

    //id do an array of function pointers here in C++, just gonna handtype it for now. im assuming the max ability count is 3
    public virtual void ShowRangeForAbility0(){
        Debug.Log("attempting to show range for ability[" + 0 + "] that hasn't been overridden");
    }
    public virtual void PerformAbility0(Vector3Int mouseTilePos){
        showingAbility = -1;
        if(abilityCount < 1){
            Debug.Log("attempting to use ability[" + 0 + "] that hasn't been overridden");
        }
    }    
    public virtual void ShowRangeForAbility1(){
        if(abilityCount < 2){
            Debug.Log("attempting to show range for ability[" + 1 + "] that hasn't been overridden");
        }

    }
    public virtual void PerformAbility1(Vector3Int mouseTilePos){
        showingAbility = -1;
        if(abilityCount < 2){
            Debug.Log("attempting to use ability[" + 1 + "] that hasn't been overridden");
        }
    }
    public virtual void ShowRangeForAbility2(){
        if(abilityCount < 3){
            Debug.Log("attempting to show range for ability[" + 2 + "] that hasn't been overridden");
        }

    }
    public virtual void PerformAbility2(Vector3Int mouseTilePos){
        showingAbility = -1;
        if(abilityCount < 3){
            Debug.Log("attempting to use ability[" + 2 + "] that hasn't been overridden");
        }

    }
}
