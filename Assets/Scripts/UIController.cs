using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIController : MonoBehaviour
{
    [SerializeField] private Button endTurnButton;
    [SerializeField] private TextMeshProUGUI gameStateText;

    void Start()
    {
        GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
        HandleGameStateChanged(GameManager.Instance.CurrentState); // Set initial button state

        endTurnButton.onClick.AddListener(OnEndTurnClicked);
    }

    public void HandleGameStateChanged(GameState state)
    {
        endTurnButton.interactable = (state == GameState.PlayerInput);
        gameStateText.text = state.ToString();
    }

    public void OnEndTurnClicked()
    {
        GameManager.Instance.OnPlayerEndTurn();
    }
}
