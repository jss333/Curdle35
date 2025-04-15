using UnityEngine;

public class SelectableTurret: SelectableUnit
{
    private Turret turret;

    public override void Start()
    {
        base.Start();

        if (!TryGetComponent<Turret>(out turret))
        {
            Debug.LogError("SelectableTurret requires a Turret component.");
        }
    }

    public override void ShowSelectedEffect()
    {
        base.ShowSelectedEffect();

        BoardManager.Instance.ShowShootingRange(turret);
    }

    public override void RemoveSelectedEffect()
    {
        base.RemoveSelectedEffect();

        BoardManager.Instance.ClearAllRanges();
    }
}
