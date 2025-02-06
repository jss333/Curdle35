using UnityEngine;
using UnityEngine.Tilemaps;

public class UnitController : MonoBehaviour
{
    Vector2 destinationPos;
    public Movement.Type movementType;
    bool currentlyControlled = false;   

    public int startingAP = 10;
    public int remainingAP = 10;

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
        
        if (Input.GetMouseButtonDown(0)) // Left-click detection
        {
            tmClick.OnClick(transform.position, movementType);
        }
    }

    void TurnUpdate(){

    }

    public virtual Vector2 ShowMovementPath(){

        

        return Vector2.zero;
    }
    public virtual bool TurnControlUpdate(){
            destinationPos = ShowMovementPath();
        if(Input.GetMouseButtonDown(0)){
        }
        return remainingAP == 0;
    }
}
