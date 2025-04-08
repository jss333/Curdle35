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
                    Debug.Log($"--Done with step for {unit.name} at {cell}");
                    HandlePotentialInteractionsInCell(cell, cell == path.Last());
                })
             ); 
        }

        moveSequence.SetEase(pathEaseType);
        moveSequence.OnComplete(() =>
        {
            // After the sequence is done, update the unit's board position to the last cell
            Debug.Log($"--Done with movement for {unit.name}");
            if(unit.IsAlive())
            {
                unit.UpdateBoardPositionAfterMove(path.Last());
            }
            moveDoneCallback?.Invoke();
        });

        Debug.Log($"Moving {unit.name} to {path.Last()}");
        moveSequence.Play();
    }

    private void HandlePotentialInteractionsInCell(Vector2Int cell, bool isLastCellInPath)
    {
        BoardManager boardMngr = BoardManager.Instance;
        Faction myFaction = unit.GetFaction();

        if (boardMngr.CellHasUnit(cell))
        {
            Unit otherUnit = boardMngr.GetUnitAt(cell);
            Faction otherFaction = otherUnit.GetFaction();

            //if cell is occupied by an unit in the same faction: log an error if this is the last cell in the path
            if (otherFaction == myFaction && isLastCellInPath)
            {
                Debug.LogError($"Error when moving {unit.name}: destination cell {cell} is occupied by a unit of the same faction: {otherUnit.name}");
            }

            //if cats are moving, destroy the other unit and continue the movement
            if(myFaction == Faction.Cats && otherFaction == Faction.Hyenas)
            {
                Debug.Log($"{unit.name} killed {otherUnit.name} at {cell}");
                otherUnit.Die();
            }

            //if hyenas are moving, damage the other unit, destroy the hyena and stop movement
            if (myFaction == Faction.Hyenas && otherFaction == Faction.Cats)
            {
                Debug.Log($"{unit.name} damaged {otherUnit.name} at {cell}, and then died");
                otherUnit.TakeDamage(1);
                unit.Die();
                // TODO: stop movement
            }
        }

        //if after any interactions the cell is empty, claim it for the unit that moved
        if (!boardMngr.CellHasUnit(cell))
        {
            boardMngr.ClaimCell(cell, myFaction);
        }
    }
}
