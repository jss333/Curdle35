using DG.Tweening;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Unit))]
public class MovableUnit : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private bool initialFacingRight = true;
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private Ease stepEaseType = Ease.Linear;
    [SerializeField] private Ease pathEaseType = Ease.Linear;

    [Header("State")]
    [SerializeField] private bool facingRight;

    private Unit unit;
    private SpriteRenderer spriteRenderer;

    public void Start()
    {
        unit = GetComponent<Unit>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        facingRight = initialFacingRight;
    }

    public void MoveToCell(Vector2Int destination, Action moveDoneCallback)
    {
        Vector2Int origin = unit.GetBoardPosition();

        EnsureSpriteIsFacingDirectionOfMovement(origin, destination);

        IEnumerable<Vector2Int> path = BuildPathToDestination(origin, destination);

        MoveAlongPath(path, moveDoneCallback);
    }

    private void EnsureSpriteIsFacingDirectionOfMovement(Vector2Int origin, Vector2Int destination)
    {
        int horizontalDirection = Math.Sign(destination.x - origin.x);
        bool shouldFlipSprite = (horizontalDirection > 0 && !facingRight) || (horizontalDirection < 0 && facingRight);
        if (shouldFlipSprite)
        {
            spriteRenderer.flipX = !spriteRenderer.flipX;
            facingRight = !facingRight;
        }
    }

    private IEnumerable<Vector2Int> BuildPathToDestination(Vector2Int origin, Vector2Int destination)
    {
        List<Vector2Int> path = new List<Vector2Int>();

        int xVariation = Math.Sign(destination.x - origin.x);
        int yVariation = Math.Sign(destination.y - origin.y);

        Vector2Int nextStep = new Vector2Int(origin.x + xVariation, origin.y + yVariation);
        while (nextStep != destination)
        {
            path.Add(nextStep);
            nextStep = new Vector2Int(nextStep.x + xVariation, nextStep.y + yVariation);
        }
        path.Add(nextStep);

        return path;
    }

    private void MoveAlongPath(IEnumerable<Vector2Int> path, Action moveDoneCallback)
    {
        Sequence moveSequence = DOTween.Sequence();

        float timePerSegment = 1 / moveSpeed;

        foreach (var cell in path)
        {
            Vector3 targetWorld = GridHelper.Instance.GridToWorld(cell);
            moveSequence.Append(
                transform.DOMove(targetWorld, timePerSegment)
                .SetEase(stepEaseType)
                .OnComplete(() =>
                {
                    ClaimCellUnderUnit(cell);
                })
             ); 
        }

        moveSequence.SetEase(pathEaseType);
        moveSequence.OnComplete(() =>
        {
            // After the sequence is done, update the unit's board position to the last cell
            unit.UpdateBoardPosition(path.Last());
            moveDoneCallback?.Invoke();
        });

        moveSequence.Play();
    }

    private void ClaimCellUnderUnit(Vector2Int cell)
    {
        BoardManager.Instance.ClaimCell(cell, unit.GetFaction());
    }
}
