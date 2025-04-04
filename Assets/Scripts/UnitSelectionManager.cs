#nullable enable

using UnityEngine;
using DG.Tweening;

public class UnitSelectionManager : MonoBehaviour
{
    [Header("State")]
    [SerializeField] private SelectableUnit? currentlySelectedUnit;

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
        SelectableUnit? selectableUnit = RaycastToFindUnitAt(Input.mousePosition);

        if (selectableUnit != null)
        {
            SelectUnit(selectableUnit);
        }
        else
        {
            DeselectCurrentUnitIfAny();
        }
    }

    private SelectableUnit? RaycastToFindUnitAt(Vector3 position)
    {
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(position), Vector2.zero); //direction of 0 means just check under the point

        if (hit.collider != null)
        {
            SelectableUnit? selectableUnit = hit.collider.GetComponent<SelectableUnit>();
            if(selectableUnit != null && selectableUnit.enabled)
            {
                return selectableUnit;
            }
        }

        return null;
    }

    private void SelectUnit(SelectableUnit newlySelectedUnit)
    {
        DeselectCurrentUnitIfAny();
        
        currentlySelectedUnit = newlySelectedUnit;
        currentlySelectedUnit.DoUnitSelection();
    }

    private void DeselectCurrentUnitIfAny()
    {
        if (currentlySelectedUnit != null)
        {
            currentlySelectedUnit.DoUnitDeselection();
            currentlySelectedUnit = null;
        }
    }

    private bool TryHandleMoveInput()
    {
        if (currentlySelectedUnit == null) return false;
        
        MovementRange? moveRange = currentlySelectedUnit.GetMovementRange();
        if(moveRange == null) return false;

        MovableUnit? movableUnit = currentlySelectedUnit.GetComponent<MovableUnit>();
        if(movableUnit == null) return false;

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2Int pos = GridHelper.Instance.WorldToGrid(mouseWorld);

        if (moveRange.IsCellInMovementRange(pos))
        {
            movableUnit.MoveToCell(pos);
            DeselectCurrentUnitIfAny();
            return true;
        }
        
        return false;
    }
}
