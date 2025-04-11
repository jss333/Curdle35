using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using System;
using static UnityEngine.UI.Image;
using UnityEngine.EventSystems;

public enum UnitCommandMode
{
    None,
    Move
}

public class UnitSelectionManager : MonoBehaviour
{
    public static UnitSelectionManager Instance { get; private set; }

    public event Action<SelectableUnit> OnUnitSelected;
    public event Action<SelectableUnit> OnUnitDeselected;
    public event Action<UnitCommandMode> OnCommandSelected;
    public event Action OnCommandUnselected;

    [Header("State")]
    [SerializeField] private SelectableUnit currentlySelectedUnit;
    [SerializeField] private UnitCommandMode currentCommand = UnitCommandMode.None;

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
        if (!GameManager.Instance.IsPlayerInputState()) return;

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
            if(currentCommand != UnitCommandMode.None)
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

    public void SelectCommand(UnitCommandMode command)
    {
        currentCommand = command;
        OnCommandSelected?.Invoke(currentCommand);

        if(command == UnitCommandMode.Move)
        {
            DoMoveCommandSelection();
        }
    }

    public void ClearCommand()
    {
        if(currentCommand == UnitCommandMode.None) return;

        UnitCommandMode previousCommand = currentCommand;
        currentCommand = UnitCommandMode.None;
        OnCommandUnselected?.Invoke();

        if(previousCommand == UnitCommandMode.Move)
        {
            DoMoveCommandClear();
        }
    }

    private bool TryExecuteCommand()
    {
        if (currentlySelectedUnit == null) return false;

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2Int clickedCell = BoardManager.Instance.WorldToBoardCell(mouseWorld);

        if(currentCommand == UnitCommandMode.Move)
        {
            return TryExecuteMoveCommand(clickedCell);
        }
        else
        {
            return false;
        }
    }

    #endregion

    #region Move command

    private void DoMoveCommandSelection()
    {
        if (currentlySelectedUnit == null) return;

        MovementRange mvmtRange = currentlySelectedUnit.GetComponent<MovementRange>();
        if (mvmtRange != null)
        {
            BoardManager.Instance.ShowMovementRange(mvmtRange);
        }
    }

    private static void DoMoveCommandClear()
    {
        BoardManager.Instance.ClearMovementRange();
    }

    private bool TryExecuteMoveCommand(Vector2Int clickedCell)
    {
        var moveRange = currentlySelectedUnit.GetComponent<MovementRange>();
        var movableUnit = currentlySelectedUnit.GetComponent<MovableUnit>();

        if (moveRange == null || movableUnit == null) return false;

        if (moveRange.IsCellInMovementRange(clickedCell))
        {
            IEnumerable<Vector2Int> path = moveRange.BuildPathToOrthogonalOrDiagonalDestination(clickedCell);

            GameManager.Instance.OnPlayerUnitStartsMoving();
            movableUnit.MoveAlongPath(path, GameManager.Instance.OnPlayerUnitFinishesMoving);
            ClearCommand();

            return true;
        }

        return false;
    }

    #endregion
}
