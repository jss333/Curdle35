using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UIController : MonoBehaviour
{
    [Header("Config - Refs to UI components")]
    [SerializeField] private Button moveButton;
    [SerializeField] private Button endTurnButton;
    [SerializeField] private TextMeshProUGUI gameStateText;
    [SerializeField] private TextMeshProUGUI playerResourcesTxt;
    [SerializeField] private TextMeshProUGUI playerNextHarvestTxt;
    [SerializeField] private TextMeshProUGUI hyenasResourcesTxt;
    [SerializeField] private TextMeshProUGUI hyenasNextHarvestTxt;
    [SerializeField] private TextMeshProUGUI hyenasNextSpawnRateIncreaseTxt;
    [SerializeField] private TextMeshProUGUI hyenasSpawnRateTxt;

    void Start()
    {
        endTurnButton.onClick.AddListener(OnEndTurnClicked);

        GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
        HandleGameStateChanged(GameManager.Instance.CurrentState); // Set initial button state

        ResourcesManager resourceMngr = ResourcesManager.Instance;
        resourceMngr.OnPlayerResourcesChanged += UpdatePlayerResourcesUI;
        UpdatePlayerResourcesUI(resourceMngr.PlayerResources);

        resourceMngr.OnPlayerNextHarvesChanged += UpdatePlayerNextHarvestUI;
        UpdatePlayerNextHarvestUI(resourceMngr.PlayerNextHarvest);

        resourceMngr.OnHyenasResourcesChanged += UpdateHyenasResourcesUI;
        UpdateHyenasResourcesUI(resourceMngr.HyenasResources);

        resourceMngr.OnHyenasNextHarvesChanged += UpdateHyenasNextHarvestUI;
        UpdateHyenasNextHarvestUI(resourceMngr.HyenasNextHarvest);

        HyenasSpawnManager hyenasSpawnMngr = HyenasSpawnManager.Instance;
        hyenasSpawnMngr.OnSpawnRateChanged += UpdateHyenasSpawnRateUI;
        UpdateHyenasSpawnRateUI(hyenasSpawnMngr.CurrentSpawnRate);

        hyenasSpawnMngr.OnUpgradeCostChanged += UpdateHyenasNextSpawnRateIncreaseUI;
        UpdateHyenasNextSpawnRateIncreaseUI(hyenasSpawnMngr.NextSpawnRateUpgradeCost);
    }

    public void OnEndTurnClicked()
    {
        UnitSelectionManager.Instance.ClearCommand();
        GameManager.Instance.OnPlayerEndsTurn();
    }

    public void HandleGameStateChanged(GameState state)
    {
        gameStateText.text = state.ToString();

        bool isPlayerInputState = (state == GameState.PlayerInput);
        moveButton.interactable = isPlayerInputState;
        endTurnButton.interactable = isPlayerInputState;
    }

    public void UpdatePlayerResourcesUI(int newResources)
    {
        playerResourcesTxt.text = newResources.ToString("D3");
    }

    public void UpdatePlayerNextHarvestUI(int newNextHarvest)
    {
        playerNextHarvestTxt.text = $"(+{newNextHarvest})";
    }

    public void UpdateHyenasResourcesUI(int newResources)
    {
        hyenasResourcesTxt.text = newResources.ToString("D3");
    }

    public void UpdateHyenasNextHarvestUI(int newNextHarvest)
    {
        hyenasNextHarvestTxt.text = $"(+{newNextHarvest})";
    }

    private void UpdateHyenasNextSpawnRateIncreaseUI(int nextSpawnRateUpgradeCost)
    {
        hyenasNextSpawnRateIncreaseTxt.text = nextSpawnRateUpgradeCost.ToString();
    }

    private void UpdateHyenasSpawnRateUI(int spawnRate)
    {
        hyenasSpawnRateTxt.text = spawnRate.ToString();
    }
}
