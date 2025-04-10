using UnityEngine;
using TMPro;

public class ResourcesManager : MonoBehaviour
{
    public static ResourcesManager Instance { get; private set; }

    public event System.Action<int> OnPlayerResourcesChanged;
    public event System.Action<int> OnHyenasResourcesChanged;

    [Header("State")]
    [SerializeField] private int initialPlayerResources;
    [SerializeField] private int initialHyenasResources;

    [Header("State")]
    [SerializeField] private int playerResources;
    [SerializeField] private int hyenasResources;

    private void Awake()
    {
        Instance = this;

        playerResources = initialPlayerResources;
        hyenasResources = initialHyenasResources;
    }

    private void Start()
    {
        GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
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

    public int HyenasResources
    {
        get => hyenasResources;
        set
        {
            hyenasResources = value;
            OnHyenasResourcesChanged?.Invoke(hyenasResources);
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

            // TODO check hyenas spawn rate uprade conditions (if resource >= X, spend them to increase rate)

            GameManager.Instance.OnHyenasFinishHarvesting();
        }
    }
}
