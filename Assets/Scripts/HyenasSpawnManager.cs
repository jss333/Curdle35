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

    [Header("Config - Spawn markers")]
    [SerializeField] private SpawnMarker spawnMarkerPrefab;

    [Header("Config - Hyenas")]
    [SerializeField] private GameObject hyenaPrefab;
    [SerializeField] private Transform hyenasManager;
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

    private static readonly HyenasSpawnAI HyenasSpawnAI = new();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;

        CurrentSpawnRate = startSpawnRate;
        NextSpawnRateUpgradeCost = GetNextSpawnRateUpgradeCostBasedOnCurrentStage();
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
        // Determine where hyenas will spawn next
        List<Vector2Int> spawnPoints = HyenasSpawnAI.GetSpawnPoints(currentSpawnRate);

        // Instantiate spawn markers at each location
        foreach (Vector2Int spawnPoint in spawnPoints)
        {
            SpawnMarker spawnMarker = Instantiate(spawnMarkerPrefab, BoardManager.Instance.BoardCellToWorld(spawnPoint), Quaternion.identity);
            spawnMarker.transform.SetParent(transform);
            spawnMarker.SetBoardCell(spawnPoint);
            spawnMarker.name = "Spawn marker @ " + spawnPoint;
        }

        GameManager.Instance.OnHyenasFinishGeneratingNewSpawnMarkers();
    }

    private void SpawnHyenasFromMarkers()
    {
        Debug.Log("Spawning hyenas from markers");

        Sequence allSpawnSeq = DOTween.Sequence();

        foreach (Transform child in transform)
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
            GameObject hyena = Instantiate(hyenaPrefab, worldPos, Quaternion.identity);
            hyena.transform.SetParent(hyenasManager); // Unit's logical board position is registered in BoardManager by Unit.Start()
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
