using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridObject
{
    private GridSystem gridSystem;
    private GridPosition gridPosition;
    private List<OldUnit> unitList;

    public GridObject(GridSystem gridSystem, GridPosition gridPosition)
    {
        this.gridSystem = gridSystem;
        this.gridPosition = gridPosition;
        unitList = new List<OldUnit>();
    }

    public override string ToString()
    {
        string unitString = "";
        foreach (OldUnit unit in unitList)
        {
            unitString += unit + "\n";
        }

        return gridPosition.ToString() + "\n" + unitString;
    }

    public void AddUnit(OldUnit unit)
    {
        unitList.Add(unit);
    }

    public void RemoveUnit(OldUnit unit)
    {
        unitList.Remove(unit);
    }

    public List<OldUnit> GetUnitList()
    {
        return unitList;
    }

    public bool HasAnyUnit()
    {
        return unitList.Count > 0;
    }
}
