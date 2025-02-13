using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public class HyenaController : UnitController
{

    bool attackedThisTurn = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void Start() 
    {
        base.team = Team.hyena;
        base.Start();
    }
}
