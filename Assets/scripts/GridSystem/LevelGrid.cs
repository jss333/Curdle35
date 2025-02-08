using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGrid : MonoBehaviour
{
    public static LevelGrid Instance { get; private set; }


    [SerializeField] private Transform gridDebugObjectPrefab;

    private GridSystem gridSystem;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.Log("There is more than one LevelGrid! " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;

        gridSystem = new GridSystem(10, 10, 1);
        gridSystem.CreateDebugObjects(gridDebugObjectPrefab);
    }

    public GridPosition GetGridPosition(Vector3 worldPosition) => gridSystem.GetGridPosition(worldPosition);
    public Vector3 GetWorldPosition(GridPosition gridPosition) => gridSystem.GetworldPosition(gridPosition);
    
    
    private void Update()
    {
        Debug.Log(gridSystem.GetGridPosition(GetPosition()));
    }
    
    public static Vector3 GetPosition()
    {
        Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return pos;
    }
}
