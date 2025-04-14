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

    public void HandlePlayerResourcesChanged(int newResources)
    {
        RefreshButtonInteractability();
    }

    protected override bool CalculateInteractabilityDuringPlayerInput()
    {
        return ResourcesManager.Instance.PlayerResources >= TurretsManager.Instance.GetTurretBuildCost();
    }
}
