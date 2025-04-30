using UnityEngine;
using System;
using UnityEditor.Timeline;

public class Unit : MonoBehaviour
{
    public event System.Action<Unit> OnUnitDeath;

    [Header("Config")]
    [SerializeField] private Faction faction;
    [SerializeField] private bool isStructure;
    [SerializeField] private int maxHealth = 1;
    [SerializeField] private bool notifyDeath = false;
    [SerializeField] private bool initialFacingRight = true;

    [Header("State")]
    [SerializeField] protected Vector2Int boardCellPosition;
    [SerializeField] bool traveling = false;
    [SerializeField] private int currentHealth;
    [SerializeField] private bool isAlive = true;
    [SerializeField] private bool facingRight;

    [Header("State SFX - set via code at startup")]
    [SerializeField] private SFX getsHitSFX = SFX.NONE;
    [SerializeField] private SFX dieSFX = SFX.NONE;

    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        if (TryGetComponent<Animator>(out animator))
        {
            animator.Play("Idle", 0, UnityEngine.Random.Range(0f, 1f));
            animator.speed = UnityEngine.Random.Range(0.95f, 1.05f);
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    protected virtual void Start()
    {
        // Determine logical board cell given current world position
        BoardManager boardMngr = BoardManager.Instance;
        boardCellPosition = boardMngr.WorldToBoardCell(transform.position);
        Debug.Log($"Unit {gameObject.name} located at {transform.position} is on board cell {boardCellPosition}");
        boardMngr.RegisterUnitInCell(this, boardCellPosition);
        boardMngr.ClaimCellForFaction(boardCellPosition, faction);

        // Set unit's world position to be the correct one for the given board cell (ie, right in the middle of the cell)
        this.transform.position = boardMngr.BoardCellToWorld(boardCellPosition);

        facingRight = initialFacingRight;

        currentHealth = maxHealth;

        // Figure out the SFX based on Faction
        if (faction == Faction.Cats && !isStructure)
        {
            getsHitSFX = SFX.Player_Unit_Is_Hit;
            dieSFX = SFX.Player_Unit_Dies;
        }
        else if (faction == Faction.Cats && isStructure)
        {
            getsHitSFX = SFX.Turret_Is_Hit;
            dieSFX = SFX.Turret_Is_Destroyed;
        }
        else if (faction == Faction.Hyenas)
        {
            getsHitSFX = SFX.NONE;
            dieSFX = SFX.Hyena_Dies;
        }
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
        if (traveling)
        {
            Debug.LogWarning($"Unit {gameObject.name} is currently traveling, returning its last known board position: {boardCellPosition}");
        }

        return boardCellPosition;
    }

    public void FaceTowards(Vector2Int destination)
    {
        int horizontalDirection = Math.Sign(destination.x - boardCellPosition.x);
        bool shouldFlipSprite = (horizontalDirection > 0 && !facingRight) || (horizontalDirection < 0 && facingRight);
        Debug.Log($"Unit {gameObject.name} facing towards {destination} from {boardCellPosition}. Should flip sprite: {shouldFlipSprite}");
        if (shouldFlipSprite)
        {
            spriteRenderer.flipX = !spriteRenderer.flipX;
            facingRight = !facingRight;
        }
    }

    public int GetOrthogonalDistance(Unit otherUnit)
    {
        return GetOrthogonalDistance(otherUnit.GetBoardPosition());
    }

    public int GetOrthogonalDistance(Vector2Int cell)
    {
        Vector2Int diff = cell - GetBoardPosition();
        return System.Math.Abs(diff.x) + System.Math.Abs(diff.y);
    }

    public void UpdateBoardPositionBeforeMove()
    {
        BoardManager.Instance.UnregisterUnitFromCell(this, boardCellPosition);
        traveling = true;
    }

    public void UpdateBoardPositionAfterMove(Vector2Int newCell)
    {
        BoardManager.Instance.RegisterUnitInCell(this, newCell);
        boardCellPosition = newCell;
        traveling = false;
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public void TakeDamage(int dmg)
    {
        currentHealth = System.Math.Max(0, currentHealth - dmg);
        if(currentHealth == 0)
        {
            Die();
        }
        else
        {
            SoundsManager.Instance.PlaySFX(getsHitSFX);
        }
    }

    public void Die()
    {
        isAlive = false;

        SoundsManager.Instance.PlaySFX(dieSFX);

        if (notifyDeath)
        {
            OnUnitDeath?.Invoke(this);
        }

        if (!traveling)
        {
            BoardManager.Instance.UnregisterUnitFromCell(this, boardCellPosition);
        }
        traveling = false;
        
        Destroy(gameObject);
    }

    public bool IsAlive()
    {
        return isAlive;
    }
}