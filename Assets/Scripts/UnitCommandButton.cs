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
    private TextMeshProUGUI buttonLabel;
    private CanvasGroup canvasGroup;

    protected virtual void Start()
    {
        button = GetComponent<Button>();
        buttonImage = GetComponent<Image>();
        buttonLabel = GetComponentInChildren<TextMeshProUGUI>();
        canvasGroup = GetComponent<CanvasGroup>();

        UnitSelectionManager.Instance.OnUnitSelected += HandleUnitSelected;
        UnitSelectionManager.Instance.OnUnitDeselected += HandleUnitDeselected;

        GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;

        button.onClick.AddListener(OnButtonClicked);
        HideButton();
    }

    #region Command button visibility and interactability

    private void HandleUnitSelected(SelectableUnit unit)
    {
        if (unit.GetAvailableCommands().Contains(this.GetCommandType()))
        {
            ShowButton();
        }
    }

    protected abstract CommandType GetCommandType();

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

    #endregion

    #region Command button selection and command execution

    public void OnButtonClicked()
    {
        UnitSelectionManager.Instance.ClickCommand(this);
    }

    public void SelectCommand(UnitCommandButton selectedCommand, SelectableUnit selectedUnit)
    {
        if (selectedCommand == this)
        {
            buttonImage.color = highlightedColor;
        }
        else
        {
            buttonImage.color = normalColor;
        }

        DoCommandSelection(selectedUnit);
    }

    public abstract void DoCommandSelection(SelectableUnit selectedUnit);

    public void ClearCommand()
    {
        DoCommandClear();

        buttonImage.color = normalColor;
    }

    public abstract void DoCommandClear();

    public abstract bool TryExecuteCommand(SelectableUnit selectedUnit, Vector2Int clickedCell);

    #endregion
}
