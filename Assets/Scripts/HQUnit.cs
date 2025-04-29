using UnityEngine;

public class HQUnit : Unit
{
    protected override void Start()
    {
        base.Start();

        // After we're done loading, signal BoardManager to calculate distance to HQ (used by Hyenas movement algorithm)
        BoardManager.Instance.ComputeMinDistToHQ(GetBoardPosition());
    }
}
