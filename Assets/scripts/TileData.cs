using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


[CreateAssetMenu(fileName = "TileData", menuName = "Scriptable Objects/TileData")]
public class TileData : ScriptableObject
{
    public TileBase[] tiles;
        public enum Type{
        None,
        Water,
        Grass,
        Desert,
        Dirt,
        Mountain,
        Tundra,
        Forest,
        Jungle,
        Corruption0, //corruption needs to come last
        Corruption1, //mod of Corruption0, still temporary?
        Corruption2,
    }
    private List<UnitController> occupants = new List<UnitController>();
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
