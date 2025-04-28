using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    [Header("Config - Refs to UI components")]
    [SerializeField] private Button endTurnButton;
    [SerializeField] private TextMeshProUGUI gameStateText;
    [SerializeField] private TextMeshProUGUI playerResourcesTxt;
    [SerializeField] private TextMeshProUGUI playerNextHarvestTxt;
    [SerializeField] private TextMeshProUGUI hyenasResourcesTxt;
    [SerializeField] private TextMeshProUGUI hyenasNextHarvestTxt;
    [SerializeField] private TextMeshProUGUI hyenasNextSpawnRateIncreaseTxt;
    [SerializeField] private TextMeshProUGUI hyenasSpawnRateTxt;
    [SerializeField] private Button helpConfigButton;
    [SerializeField] private GameObject helpConfigPanel;
    [SerializeField] private TextMeshProUGUI masterBGMVolumeValueTxt;
    [SerializeField] private TextMeshProUGUI masterSFXVolumeValueTxt;

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

        helpConfigButton.onClick.AddListener(ShowHideHelpConfigPanel);
        helpConfigPanel.SetActive(false);

        Debug.Log("=== UIController initialized and listeners set up. ===");
    }

    public void OnEndTurnClicked()
    {
        UnitSelectionManager.Instance.ClearCommand();
        SoundsManager.Instance.PlaySFX(SFX.Player_Confirms_End_Of_Turn);
        GameManager.Instance.OnPlayerEndsTurn();
    }

    public void HandleGameStateChanged(GameState state)
    {
        gameStateText.text = state.ToString();

        bool isPlayerInputState = (state == GameState.PlayerInput);
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

    public void ShowHideHelpConfigPanel()
    {
        helpConfigPanel.SetActive(!helpConfigPanel.activeSelf);
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadSceneAsync(0);
    }

    public void OnMasterBGMVolumeChanged(float newVolume)
    {
        masterBGMVolumeValueTxt.text = FormatAsPercent(newVolume);
    }

    public void OnMasterSFXVolumeChanged(float newVolume)
    {
        masterSFXVolumeValueTxt.text = FormatAsPercent(newVolume);
    }

    private string FormatAsPercent(float value)
    {
        return ((int)(value * 100)).ToString("D1") + "%";
    }
}
