using UnityEngine;

public class TurretsManager : MonoBehaviour
{
    public static TurretsManager Instance { get; private set; }

    [Header("Config")]
    [SerializeField] private GameObject turretPrefab;
    [SerializeField] private int turretBuildCost = 15;

    [Header("State")]
    [SerializeField] private int nextTurretId = 0;

    void Awake()
    {
        Instance = this;
    }

    public int GetTurretBuildCost()
    {
        return turretBuildCost;
    }

    public void BuildTurretAt(Vector2Int cell)
    {
        BoardManager boardMngr = BoardManager.Instance;
        ResourcesManager resourceMngr = ResourcesManager.Instance;

        if (boardMngr.TryGetUnitAt(cell, out Unit unit))
        {
            Debug.LogError($"Trying to build a new turret at {cell} but {unit.name} is already there. Will ignore build.");
            return;
        }

        if (resourceMngr.PlayerResources < turretBuildCost)
        {
            Debug.LogError($"Not enough resources to build a turret at {cell}. Will ignore build.");
            return;
        }

        Vector3 worldPos = boardMngr.BoardCellToWorld(cell);
        GameObject turret = Instantiate(turretPrefab, worldPos, Quaternion.identity); // Unit's logical board position is registered in BoardManager by Unit.Start()
        turret.transform.SetParent(this.transform); 
        turret.name = $"Turret #{nextTurretId++} @ {cell}";

        resourceMngr.PlayerResources -= turretBuildCost;
    }
}
