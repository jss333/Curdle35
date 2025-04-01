#nullable enable

using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class Unit : MonoBehaviour
{
    [Header("State")]
    [SerializeField] private Vector2Int boardPosition;

    void Start()
    {
        // Determine logical board position given current world position
        boardPosition = GridHelper.Instance.WorldToGrid(transform.position);

        // Set unit's workd position to be the correct one for the given board position
        this.transform.position = GridHelper.Instance.GridToWorld(boardPosition);
    }

    public Vector2Int GetBoardPosition()
    {
        return boardPosition;
    }

    public void SetBoardPosition(Vector2Int pos)
    {
        boardPosition = pos;
    }
}