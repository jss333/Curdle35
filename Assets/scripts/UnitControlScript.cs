using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class UnitController : MonoBehaviour
{
    public enum Team{
        neutral,
        cats,
        hyena,

        TEAM_COUNT,
    }

    Vector3 destinationPos;
    public Movement.Type movementType;
    public bool moving = false;
    [SerializeField] public float speed = 1.0f;

    public bool showMovement = false;
    public Vector3Int[] movementOffsets;
    

    public int startingAP = 10;
    public int remainingAP = 10;

    public int movability = 3;

    public TilemapManager tmManager;

    public int abilityCount = 0; //set this from derived class

    public bool showTowerPlacement = false;
    public int showingAbility = -1;


    public bool alreadyMoved = false;
    public string[] abilityNames;

    public Team team = Team.hyena;

    public Sprite face_sprite;

    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public virtual void Start()
    {
        tmManager.SetOccupancy(transform.position, team);
        if(abilityCount > 0){
            abilityNames = new string[abilityCount];
        }
        else{
            abilityNames = null;
        }
        Vector3Int myTilePos = tmManager.tilemapArray[(int)TilemapManager.MapType.ground].WorldToCell(transform.position);
        tmManager.tilemapArray[(int)TilemapManager.MapType.ground].SetTile(myTilePos, tmManager.groundTiles[(int)team]);
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
                Debug.Log("finished moving");
                tmManager.SetOccupancy(transform.position, team);
            }
            else{
                direction /= distance;
                transform.position += speed * direction * Time.deltaTime;
            }
            Vector3Int myTilePos = tmManager.tilemapArray[(int)TilemapManager.MapType.ground].WorldToCell(transform.position);
            tmManager.tilemapArray[(int)TilemapManager.MapType.ground].SetTile(myTilePos, tmManager.groundTiles[(int)team]);
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
        Debug.Log("showing default movement");
        showMovement = true;

        Vector3Int myTilePos = tmManager.tilemapArray[(int)TilemapManager.MapType.ground].WorldToCell(transform.position);
        tmManager.ClearMovement();
        switch(movementType){
            case Movement.Type.Straight:{
                    //need to do both y and X, like a cross
                    {
                        Vector3Int horizontalPath = myTilePos;
                        int rightBound = horizontalPath.x + remainingAP;
                        horizontalPath.x -= remainingAP;
                        for(; horizontalPath.x < rightBound; horizontalPath.x++){
                            tmManager.ActivateMovementTile(horizontalPath, team);
                        }
                    }
                    {
                        Vector3Int verticalPath = myTilePos;
                        int highBound = verticalPath.y + remainingAP;
                        verticalPath.y -= remainingAP;
                        for(; verticalPath.y < highBound; verticalPath.y++){
                            tmManager.ActivateMovementTile(verticalPath, team);
                        }
                    }
                break;
            }
            case Movement.Type.Lion:{ //placeholder type, remove types later
                
                foreach(Vector3Int offset in movementOffsets){
                    Debug.Log("offset in movement offsets : " + offset);
                    tmManager.ActivateMovementTile(offset + myTilePos, team);
                }
                break;
            }
            case Movement.Type.Jaguar:{
                
                foreach(Vector3Int offset in movementOffsets){
                    Debug.Log("offset in movement offsets : " + offset);
                    tmManager.ActivateMovementTile(offset + myTilePos, team);
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
                            tmManager.ActivateMovementTile(myTilePos + offsets, team);
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
    public void ShowTowerPlacement(){
        Debug.Log("showing range for tower placement");
        tmManager.ClearMovement();
        Vector3Int myTilePos = tmManager.tilemapArray[(int)TilemapManager.MapType.ground].WorldToCell(transform.position);


        Vector3Int offset = Vector3Int.zero;
        for(offset.x = -1; offset.x <= 1; offset.x++){
            for(offset.y = -1; offset.y <= 1; offset.y++){
                tmManager.ActivateMovementTile(myTilePos + offset, team);
            }
        }

        showTowerPlacement = false;
    }
    public void PlaceTower(Vector3Int mouseTilePos){
        tmManager.PlaceTower(mouseTilePos);

        showTowerPlacement = false;
    }    

    //abilities are benched currently
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
