using UnityEngine;

public class HyenaUnit : Unit
{
    protected override void Start()
    {
        base.Start();

        FaceTowards(BoardManager.Instance.GetCenterCell());
    }
}
