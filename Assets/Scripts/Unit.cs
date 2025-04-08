using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class Unit : MonoBehaviour
{
    private Animator animator;

    [Header("Config")]
    [SerializeField] private Faction faction;
    [SerializeField] private bool isStructure;

    [Header("State")]
    [SerializeField] private Vector2Int boardPosition;

    void Start()
    {
        animator = GetComponent<Animator>();
        if(animator != null)
        {
            animator.Play("Idle", 0, Random.Range(0f, 1f));
            animator.speed = Random.Range(0.95f, 1.05f);
        }

        // Determine logical board position given current world position
        boardPosition = GridHelper.Instance.WorldToGrid(transform.position);
        BoardManager.Instance.RegisterUnitPos(this, boardPosition);
        BoardManager.Instance.ClaimCell(boardPosition, faction);

        // Set unit's world position to be the correct one for the given board position
        this.transform.position = GridHelper.Instance.GridToWorld(boardPosition);
    }

    public Faction GetFaction()
    {
        return faction;
    }

    public bool IsStructure()
    {
        return isStructure;
    }

    public Vector2Int GetBoardPosition()
    {
        return boardPosition;
    }

    public void UpdateBoardPositionAfterMove(Vector2Int newPos)
    {
        BoardManager.Instance.UpdateUnitPosRegister(this, boardPosition, newPos);
        boardPosition = newPos;
    }

    public void Die()
    {
        BoardManager.Instance.UnregisterUnitPos(this, boardPosition);
        Destroy(gameObject);
        // TODO add death VFX and SFX
    }
}