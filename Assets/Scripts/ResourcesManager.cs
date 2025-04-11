using UnityEngine;
using TMPro;
using System;

public class ResourcesManager : MonoBehaviour
{
    public static ResourcesManager Instance { get; private set; }

    public event System.Action<int> OnPlayerResourcesChanged;
    public event System.Action<int> OnPlayerNextHarvesChanged;
    public event System.Action<int> OnHyenasResourcesChanged;
    public event System.Action<int> OnHyenasNextHarvesChanged;

    [Header("Config")]
    [SerializeField] private int initialPlayerResources;
    [SerializeField] private int initialHyenasResources;
    [SerializeField] private HyenasSpawnManager hyenasSpawnManager;

    [Header("State")]
    [SerializeField] private int playerResources;
    [SerializeField] private int playerNextHarvest;
    [SerializeField] private int hyenasResources;
    [SerializeField] private int hyenasNextHarvest;

    void Awake()
    {
        Instance = this;

        playerResources = initialPlayerResources;
        hyenasResources = initialHyenasResources;
    }

    void Start()
    {
        GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;

        BoardManager boardMngr = BoardManager.Instance;
        PlayerNextHarvest = boardMngr.GetResourceTotalOfCellsOwnedBy(Faction.Cats);
        hyenasNextHarvest = boardMngr.GetResourceTotalOfCellsOwnedBy(Faction.Hyenas);

        boardMngr.OnCellOwnershipChanged += HandleCellOwnershipChanged;
    }

    public int PlayerResources
    {
        get => playerResources;
        set
        {
            playerResources = value;
            OnPlayerResourcesChanged?.Invoke(playerResources);
        }
    }

    public int PlayerNextHarvest
    {
        get => playerNextHarvest;
        set
        {
            playerNextHarvest = value;
            OnPlayerNextHarvesChanged?.Invoke(playerNextHarvest);
        }
    }

    public int HyenasResources
    {
        get => hyenasResources;
        set
        {
            hyenasResources = value;
            OnHyenasResourcesChanged?.Invoke(hyenasResources);
        }
    }

    public int HyenasNextHarvest
    {
        get => hyenasNextHarvest;
        set
        {
            hyenasNextHarvest = value;
            OnHyenasNextHarvesChanged?.Invoke(hyenasNextHarvest);
        }
    }

    public void HandleGameStateChanged(GameState newState)
    {
        if (newState == GameState.PlayerHarvesting)
        {
            int resourcesHarvested = BoardManager.Instance.GetResourceTotalOfCellsOwnedBy(Faction.Cats);
            // TODO animate resource collection and only update + transition game state after animation ends
            PlayerResources += resourcesHarvested;

            //TODO check player victory conditions (if resources >= X, win!)

            GameManager.Instance.OnPlayerFinishesHarvesting();
        }

        if (newState == GameState.HyenasHarvesting)
        {
            int resourcesHarvested = BoardManager.Instance.GetResourceTotalOfCellsOwnedBy(Faction.Hyenas);
            // TODO animate resource collection and only update + transition game state after animation ends
            HyenasResources += resourcesHarvested;

            //Handle upgrading of spawn rate
            if (hyenasSpawnManager.IsEnoughResourcesToUpgradeSpawnRate(hyenasResources))
            {
                HyenasResources -= hyenasSpawnManager.UpgradeSpawnRateAndReturnTotalCost();
            }

            GameManager.Instance.OnHyenasFinishHarvesting();
        }
    }

    private void HandleCellOwnershipChanged(CellData cellData, Faction fromFaction, Faction toFaction)
    {
        if (fromFaction == toFaction) return;

        int resourceValue = cellData.resourceValue;
        
        if (fromFaction == Faction.Cats) PlayerNextHarvest -= resourceValue;
        if (fromFaction == Faction.Hyenas) HyenasNextHarvest -= resourceValue;

        if (toFaction == Faction.Cats) PlayerNextHarvest += resourceValue;
        if (toFaction == Faction.Hyenas) HyenasNextHarvest += resourceValue;
    }
}
