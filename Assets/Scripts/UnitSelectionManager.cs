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

    [Header("State")]
    [SerializeField] private SelectableUnit currentlySelectedUnit;
    [SerializeField] private UnitCommandMode currentCommand = UnitCommandMode.None;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (!GameManager.Instance.IsPlayerInputState()) return;

        // Handle left click
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;

            if (TryHandleMoveInput()) return;

            HandleSelection();
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

    private void HandleSelection()
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
            currentlySelectedUnit.DoUnitDeselection();
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
        currentlySelectedUnit.DoUnitSelection();
        OnUnitSelected?.Invoke(currentlySelectedUnit);
    }

    private bool TryHandleMoveInput()
    {
        if (currentlySelectedUnit == null) return false;

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2Int clickedCell = BoardManager.Instance.WorldToBoardCell(mouseWorld);

        switch (currentCommand)
        {
            case UnitCommandMode.Move:
                return TryExecuteMove(clickedCell);
            default:
                return false;
        }
    }

    private bool TryExecuteMove(Vector2Int clickedCell)
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

    public void OnMoveCommandSelected()
    {
        if (currentlySelectedUnit == null) return;

        currentCommand = UnitCommandMode.Move;

        MovementRange mvmtRange = currentlySelectedUnit.GetComponent<MovementRange>();
        if (mvmtRange != null)
        {
            BoardManager.Instance.ShowMovementRange(mvmtRange);
        }
    }

    private void ClearCommand()
    {
        currentCommand = UnitCommandMode.None;

        BoardManager.Instance.ClearMovementRange();
    }
}
