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

    public void MoveAlongPath(IEnumerable<Vector2Int> path, Action moveDoneCallback)
    {
        Vector2Int origin = unit.GetBoardPosition();

        EnsureSpriteIsFacingDirectionOfDestination(origin, path.Last());

        DoMoveAlongPath(path, moveDoneCallback);
    }

    private void EnsureSpriteIsFacingDirectionOfDestination(Vector2Int origin, Vector2Int destination)
    {
        int horizontalDirection = Math.Sign(destination.x - origin.x);
        bool shouldFlipSprite = (horizontalDirection > 0 && !facingRight) || (horizontalDirection < 0 && facingRight);
        if (shouldFlipSprite)
        {
            spriteRenderer.flipX = !spriteRenderer.flipX;
            facingRight = !facingRight;
        }
    }

    private void DoMoveAlongPath(IEnumerable<Vector2Int> path, Action moveDoneCallback)
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
            unit.UpdateBoardPositionAfterMove(path.Last());
            moveDoneCallback?.Invoke();
        });

        moveSequence.Play();
    }

    private void ClaimCellUnderUnit(Vector2Int cell)
    {
        BoardManager.Instance.ClaimCell(cell, unit.GetFaction());
    }
}
