using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using TMPro;

public enum CommandType
{
    Move,
    Build
}

public abstract class UnitCommandButton : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color highlightedColor = Color.yellow;

    [Header("State")]
    [SerializeField] private bool visible;

    private Button button;
    private Image buttonImage;
    private CanvasGroup canvasGroup;
    private TextMeshProUGUI buttonLabel;

    void Awake()
    {
        button = GetComponent<Button>();
        buttonImage = GetComponent<Image>();
        canvasGroup = GetComponent<CanvasGroup>();
        buttonLabel = GetComponentInChildren<TextMeshProUGUI>();

        button.onClick.AddListener(OnButtonClicked);
        HideButton();
    }

    protected virtual void Start()
    {
        UnitSelectionManager.Instance.OnUnitSelected += HandleUnitSelected;
        UnitSelectionManager.Instance.OnUnitDeselected += HandleUnitDeselected;
        UnitSelectionManager.Instance.OnCommandSelected += HandleCommandSelected;
        UnitSelectionManager.Instance.OnCommandUnselected += HandleCommandUnselected;

        GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
    }

    private void HandleUnitSelected(SelectableUnit unit)
    {
        if (unit.GetAvailableCommands().Contains(this.GetCommandType()))
        {
            ShowButton();
        }
    }

    private void HandleUnitDeselected(SelectableUnit unit)
    {
        HideButton();
    }

    private void ShowButton()
    {
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        visible = true;
        RefreshButtonInteractabilityAndLabelIfVisible();
    }

    private void HideButton()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        visible = false;
    }

    private void HandleCommandSelected(UnitCommandButton selectedCommand)
    {
        if (selectedCommand == this)
        {
            buttonImage.color = highlightedColor;
        }
        else
        {
            buttonImage.color = normalColor;
        }
    }

    private void HandleCommandUnselected()
    {
        buttonImage.color = normalColor;
    }

    private void HandleGameStateChanged(GameState state)
    {
        RefreshButtonInteractabilityAndLabelIfVisible();
    }

    public void RefreshButtonInteractabilityAndLabelIfVisible()
    {
        if (visible)
        {
            CalculateInteractabilityAndLabel(out bool buttonInteractableDuringPlayerInput, out string buttonLabel);

            button.interactable = (GameManager.Instance.CurrentState == GameState.PlayerInput) & buttonInteractableDuringPlayerInput;
            this.buttonLabel.text = buttonLabel;
        }
    }

    protected abstract void CalculateInteractabilityAndLabel(out bool interactableDuringPlayerInput, out string label);

    public void OnButtonClicked()
    {
        UnitSelectionManager.Instance.SelectCommand(this);
    }

    public abstract CommandType GetCommandType();

    public abstract void DoCommandSelection(SelectableUnit selectedUnit);

    public abstract void DoCommandClear();

    public abstract bool TryExecuteCommand(SelectableUnit selectedUnit, Vector2Int clickedCell);
}
