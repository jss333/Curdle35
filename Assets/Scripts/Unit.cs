#nullable enable

using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class Unit : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private float moveSpeed = 4f;

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

    public IEnumerator MoveToCell(Vector2Int destination)
    {
        GameManager.Instance.SetState(GameState.UnitIsMoving);

        Vector3 targetWorld = GridHelper.Instance.GridToWorld(destination);
        while (Vector3.Distance(transform.position, targetWorld) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetWorld, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetWorld;
        boardPosition = destination;

        GameManager.Instance.SetState(GameState.PlayerInput);
    }
}