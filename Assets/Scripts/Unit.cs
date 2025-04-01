#nullable enable

using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class Unit : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private Faction faction;

    [Header("State")]
    [SerializeField] private Vector2Int boardPosition;

    void Start()
    {
        // Determine logical board position given current world position
        boardPosition = GridHelper.Instance.WorldToGrid(transform.position);
        BoardManager.Instance.RegisterUnitPos(this, boardPosition);

        // Set unit's workd position to be the correct one for the given board position
        this.transform.position = GridHelper.Instance.GridToWorld(boardPosition);
    }

    public Faction GetFaction()
    {
        return faction;
    }

    public Vector2Int GetBoardPosition()
    {
        return boardPosition;
    }

    public void UpdateBoardPosition(Vector2Int newPos)
    {
        BoardManager.Instance.UpdateUnitPos(this, boardPosition, newPos);
        boardPosition = newPos;
    }
}