#nullable enable

using UnityEngine;
using System.Collections;

public class UnitSelectionManager : MonoBehaviour
{
    [Header("State")]
    [SerializeField] private Unit? selectedUnit;

    void Update()
    {
        if (!GameManager.Instance.IsPlayerInputState()) return;

        if (Input.GetMouseButtonDown(0)) // Left-click
        {
            if (HandleMoveInput()) return;
            HandleSelection();
        }
    }

    private void HandleSelection()
    {
        Unit? unit = RaycastToFindUnitAt(Input.mousePosition);

        if (unit != null)
        {
            SelectUnitAndShowMovementRange(unit);
        }
        else
        {
            DeselectCurrentUnitAndClearMovementRange();
        }
    }

    private Unit? RaycastToFindUnitAt(Vector3 position)
    {
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(position), Vector2.zero); //direction of 0 means just check under the point

        if (hit.collider != null)
        {
            return hit.collider.GetComponent<Unit>();
        }

        return null;
    }

    private void SelectUnitAndShowMovementRange(Unit unit)
    {
        if (selectedUnit != null)
        {
            DeselectCurrentUnitAndClearMovementRange();
        }

        selectedUnit = unit;
        selectedUnit.ShowSelected();

        PlayerMovableUnit? movableUnit = unit.GetPlayerMovableUnit();
        if (movableUnit != null)
        {
            BoardManager.Instance.ShowMovementRangeForUnit(movableUnit);
        }
    }

    private void DeselectCurrentUnitAndClearMovementRange()
    {
        if (selectedUnit != null)
        {
            selectedUnit.ShowDeselected();
            BoardManager.Instance.ClearMovementRange();
            selectedUnit = null;
        }
    }

    private bool HandleMoveInput()
    {
        if (selectedUnit == null) return false;
        
        PlayerMovableUnit? movableUnit = selectedUnit.GetPlayerMovableUnit();
        if(movableUnit == null) return false;
        
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2Int pos = GridHelper.Instance.WorldToGrid(mouseWorld);

        if (movableUnit.IsCellInMoveRange(pos))
        {
            StartCoroutine(selectedUnit.MoveToCell(pos));
            DeselectCurrentUnitAndClearMovementRange();
            return true;
        }

        return false;
    }
}
