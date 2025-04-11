using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class UnitCommandButton : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private UnitCommandMode associatedCommand;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color highlightedColor = Color.yellow;

    private Button button;
    private Image buttonImage;
    private CanvasGroup canvasGroup;

    void Awake()
    {
        button = GetComponent<Button>();
        buttonImage = GetComponent<Image>();
        canvasGroup = GetComponent<CanvasGroup>();

        button.onClick.AddListener(OnButtonClicked);
        HideButton();
    }

    void Start()
    {
        UnitSelectionManager.Instance.OnUnitSelected += HandleUnitSelected;
        UnitSelectionManager.Instance.OnUnitDeselected += HandleUnitDeselected;
        UnitSelectionManager.Instance.OnCommandSelected += HandleCommandSelected;
        UnitSelectionManager.Instance.OnCommandUnselected += HandleCommandUnselected;

        GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
    }

    private void HandleUnitSelected(SelectableUnit unit)
    {
        ShowButton();
    }

    private void HandleUnitDeselected(SelectableUnit unit)
    {
        HideButton();
    }

    private void ShowButton()
    {
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
    }

    private void HideButton()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
    }

    private void HandleCommandSelected(UnitCommandMode selectedCommand)
    {
        if (selectedCommand == associatedCommand)
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
        button.interactable = state == GameState.PlayerInput;
    }

    public void OnButtonClicked()
    {
        UnitSelectionManager.Instance.SelectCommand(associatedCommand);
    }
}
