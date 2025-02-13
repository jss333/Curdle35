using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public class HyenaController : UnitController
{

    bool attackedThisTurn = false;

    public List<Vector3Int> movementPath;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void Start() 
    {
        base.team = Team.hyena;
        base.Start();
    }

    public override void Update()
    {
        if(moving){
            if(movementPath.Count <= 0){
                Debug.Log("finished moving hyena");
                movedThisTurn = true;
                moving = false;
                tmManager.SetOccupancy(transform.position, team);

            }
            else{
                Vector3 direction = movementPath[0] - transform.position;
                float distance = direction.magnitude;
                if(distance < (speed * Time.deltaTime)){
                    transform.position = movementPath[0];
                    movementPath.RemoveAt(0);
                }
                else{
                    direction /= distance;
                    transform.position += speed * direction * Time.deltaTime;
                }
            }

        }
    }
}
