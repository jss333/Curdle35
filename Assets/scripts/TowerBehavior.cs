using System;
using System.Collections.Generic;
using UnityEngine;

public class TowerBehavior
{
    private Vector3Int towerCell;
    public int health;
    public bool isHQTower;

    private HashSet<Vector3Int> cellsInTowerArea = new HashSet<Vector3Int>();

    public TowerBehavior(Vector3Int towerCell, int health, bool isHQTower)
    {
        this.towerCell = towerCell;
        this.health = health;
        this.isHQTower = isHQTower;

        Vector3Int offset = Vector3Int.zero;
        for (offset.x = -1; offset.x <= 1; offset.x++)
        {
            for (offset.y = -1; offset.y <= 1; offset.y++)
            {
                cellsInTowerArea.Add(towerCell + offset);
            }
        }
        LogUtils.LogEnumerable("cellsInTowerArea: ", cellsInTowerArea);
    }

    public bool AreaIncludesCellAtPosition(Vector3Int cell)
    {
        return cellsInTowerArea.Contains(cell);
    }

    public HashSet<Vector3Int> getCellsInTowerArea()
    {
        return cellsInTowerArea;
    }
}
