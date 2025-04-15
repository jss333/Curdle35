using UnityEngine;

public class Unit : MonoBehaviour
{
    private Animator animator;

    [Header("Config")]
    [SerializeField] private Faction faction;
    [SerializeField] private bool isStructure;
    [SerializeField] private int maxHealth = 1;

    [Header("State")]
    [SerializeField] private Vector2Int boardCellPosition;
    [SerializeField] private int currentHealth;
    [SerializeField] private bool isAlive = true;

    void Start()
    {
        if(TryGetComponent<Animator>(out animator))
        {
            animator.Play("Idle", 0, Random.Range(0f, 1f));
            animator.speed = Random.Range(0.95f, 1.05f);
        }

        // Determine logical board cell given current world position
        BoardManager boardMngr = BoardManager.Instance;
        boardCellPosition = boardMngr.WorldToBoardCell(transform.position);
        Debug.Log($"Unit {gameObject.name} located at {transform.position} is on board cell {boardCellPosition}");
        boardMngr.RegisterUnitInCell(this, boardCellPosition);
        boardMngr.ClaimCellForFaction(boardCellPosition, faction);

        // Set unit's world position to be the correct one for the given board cell (ie, right in the middle of the cell)
        this.transform.position = boardMngr.BoardCellToWorld(boardCellPosition);

        currentHealth = maxHealth;
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
        return boardCellPosition;
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

    public void UpdateBoardPositionAfterMove(Vector2Int newCell)
    {
        BoardManager.Instance.UpdateUnitFromOldToNewCell(this, boardCellPosition, newCell);
        boardCellPosition = newCell;
    }

    public void TakeDamage(int dmg)
    {
        currentHealth = System.Math.Max(0, currentHealth - dmg);
        if(currentHealth == 0)
        {
            Die();
        }
    }

    public void Die()
    {
        isAlive = false;
        BoardManager.Instance.UnregisterUnitFromCell(this, boardCellPosition);
        Destroy(gameObject);
    }

    public bool IsAlive()
    {
        return isAlive;
    }
}