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

    [SerializeField] public bool showMovement = false;

    public int startingAP = 10;
    public int remainingAP = 10;

    public int movability = 3;

    public Tilemap tilemap;
    public Tilemap movementUIMap;

    public TilemapClick tmClick;
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
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
            }
            else{
                direction /= distance;
                transform.position += direction * Time.deltaTime;
            }
        }
    }

    void TurnUpdate(){
    }

    public void MoveTo(Vector3 dest){
        destinationPos.x = Mathf.Floor(dest.x) + 0.5f;
        destinationPos.y = Mathf.Floor(dest.y) + 0.5f;
        destinationPos.z = transform.position.z;
        moving = true;
        showMovement = false;
    }
    
    public void CalculatePossibleDestinations(){
        showMovement = true;

        Vector3Int myTilePos = tilemap.WorldToCell(transform.position);
        tmClick.ClearMovement();
        switch(movementType){
            case Movement.Type.Straight:{
                    //need to do both y and X, like a cross
                    {
                        Vector3Int horizontalPath = myTilePos;
                        int rightBound = horizontalPath.x + remainingAP;
                        horizontalPath.x -= remainingAP;
                        for(; horizontalPath.x < rightBound; horizontalPath.x++){
                            tmClick.ActivateMovementTile(horizontalPath);
                        }
                    }
                    {
                        Vector3Int verticalPath = myTilePos;
                        int highBound = verticalPath.y + remainingAP;
                        verticalPath.y -= remainingAP;
                        for(; verticalPath.y < highBound; verticalPath.y++){
                            tmClick.ActivateMovementTile(verticalPath);
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
                        Debug.Log("offset iter : " + offsets);
                        if(((Math.Abs(offsets.x) + Math.Abs(offsets.y)) <= movability) && !(offsets.x == 0 && offsets.y == 0)){
                            Debug.Log("activating tile : " + offsets);
                            tmClick.ActivateMovementTile(myTilePos + offsets);
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
}
