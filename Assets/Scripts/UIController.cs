using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIController : MonoBehaviour
{
    [Header("Config - Refs to UI components")]
    [SerializeField] private TextMeshProUGUI gameStateText;
    [SerializeField] private TextMeshProUGUI playerResourcesTxt;
    [SerializeField] private TextMeshProUGUI hyenasResourcesTxt;
    [SerializeField] private Button endTurnButton;

    void Start()
    {
        GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
        HandleGameStateChanged(GameManager.Instance.CurrentState); // Set initial button state

        ResourcesManager resourceMngr = ResourcesManager.Instance;
        if(resourceMngr == null) Debug.LogError("ResourcesManager not found in scene!");
        resourceMngr.OnPlayerResourcesChanged += UpdatePlayerResourcesUI;
        UpdatePlayerResourcesUI(resourceMngr.PlayerResources);

        resourceMngr.OnHyenasResourcesChanged += UpdateHyenasResourcesUI;
        UpdateHyenasResourcesUI(resourceMngr.HyenasResources);

        endTurnButton.onClick.AddListener(OnEndTurnClicked);
    }

    public void HandleGameStateChanged(GameState state)
    {
        endTurnButton.interactable = (state == GameState.PlayerInput);
        gameStateText.text = state.ToString();
    }

    public void UpdatePlayerResourcesUI(int newResources)
    {
        Debug.Log("Updating player resources UI: " + newResources);
        playerResourcesTxt.text = newResources.ToString("D3");
    }

    public void UpdateHyenasResourcesUI(int newResources)
    {
        Debug.Log("Updating hyenas resources UI: " + newResources);
        hyenasResourcesTxt.text = newResources.ToString("D3");
    }

    public void OnEndTurnClicked()
    {
        GameManager.Instance.OnPlayerEndsTurn();
    }
}
