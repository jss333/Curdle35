using UnityEngine;
using TMPro;

public class BuildCommandButton : UnitCommandButton
{
    protected override void Start()
    {
        base.Start();

        ResourcesManager.Instance.OnPlayerResourcesChanged += HandlePlayerResourcesChanged;
        RefreshButtonInteractability();

        TextMeshProUGUI label = gameObject.GetComponentInChildren<TextMeshProUGUI>();
        if (label != null)
        {
            label.text = $"Build ({TurretsManager.Instance.GetTurretBuildCost()})";
        }
    }

    private void HandlePlayerResourcesChanged(int newResources)
    {
        RefreshButtonInteractability();
    }

    protected override bool CalculateInteractabilityDuringPlayerInput()
    {
        return ResourcesManager.Instance.PlayerResources >= TurretsManager.Instance.GetTurretBuildCost();
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
            if (buildRange.IsCellInsideRange(clickedCell))
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
