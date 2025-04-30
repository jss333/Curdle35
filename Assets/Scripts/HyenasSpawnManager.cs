using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class HyenasSpawnManager : MonoBehaviour
{
    public static HyenasSpawnManager Instance { get; private set; }

    public event System.Action<int> OnSpawnRateChanged;
    public event System.Action<int> OnUpgradeCostChanged;

    [Header("Config - Spawn rate")]
    [SerializeField] private int startSpawnRate = 1;
    [SerializeField] private int spawnRateUpgradeCostConstant = 5;
    [SerializeField] private int spawnRateUpgradeCostStageMultiplier = 5;
    [SerializeField] private bool haltSpawning = false; // Used for testing purposes, to stop spawning hyenas

    [Header("Config - Spawn strategy and markers")]
    [Tooltip("Assign a MonoBehaviour that implements IHyenaSpawnStrategy.")]
    [SerializeField] private MonoBehaviour spawnStrategy;
    [SerializeField] private SpawnMarker spawnMarkerPrefab;
    [SerializeField] private Transform spawnMarkersParent;

    [Header("Config - Hyenas")]
    [SerializeField] private HyenaUnit hyenaPrefab;
    [SerializeField] private Transform hyenasParent;
    [SerializeField] private float maxDelayBeforeSpawning = 0.5f;

    [Header("State")]
    [SerializeField] private int currentSpawnRate;
    [SerializeField] private int currentSpawnRateStage = 0;
    [SerializeField] private int nextSpawnRateUpgradeCost;
    [SerializeField] private int nextHyenaId = 0;

    public int CurrentSpawnRate
    {
        get => currentSpawnRate;
        set
        {
            currentSpawnRate = value;
            OnSpawnRateChanged?.Invoke(currentSpawnRate);
        }
    }

    public int NextSpawnRateUpgradeCost
    {
        get => nextSpawnRateUpgradeCost;
        set
        {
            nextSpawnRateUpgradeCost = value;
            OnUpgradeCostChanged?.Invoke(nextSpawnRateUpgradeCost);
        }
    }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;

        CurrentSpawnRate = startSpawnRate;
        NextSpawnRateUpgradeCost = GetNextSpawnRateUpgradeCostBasedOnCurrentStage();

        GetSpawnStrategy(); // Validate the spawn strategy

        Debug.Log("=== HyenasSpawnManager initialized and listeners set up. ===");
    }

    private void HandleGameStateChanged(GameState newState)
    {
        if (newState == GameState.HyenasGenerateNewSpawnMarkers)
        {
            InstantiateSpawnMarkers();
        }

        if (newState == GameState.HyenasSpawning)
        {
            SpawnHyenasFromMarkers();
        }
    }

    private void InstantiateSpawnMarkers()
    {
        if (haltSpawning)
        {
            GameManager.Instance.OnHyenasFinishGeneratingNewSpawnMarkers();
            return;
        }

        // Determine where hyenas will spawn next
        List<Vector2Int> spawnPoints = GetSpawnPointsFromStrategyAndValidate(currentSpawnRate);

        SoundsManager.Instance.PlaySFX(SFX.Hyenas_Generate_Spawn_Marker);

        // Instantiate spawn markers at each location
        foreach (Vector2Int spawnPoint in spawnPoints)
        {
            SpawnMarker spawnMarker = Instantiate(spawnMarkerPrefab, BoardManager.Instance.BoardCellToWorld(spawnPoint), Quaternion.identity, spawnMarkersParent);
            spawnMarker.SetBoardCell(spawnPoint);
            spawnMarker.name = "Spawn marker @ " + spawnPoint;
        }

        GameManager.Instance.OnHyenasFinishGeneratingNewSpawnMarkers();
    }

    private List<Vector2Int> GetSpawnPointsFromStrategyAndValidate(int numSpawnPoints)
    {
        IHyenaSpawnStrategy strategy = GetSpawnStrategy();
        if (strategy == null)
        {
            return new List<Vector2Int>();
        }

        List<Vector2Int> spawnPoints = strategy.GetSpawnCells(numSpawnPoints);

        if (spawnPoints == null)
        {
            Debug.LogError($"Requested {numSpawnPoints} spawn points from {spawnStrategy.name} but null was returned.");
            return new List<Vector2Int>();
        }
        else if (spawnPoints.Count < numSpawnPoints)
        {
            Debug.LogWarning($"Requested {numSpawnPoints} spawn points from {spawnStrategy.name} but only {spawnPoints.Count} were generated: {string.Join(", ", spawnPoints)}.");
        }
        else if (spawnPoints.Count > numSpawnPoints)
        {
            Debug.LogWarning($"Requested {numSpawnPoints} spawn points from {spawnStrategy.name} but {spawnPoints.Count} were generated: {string.Join(", ", spawnPoints)}. Will ignore the extra ones.");
            spawnPoints.RemoveRange(numSpawnPoints, spawnPoints.Count - numSpawnPoints);
        }
        else
        {
            Debug.Log($"Strategy {spawnStrategy.name} generated {spawnPoints.Count} points: {string.Join(", ", spawnPoints)}.");
        }

        return spawnPoints;
    }

    private IHyenaSpawnStrategy GetSpawnStrategy()
    {
        if (spawnStrategy == null)
        {
            Debug.LogError("Property spawnStrategy is not assigned");
            return null;
        }
        IHyenaSpawnStrategy strategy = spawnStrategy as IHyenaSpawnStrategy;
        if (strategy == null)
        {
            Debug.LogError($"Object {spawnStrategy.name} is assigned as spawnStrategy, does not implement IHyenaSpawnStrategy");
        }
        return strategy;
    }

    private void SpawnHyenasFromMarkers()
    {
        Debug.Log("Spawning hyenas from markers");

        Sequence allSpawnSeq = DOTween.Sequence();

        foreach (Transform child in spawnMarkersParent)
        {
            if (child.TryGetComponent<SpawnMarker>(out var spawnMarker))
            {
                Sequence spawnSeq = DOTween.Sequence();
                spawnSeq
                    .AppendInterval(Random.Range(0, maxDelayBeforeSpawning))
                    .AppendCallback(() => InstantiateHyenaAtMarkerLocation(spawnMarker))
                    .AppendInterval(0.1f); // Give the Start() code in Unit time to run and register the hyena's position in BoardManager

                allSpawnSeq.Join(spawnSeq);
            }
        }

        allSpawnSeq.onComplete = GameManager.Instance.OnHyenasFinishSpawningFromMarkers;
        SoundsManager.Instance.PlaySFX(SFX.Hyenas_Spawn);
        allSpawnSeq.Play();
    }

    private void InstantiateHyenaAtMarkerLocation(SpawnMarker spawnMarker)
    {
        BoardManager boardMngr = BoardManager.Instance;
        Vector2Int cell = spawnMarker.GetBoardCell();
        Vector3 worldPos = boardMngr.BoardCellToWorld(cell);

        // Destroy the spawn marker
        Destroy(spawnMarker.gameObject);

        // TODO also spawn a dust cloud that self-destructs once the animation ends

        // Check if the cell is already occupied
        if (boardMngr.TryGetUnitAt(cell, out Unit unit))
        {
            if (unit.GetFaction() == Faction.Hyenas || unit.IsStructure())
            {
                Debug.LogError($"Trying to spawn a new hyena at {cell} but {unit.name} is already there. Will ignore spawn.");
            }
            else if(unit.GetFaction() == Faction.Cats)
            {
                Debug.Log($"New hyena spawned at {cell} which is occupied by {unit.name}. Unit will take damage and hyena won't be spawned.");
                unit.TakeDamage(1);
            }
        }
        else
        {
            // Spawns a hyena at the spawn marker location
            HyenaUnit hyena = Instantiate(hyenaPrefab, worldPos, Quaternion.identity);
            hyena.transform.SetParent(hyenasParent); // Unit's logical board position is registered in BoardManager by Unit.Start()
            hyena.name = "Hyena #" + nextHyenaId++;
        }
    }

    public bool IsEnoughResourcesToUpgradeSpawnRate(int hyenasResources)
    {
        bool isEnough = hyenasResources >= nextSpawnRateUpgradeCost;
        Debug.Log($"Hyenas do{(isEnough ? "" : " not")} have enough resources ({hyenasResources}) to pay upgrade spawn rate cost ({nextSpawnRateUpgradeCost}).");
        return isEnough;
    }

    public int UpgradeSpawnRateAndReturnTotalCost()
    {
        // TODO animate and return only when animation ends
        SoundsManager.Instance.PlaySFX(SFX.Hyenas_Upgrade_Spawn_Rate);

        int prevSpawnRateUpgradeCost = nextSpawnRateUpgradeCost;
        CurrentSpawnRate++;
        currentSpawnRateStage++;
        NextSpawnRateUpgradeCost = GetNextSpawnRateUpgradeCostBasedOnCurrentStage();
        Debug.Log($"Hyenas spawn rate upgraded to {currentSpawnRate}. It cost {prevSpawnRateUpgradeCost} resources. Next upgrade at {nextSpawnRateUpgradeCost}");

        return prevSpawnRateUpgradeCost;
    }

    private int GetNextSpawnRateUpgradeCostBasedOnCurrentStage()
    {
        return spawnRateUpgradeCostConstant + (currentSpawnRateStage * spawnRateUpgradeCostStageMultiplier);
    }
}
