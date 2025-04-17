using UnityEngine;
using TMPro;

public class BuildCommandButton : UnitCommandButton
{
    [Header("Config - Label")]
    [SerializeField] private string buildLabel = "Build ($COST)";

    protected override void Start()
    {
        base.Start();

        ResourcesManager.Instance.OnPlayerResourcesChanged += HandlePlayerResourcesChanged;
    }

    private void HandlePlayerResourcesChanged(int newResources)
    {
        RefreshButtonInteractabilityAndLabelIfVisible();
    }

    protected override bool CalculateInteractabilityDuringPlayerInput()
    {
        return ResourcesManager.Instance.PlayerResources >= TurretsManager.Instance.GetTurretBuildCost();
    }

    protected override string CalculateLabel()
    {
        return buildLabel.Replace("$COST", TurretsManager.Instance.GetTurretBuildCost().ToString());
    }

    public override CommandType GetCommandType()
    {
        return CommandType.Build;
    }

    public override void DoCommandSelection(SelectableUnit selectedUnit)
    {
        if(UnitHasBuildRange(selectedUnit, out var buildRange))
        {
            BoardManager.Instance.ShowBuildRange(buildRange);
        }
    }

    public override void DoCommandClear()
    {
        BoardManager.Instance.ClearAllRanges();
    }

    public override bool TryExecuteCommand(SelectableUnit selectedUnit, Vector2Int clickedCell)
    {
        if(UnitHasBuildRange(selectedUnit, out var buildRange))
        {
            if (buildRange.IsCellInRange(clickedCell))
            {
                TurretsManager.Instance.BuildTurretAt(clickedCell);
                UnitSelectionManager.Instance.ClearCommand();

                return true;
            }
        }

        return false;
    }

    private bool UnitHasBuildRange(SelectableUnit selectedUnit, out BuildRange buildRange)
    {
        if (selectedUnit == null)
        {
            Debug.LogError($"No selected unit for Build command.");
            buildRange = null;
            return false;
        }

        if (!selectedUnit.TryGetComponent<BuildRange>(out buildRange))
        {
            Debug.LogError($"Selected unit {selectedUnit.name} does not have a MovementRange component.");
            return false;
        }

        return true;
    }
}
