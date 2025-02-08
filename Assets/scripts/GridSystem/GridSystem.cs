using Unity.Mathematics;
using UnityEngine;

public class GridSystem 
{
    private int width;
    private int height;
    private float cellSize;
    private GridObject[,] gridObjectArray;
    
    public GridSystem(int width, int height, float cellSize)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;

        gridObjectArray = new GridObject[width, height];
        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GridPosition gridPosition = new GridPosition(x, y);
                gridObjectArray[x, y] = new GridObject(this, gridPosition);
            }
        }
    }
    
    public Vector3 GetworldPosition(GridPosition gridPosition)
    {
        return new Vector3(gridPosition.x, gridPosition.y, 0) * cellSize;
    }
    
    public GridPosition GetGridPosition(Vector3 worldPosition)
    {
        return new GridPosition(
            Mathf.RoundToInt(worldPosition.x / cellSize),
            Mathf.RoundToInt(worldPosition.y / cellSize)
        );
    }
    
    public void CreateDebugObjects(Transform debugPrefab)
    {
        Vector3 startPosition = new Vector3(-width / 2 * cellSize + cellSize / 2,
            -height / 2 * cellSize + cellSize / 2, 0);
        
        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 cellPosition = startPosition + new Vector3(x * cellSize, y * cellSize, 0);
                
                GridPosition gridPosition = new GridPosition(x, y);

                Transform debugTransform = GameObject.Instantiate(debugPrefab, cellPosition, Quaternion.identity);
                GridDebugObject gridDebugObject = debugTransform.GetComponent<GridDebugObject>();
                gridDebugObject.SetGridObject(GetGridObject(gridPosition));
            }
        }
    }
    
    public GridObject GetGridObject(GridPosition gridPosition)
    {
        return gridObjectArray[gridPosition.x, gridPosition.y];
    }

}
