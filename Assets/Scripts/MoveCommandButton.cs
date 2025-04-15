using UnityEngine;
using System.Collections.Generic;

public class MoveCommandButton : UnitCommandButton
{
    public override CommandType GetCommandType()
    {
        return CommandType.Move;
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
            if (moveRange.IsCellInRange(clickedCell))
            {
                IEnumerable<Vector2Int> path = moveRange.BuildPathToOrthogonalOrDiagonalDestination(clickedCell);

                GameManager.Instance.OnPlayerUnitStartsMoving();
                movableUnit.MoveAlongPath(path, GameManager.Instance.OnPlayerUnitFinishesMoving);
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
