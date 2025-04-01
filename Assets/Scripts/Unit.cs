#nullable enable

using System.Collections.Generic;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Unit : MonoBehaviour
{
    private SpriteRenderer spriteRenderer = null!;

    [Header("Config")]
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private float moveSpeed = 4f;

    [Header("State")]
    [SerializeField] private Color originalColor;
    [SerializeField] private Vector2Int boardPosition;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;

        // Determine logical board position given current world position
        boardPosition = GridHelper.Instance.WorldToGrid(transform.position);

        // Set unit's workd position to be the correct one for the given board position
        this.transform.position = GridHelper.Instance.GridToWorld(boardPosition);
    }

    public Vector2Int GetBoardPosition()
    {
        return boardPosition;
    }

    public void ShowSelected()
    {
        spriteRenderer.color = highlightColor;
    }

    public void ShowDeselected()
    {
        spriteRenderer.color = originalColor;
    }

    public PlayerMovableUnit? GetPlayerMovableUnit()
    {
        return GetComponent<PlayerMovableUnit>();
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