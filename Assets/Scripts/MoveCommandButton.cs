using UnityEngine;
using System.Collections.Generic;

public class MoveCommandButton : UnitCommandButton
{
    [Header("Config - Label")]
    [SerializeField] private string canMoveLabel = "Move";
    [SerializeField] private string cannotMoveLabel = "Moved";

    protected override bool CommandIsAvailableToUnit(SelectableUnit unit)
    {
        return unit.TryGetComponent<Mover>(out Mover mover) && mover.enabled;
    }

    protected override void CalculateInteractabilityAndLabel(out bool interactableDuringPlayerInput, out string label)
    {
        if (UnitIsMover(UnitSelectionManager.Instance.GetCurrentlySelectedUnit(), out Mover mover))
        {
            interactableDuringPlayerInput = mover.CanMoveThisTurn();
            label = mover.CanMoveThisTurn() ? canMoveLabel : cannotMoveLabel;
        }
        else
        {
            interactableDuringPlayerInput = false;
            label = canMoveLabel;
        }
    }

    public override void DoCommandSelection(SelectableUnit selectedUnit)
    {
        if(UnitIsMover(selectedUnit, out Mover mover))
        {
            BoardManager.Instance.ShowMovementRange(mover);
        }
    }

    public override void DoCommandClear()
    {
        BoardManager.Instance.ClearAllRanges();
    }

    public override bool TryExecuteCommand(SelectableUnit selectedUnit, Vector2Int clickedCell)
    {
        if(UnitIsMoverAndMovable(selectedUnit, out Mover mover, out MovableUnit movableUnit))
        {
            if (!mover.CanMoveThisTurn())
            {
                Debug.LogError($"Unit {mover.name} has already moved this turn.");
                return false;
            }

            if (mover.IsCellInRange(clickedCell))
            {
                IEnumerable<Vector2Int> path = mover.BuildPathToOrthogonalOrDiagonalDestination(clickedCell);

                GameManager.Instance.OnPlayerUnitStartsMoving();
                movableUnit.MoveAlongPath(path, () => { mover.MarkAsMovedThisTurn(); GameManager.Instance.OnPlayerUnitFinishesMoving(); });
                UnitSelectionManager.Instance.ClearCommand();

                return true;
            }
        }

        return false;
    }

    private bool UnitIsMoverAndMovable(SelectableUnit selectedUnit, out Mover mover, out MovableUnit movableUnit)
    {
        if (!UnitIsMover(selectedUnit, out mover))
        {
            movableUnit = null;
            return false;
        }

        if (!selectedUnit.TryGetComponent<MovableUnit>(out movableUnit))
        {
            Debug.LogError($"Selected unit {selectedUnit.name} does not have a MovableUnit component.");
            return false;
        }

        return true;
    }

    private bool UnitIsMover(SelectableUnit selectedUnit, out Mover mover)
    {
        if (selectedUnit == null)
        {
            Debug.LogError($"No selected unit for Move command.");
            mover = null;
            return false;
        }

        if (!selectedUnit.TryGetComponent<Mover>(out mover))
        {
            Debug.LogError($"Selected unit {selectedUnit.name} does not have a Mover component.");
            return false;
        }

        return true;
    }
}
