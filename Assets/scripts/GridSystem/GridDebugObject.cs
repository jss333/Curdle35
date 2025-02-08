using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GridDebugObject : MonoBehaviour
{
    [SerializeField] private TMP_Text textMeshPro;

    private GridObject gridObject;

    private int resources;
    
    public void SetGridObject(GridObject gridObject)
    {
        resources = Random.Range(1, 4);
    }

    private void Update()
    {
        textMeshPro.text = resources.ToString();
    }
}
