using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


[CreateAssetMenu(fileName = "TileTerritoryData", menuName = "Scriptable Objects/TileTerritoryData")]
public class TileTerritoryData : ScriptableObject
{
    public TileBase[] tiles;
    public enum Type{
        None,
        Cats,
        Hyena,
        Impassable,
    }
    public Type type;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
