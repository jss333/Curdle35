using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using System;
using static UnityEngine.UI.Image;

public class UnitSelectionManager : MonoBehaviour
{
    public static UnitSelectionManager Instance { get; private set; }

    public event Action<SelectableUnit> OnUnitSelected;
    public event Action<SelectableUnit> OnUnitDeselected;

    [Header("State")]
    [SerializeField] private SelectableUnit currentlySelectedUnit;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (!GameManager.Instance.IsPlayerInputState()) return;

        if (Input.GetMouseButtonDown(0)) // Left-click
        {
            if (TryHandleMoveInput()) return;
            HandleSelection();
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

    private void DeselectCurrentUnitIfAny()
    {
        if (currentlySelectedUnit != null)
        {
            SelectableUnit previouslySelectedUnit = currentlySelectedUnit;
            currentlySelectedUnit.DoUnitDeselection();
            currentlySelectedUnit = null;
            OnUnitDeselected?.Invoke(previouslySelectedUnit);
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

        var moveRange = currentlySelectedUnit.GetMovementRange();
        var movableUnit = currentlySelectedUnit.GetComponent<MovableUnit>();

        if(moveRange == null || movableUnit == null) return false;

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2Int clickedCell = BoardManager.Instance.WorldToBoardCell(mouseWorld);

        if (moveRange.IsCellInMovementRange(clickedCell))
        {
            IEnumerable<Vector2Int> path = moveRange.BuildPathToOrthogonalOrDiagonalDestination(clickedCell);

            GameManager.Instance.OnPlayerUnitStartsMoving();
            movableUnit.MoveAlongPath(path, GameManager.Instance.OnPlayerUnitFinishesMoving);
            DeselectCurrentUnitIfAny();

            return true;
        }
        
        return false;
    }
}
