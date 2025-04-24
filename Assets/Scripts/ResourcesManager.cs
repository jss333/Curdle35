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
    [SerializeField] private int playerResourcesField;
    [SerializeField] private int playerNextHarvestField;
    [SerializeField] private int hyenasResourcesField;
    [SerializeField] private int hyenasNextHarvestField;

    void Awake()
    {
        Instance = this;

        playerResourcesField = initialPlayerResources;
        hyenasResourcesField = initialHyenasResources;
    }

    void Start()
    {
        GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;

        BoardManager boardMngr = BoardManager.Instance;
        PlayerNextHarvest = boardMngr.GetResourceTotalOfCellsOwnedBy(Faction.Cats);
        HyenasNextHarvest = boardMngr.GetResourceTotalOfCellsOwnedBy(Faction.Hyenas);

        boardMngr.OnCellOwnershipChanged += HandleCellOwnershipChanged;

        Debug.Log("=== ResourcesManager initialized and listeners set up. ===");
    }

    public int PlayerResources
    {
        get => playerResourcesField;
        set
        {
            playerResourcesField = value;
            OnPlayerResourcesChanged?.Invoke(playerResourcesField);
        }
    }

    public int PlayerNextHarvest
    {
        get => playerNextHarvestField;
        set
        {
            playerNextHarvestField = value;
            OnPlayerNextHarvesChanged?.Invoke(playerNextHarvestField);
        }
    }

    public int HyenasResources
    {
        get => hyenasResourcesField;
        set
        {
            hyenasResourcesField = value;
            OnHyenasResourcesChanged?.Invoke(hyenasResourcesField);
        }
    }

    public int HyenasNextHarvest
    {
        get => hyenasNextHarvestField;
        set
        {
            hyenasNextHarvestField = value;
            OnHyenasNextHarvesChanged?.Invoke(hyenasNextHarvestField);
        }
    }

    public void HandleGameStateChanged(GameState newState)
    {
        if (newState == GameState.PlayerHarvesting)
        {
            int resourcesHarvested = BoardManager.Instance.GetResourceTotalOfCellsOwnedBy(Faction.Cats);
            // TODO animate resource collection and only update + transition game state after animation ends
            SoundsManager.Instance.PlaySFX(SFX.Player_Harvests);
            PlayerResources += resourcesHarvested;

            //TODO check player victory conditions (if resources >= X, win!)

            GameManager.Instance.OnPlayerFinishesHarvesting();
        }

        if (newState == GameState.HyenasHarvesting)
        {
            int resourcesHarvested = BoardManager.Instance.GetResourceTotalOfCellsOwnedBy(Faction.Hyenas);
            // TODO animate resource collection and only update + transition game state after animation ends
            SoundsManager.Instance.PlaySFX(SFX.Hyenas_Harvest);
            HyenasResources += resourcesHarvested;

            //Handle upgrading of spawn rate
            if (hyenasSpawnManager.IsEnoughResourcesToUpgradeSpawnRate(hyenasResourcesField))
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
