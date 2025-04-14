using UnityEngine;

public class TurretsManager : MonoBehaviour
{
    public static TurretsManager Instance { get; private set; }

    [Header("Config")]
    [SerializeField] private GameObject turretPrefab;

    [Header("State")]
    [SerializeField] private int nextTurretId = 0;

    void Awake()
    {
        Instance = this;
    }

    public void BuildTurretAt(Vector2Int cell)
    {
        BoardManager boardMngr = BoardManager.Instance;

        if (boardMngr.TryGetUnitAt(cell, out Unit unit))
        {
            Debug.LogError($"Trying to build a new turret at {cell} but {unit.name} is already there. Will ignore build.");
        }
        else
        {
            Vector3 worldPos = boardMngr.BoardCellToWorld(cell);
            GameObject turret = Instantiate(turretPrefab, worldPos, Quaternion.identity); // Unit's logical board position is registered in BoardManager by Unit.Start()
            turret.transform.SetParent(this.transform); 
            turret.name = $"Turret #{nextTurretId++} @ {cell}";
        }
    }
}
