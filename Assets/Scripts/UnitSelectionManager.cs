using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using System;
using UnityEngine.EventSystems;

public class UnitSelectionManager : MonoBehaviour
{
    public static UnitSelectionManager Instance { get; private set; }

    public event Action<SelectableUnit> OnUnitSelected;
    public event Action<SelectableUnit> OnUnitDeselected;

    [Header("State")]
    [SerializeField] private SelectableUnit currentlySelectedUnit;
    [SerializeField] private UnitCommandButton currentCommand;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
    }

    void Update()
    {
        if (GameManager.Instance.CurrentState != GameState.PlayerInput) return;

        // Handle left click
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;

            if (TryExecuteCommand()) return;

            HandleUnitSelection();
        }

        // Handle ESC key
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if(currentCommand != null)
            {
                ClearCommand();
            }
            else
            {
                DeselectCurrentUnitIfAny();
            }
        }
    }

    #region Unit selection

    private void HandleUnitSelection()
    {
        SelectableUnit selectableUnit = RaycastToFindUnitAt(Input.mousePosition);

        if (selectableUnit == null)
        {
            DeselectCurrentUnitIfAny();
        }
        else
        {
            SelectUnit(selectableUnit);
        }
    }

    private SelectableUnit RaycastToFindUnitAt(Vector3 position)
    {
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(position), Vector2.zero); //direction of 0 means just check under the point

        if (hit.collider != null)
        {
            SelectableUnit selectableUnit = hit.collider.GetComponent<SelectableUnit>();
            if(selectableUnit != null && selectableUnit.enabled)
            {
                return selectableUnit;
            }
        }

        return null;
    }

    public void DeselectCurrentUnitIfAny()
    {
        if (currentlySelectedUnit != null)
        {
            SelectableUnit previouslySelectedUnit = currentlySelectedUnit;
            currentlySelectedUnit.RemoveSelectedEffect();
            currentlySelectedUnit = null;
            OnUnitDeselected?.Invoke(previouslySelectedUnit);

            ClearCommand();
        }
    }

    private void SelectUnit(SelectableUnit newlySelectedUnit)
    {
        if (newlySelectedUnit == currentlySelectedUnit) return;

        DeselectCurrentUnitIfAny();
        
        currentlySelectedUnit = newlySelectedUnit;
        currentlySelectedUnit.ShowSelectedEffect();
        OnUnitSelected?.Invoke(currentlySelectedUnit);
    }

    public SelectableUnit GetCurrentlySelectedUnit()
    {
        return currentlySelectedUnit;
    }

    public void HandleGameStateChanged(GameState state)
    {
        // If we have a unit that's selected, remove the selected effect when not in PlayerInput/UnitPlayerUnitMoving state
        if (currentlySelectedUnit != null)
        {
            if(state == GameState.PlayerInput || state == GameState.PlayerUnitMoving)
            {
                currentlySelectedUnit.ShowSelectedEffect();
            }
            else
            {
                currentlySelectedUnit.RemoveSelectedEffect();
            }
        }
    }

    #endregion

    #region Command selection and execution

    public void ClickCommand(UnitCommandButton command)
    {
        if (currentCommand != null)
        {
            if (command == currentCommand)
            {
                ClearCommand();
                return;
            }
            else
            {
                ClearCommand();
            }
        }

        currentCommand = command;
        currentCommand.SelectCommand(currentCommand, currentlySelectedUnit);
    }

    public void ClearCommand()
    {
        if (currentCommand == null) return;

        currentCommand.ClearCommand();
        currentCommand = null;
    }

    private bool TryExecuteCommand()
    {
        if (currentlySelectedUnit == null) return false;

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2Int clickedCell = BoardManager.Instance.WorldToBoardCell(mouseWorld);

        if(currentCommand == null)
        {
            return false;
        }
        else
        {
            return currentCommand.TryExecuteCommand(currentlySelectedUnit, clickedCell);
        }
    }

    #endregion
}
