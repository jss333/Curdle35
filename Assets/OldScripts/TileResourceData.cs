using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


[CreateAssetMenu(fileName = "TileResourceData", menuName = "Scriptable Objects/TileResourceData")]
public class TileResourceData : ScriptableObject
{
    public TileBase[] tiles;
    public int score = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
