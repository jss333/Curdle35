using UnityEngine;
using TMPro;

public class BuildCommandButton : UnitCommandButton
{
    [Header("Config - Label")]
    [SerializeField] private string buildLabel = "Build ($COST)";

    private string calculatedLabel;

    protected override void Start()
    {
        base.Start();

        ResourcesManager.Instance.OnPlayerResourcesChanged += HandlePlayerResourcesChanged;

        calculatedLabel = buildLabel.Replace("$COST", TurretsManager.Instance.GetTurretBuildCost().ToString());
    }

    private void HandlePlayerResourcesChanged(int newResources)
    {
        RefreshButtonInteractabilityAndLabelIfVisible();
    }

    protected override bool CommandIsAvailableToUnit(SelectableUnit unit)
    {
        return unit.TryGetComponent<Builder>(out Builder builder) && builder.enabled;
    }

    protected override void CalculateInteractabilityAndLabel(out bool interactableDuringPlayerInput, out string label)
    {
        interactableDuringPlayerInput = ResourcesManager.Instance.PlayerResources >= TurretsManager.Instance.GetTurretBuildCost();
        label = calculatedLabel;
    }

    public override void DoCommandSelection(SelectableUnit selectedUnit)
    {
        if(UnitIsBuilder(selectedUnit, out Builder builder))
        {
            BoardManager.Instance.ShowBuildRange(builder);
        }
    }

    public override void DoCommandClear()
    {
        BoardManager.Instance.ClearAllRanges();
    }

    public override bool TryExecuteCommand(SelectableUnit selectedUnit, Vector2Int clickedCell)
    {
        if(UnitIsBuilder(selectedUnit, out Builder builder))
        {
            if (builder.IsCellInRange(clickedCell))
            {
                SoundsManager.Instance.PlaySFX(SFX.Build_Confirm);

                TurretsManager.Instance.BuildTurretAt(clickedCell);
                UnitSelectionManager.Instance.ClearCommand();

                return true;
            }
        }

        return false;
    }

    private bool UnitIsBuilder(SelectableUnit selectedUnit, out Builder builder)
    {
        if (selectedUnit == null)
        {
            Debug.LogError($"No selected unit for Build command.");
            builder = null;
            return false;
        }

        if (!selectedUnit.TryGetComponent<Builder>(out builder))
        {
            Debug.LogError($"Selected unit {selectedUnit.name} does not have a Builder component.");
            return false;
        }

        return true;
    }
}
