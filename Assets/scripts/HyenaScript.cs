using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public class HyenaController : UnitController
{

    bool attackedThisTurn = false;

    public List<Vector3> movementPath;

    public bool attacking = false;
    public Vector3 attackPosition = Vector3.zero;

    public CatController target;

    [SerializeField] private int damage = 1;

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
                //Debug.Log("finished moving hyena");
                movedThisTurn = true;
                moving = false;
                tmManager.SetOccupancy(transform.position, team);

            }
            else{
                Vector3 direction = movementPath[0] - transform.position;
                float distance = direction.magnitude;
                if(distance < (speed * Time.deltaTime)){
                    transform.position = movementPath[0];
                    //Debug.Log("popping position - [" + movementPath.Count + "] : " + movementPath[0]);
                    movementPath.RemoveAt(0);
                }
                else{
                    direction /= distance;
                    transform.position += speed * direction * Time.deltaTime;
                }
            }
            tmManager.TryClaimTile(transform.position, team);

        }
        else if(attacking){
            
            Vector3 direction = attackPosition - transform.position;
            float distance = direction.magnitude;
            if(distance < (speed * Time.deltaTime)){
                transform.position = attackPosition;
                attacking = false;
                Debug.Log("finished attacking");
                movedThisTurn = true;
                //do an attack here, spawn a thing and remove hp from the cat
                target.health -= damage;
            }
            else{
                direction /= distance;
                transform.position += speed * direction * Time.deltaTime;
            }
            tmManager.TryClaimTile(transform.position, team);

        }
    }
}
