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

    public CellData(int resourceValue)
    {
        this.resourceValue = resourceValue;
    }

    public bool IsVoidCell()
    {
        return resourceValue == -1;
    }
}
