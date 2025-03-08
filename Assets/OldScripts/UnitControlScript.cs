using System;
using System.Collections.Generic;
using UnityEngine;

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


    public TilemapManager tmManager;

    public bool showTowerPlacement = false;


    public bool movedThisTurn = false;

    public Team team = Team.hyena;

    public Sprite face_sprite;
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public virtual void Start()
    {
        tmManager = FindAnyObjectByType<TilemapManager>();

        tmManager.SetOccupancy(transform.position, team);
        tmManager.TryClaimTile(transform.position, team);
    }

    // Update is called once per frame
    public virtual void Update()
    {
        if(moving){
            if(team == Team.hyena){
                Debug.Log("hyena incorrectly moving in unitcontroller");
            }

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
            tmManager.TryClaimTile(transform.position, team);

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
        showTowerPlacement = false;
        showMovement = true;

        Vector3Int myTilePos = tmManager.tilemapArray[(int)TilemapManager.MapType.ground].WorldToCell(transform.position);
        tmManager.ClearMovement();
        switch(movementType){
            case Movement.Type.Lion:{

                Vector3Int placement = Vector3Int.zero;
                for(placement.x = -1; placement.x >= -3; placement.x--){
                    if(!tmManager.TryActivateMovementTile(placement + myTilePos, team)){
                        break;
                    }
                }
                for(placement.x = 1; placement.x <= 3; placement.x++){
                    if(!tmManager.TryActivateMovementTile(placement + myTilePos, team)){
                        break;
                    }
                }

                //currentIter should be 6 right now
                placement.x = 0;

                for(placement.y = -1; placement.y >= -3; placement.y--){
                    if(!tmManager.TryActivateMovementTile(placement + myTilePos, team)){
                        break;
                    }
                }
                for(placement.y = 1; placement.y <= 3; placement.y++){
                    if(!tmManager.TryActivateMovementTile(placement + myTilePos, team)){
                        break;
                    }
                }

                placement.x = 1;
                placement.y = 1;
                if(tmManager.TryActivateMovementTile(placement + myTilePos, team)){
                    placement.x = 2;
                    placement.y = 2;
                    tmManager.TryActivateMovementTile(placement + myTilePos, team);
                }
                placement.x = -1;
                placement.y = 1;
                if(tmManager.TryActivateMovementTile(placement + myTilePos, team)){
                    placement.x = -2;
                    placement.y = 2;
                    tmManager.TryActivateMovementTile(placement + myTilePos, team);
                }
                placement.x = -1;
                placement.y = -1;
                if(tmManager.TryActivateMovementTile(placement + myTilePos, team)){
                    placement.x = -2;
                    placement.y = -2;
                    tmManager.TryActivateMovementTile(placement + myTilePos, team);
                }
                placement.x = 1;
                placement.y = -1;
                if(tmManager.TryActivateMovementTile(placement + myTilePos, team)){
                    placement.x = 2;
                    placement.y = -2;
                    tmManager.TryActivateMovementTile(placement + myTilePos, team);
                }

                break;
            }
            case Movement.Type.Jaguar:{
                
                Vector3Int placement = Vector3Int.zero;
                for(placement.x = -1; placement.x >= -4; placement.x--){
                    if(!tmManager.TryActivateMovementTile(placement + myTilePos, team)){
                        break;
                    }
                }
                for(placement.x = 1; placement.x <= 4; placement.x++){
                    if(!tmManager.TryActivateMovementTile(placement + myTilePos, team)){
                        break;
                    }
                }

                //currentIter should be 6 right now
                placement.x = 0;

                for(placement.y = -1; placement.y >= -4; placement.y--){
                    if(!tmManager.TryActivateMovementTile(placement + myTilePos, team)){
                        break;
                    }
                }
                for(placement.y = 1; placement.y <= 4; placement.y++){
                    if(!tmManager.TryActivateMovementTile(placement + myTilePos, team)){
                        break;
                    }
                }

                placement.x = 1;
                placement.y = 1;
                if(tmManager.TryActivateMovementTile(placement + myTilePos, team)){
                    placement.x = 2;
                    placement.y = 2;
                    if(tmManager.TryActivateMovementTile(placement + myTilePos, team)){
                        placement.x = 3;
                        placement.y = 3;
                        tmManager.TryActivateMovementTile(placement + myTilePos, team);
                    }
                }
                placement.x = -1;
                placement.y = 1;
                if(tmManager.TryActivateMovementTile(placement + myTilePos, team)){
                    placement.x = -2;
                    placement.y = 2;
                    if(tmManager.TryActivateMovementTile(placement + myTilePos, team)){
                        placement.x = -3;
                        placement.y = 3;
                        tmManager.TryActivateMovementTile(placement + myTilePos, team);
                    }
                }
                placement.x = -1;
                placement.y = -1;
                if(tmManager.TryActivateMovementTile(placement + myTilePos, team)){
                    placement.x = -2;
                    placement.y = -2;
                    if(tmManager.TryActivateMovementTile(placement + myTilePos, team)){
                        placement.x = -3;
                        placement.y = -3;
                        tmManager.TryActivateMovementTile(placement + myTilePos, team);
                    }
                }
                placement.x = 1;
                placement.y = -1;
                if(tmManager.TryActivateMovementTile(placement + myTilePos, team)){
                    placement.x = 2;
                    placement.y = -2;
                    if(tmManager.TryActivateMovementTile(placement + myTilePos, team)){
                        placement.x = 3;
                        placement.y = -3;
                        tmManager.TryActivateMovementTile(placement + myTilePos, team);
                    }
                }
                break;
            }
        }
    }

    //id do an array of function pointers here in C++, just gonna handtype it for now. im assuming the max ability count is 3
    public void ShowTowerPlacement(){
        showMovement = false;
        Debug.Log("showing range for tower placement");
        tmManager.ClearMovement();
        Vector3Int myTilePos = tmManager.tilemapArray[(int)TilemapManager.MapType.ground].WorldToCell(transform.position);


        Vector3Int offset = Vector3Int.zero;
        for(offset.x = -1; offset.x <= 1; offset.x++){
            for(offset.y = -1; offset.y <= 1; offset.y++){
                tmManager.ActivateMovementTile(myTilePos + offset, team);
            }
        }

        showTowerPlacement = true;
    }
}
