using UnityEngine;
using System.Collections.Generic;

public class MoveCommandButton : UnitCommandButton
{
    [Header("Config - Label")]
    [SerializeField] private string canMoveLabel = "Move";
    [SerializeField] private string cannotMoveLabel = "Moved";

    protected override CommandType GetCommandType()
    {
        return CommandType.Move;
    }

    protected override void CalculateInteractabilityAndLabel(out bool interactableDuringPlayerInput, out string label)
    {
        if (UnitHasMoveRange(UnitSelectionManager.Instance.GetCurrentlySelectedUnit(), out var moveRange))
        {
            interactableDuringPlayerInput = moveRange.CanMoveThisTurn();
            label = moveRange.CanMoveThisTurn() ? canMoveLabel : cannotMoveLabel;
        }
        else
        {
            interactableDuringPlayerInput = false;
            label = canMoveLabel;
        }
    }

    public override void DoCommandSelection(SelectableUnit selectedUnit)
    {
        if(UnitHasMoveRange(selectedUnit, out var moveRange))
        {
            BoardManager.Instance.ShowMovementRange(moveRange);
        }
    }

    public override void DoCommandClear()
    {
        BoardManager.Instance.ClearAllRanges();
    }

    public override bool TryExecuteCommand(SelectableUnit selectedUnit, Vector2Int clickedCell)
    {
        if(UnitHasMoveRangeAndIsMoveable(selectedUnit, out var moveRange, out var movableUnit))
        {
            if (!moveRange.CanMoveThisTurn())
            {
                Debug.LogError($"Unit {movableUnit.name} has already moved this turn.");
                return false;
            }

            if (moveRange.IsCellInRange(clickedCell))
            {
                IEnumerable<Vector2Int> path = moveRange.BuildPathToOrthogonalOrDiagonalDestination(clickedCell);

                GameManager.Instance.OnPlayerUnitStartsMoving();
                movableUnit.MoveAlongPath(path, () => { moveRange.MarkAsMovedThisTurn(); GameManager.Instance.OnPlayerUnitFinishesMoving(); });
                UnitSelectionManager.Instance.ClearCommand();

                return true;
            }
        }

        return false;
    }

    private bool UnitHasMoveRange(SelectableUnit selectedUnit, out MovementRange moveRange)
    {
        if (selectedUnit == null)
        {
            Debug.LogError($"No selected unit for Move command.");
            moveRange = null;
            return false;
        }

        if (!selectedUnit.TryGetComponent<MovementRange>(out moveRange))
        {
            Debug.LogError($"Selected unit {selectedUnit.name} does not have a MovementRange component.");
            return false;
        }

        return true;
    }

    private bool UnitHasMoveRangeAndIsMoveable(SelectableUnit selectedUnit, out MovementRange moveRange, out MovableUnit movableUnit)
    {
        if(!UnitHasMoveRange(selectedUnit, out moveRange))
        {
            movableUnit = null;
            return false;
        }

        if(!selectedUnit.TryGetComponent<MovableUnit>(out movableUnit))
        {
            Debug.LogError($"Selected unit {selectedUnit.name} does not have a MovableUnit component.");
            return false;
        }

        return true;
    }
}
