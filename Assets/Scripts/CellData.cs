using UnityEngine;

public enum Faction
{
    None,
    Cats,
    Hyenas
}

public class CellData
{
    public int resourceValue;         // -1 for void
    public Faction owner = Faction.None;
    public int minDistToHQ = int.MaxValue;

    public CellData(int resourceValue)
    {
        this.resourceValue = resourceValue;
    }

    public CellData(int resourceValue, Faction owner)
    {
        this.resourceValue = resourceValue;
        this.owner = owner;
    }

    public bool IsVoidCell()
    {
        return resourceValue == -1;
    }
}
