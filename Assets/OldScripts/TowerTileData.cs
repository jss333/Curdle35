using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


[CreateAssetMenu(fileName = "TowerTileData", menuName = "Scriptable Objects/TowerTileData")]
public class TowerTileData : ScriptableObject
{
    public TileBase[] tiles;
    public enum Type{
        Tower1,
        Tower2,
        Tower3,
    }
    public Type type;
    int owningPlayer; //0 for local, 1 for AI/opponent, 2 for neutral?

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
