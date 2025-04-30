using DG.Tweening;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public struct HyenaMoveOrder
{
    public MovableUnit hyena;
    public Vector2Int origin;
    public IEnumerable<Vector2Int> movePath;

    public HyenaMoveOrder(MovableUnit hyena, Vector2Int origin, IEnumerable<Vector2Int> movePath)
    {
        this.hyena = hyena;
        this.origin = origin;
        this.movePath = movePath;
    }

    public override string ToString()
    {
        return $"HyenaMoveOrder(hyena:{hyena.name} at {origin}, path:[{string.Join(",", movePath)}])";
    }
}

public class HyenasManager : MonoBehaviour
{
    public static HyenasManager Instance { get; private set; }

    [Header("Config")]
    [SerializeField] private Transform hyenasParent;
    [SerializeField] private float delayBetweenMoves = 0.3f;
    [SerializeField] private bool moveHyenasSimultaneously = false;
    [Tooltip("Assign a MonoBehaviour that implements IHyenaMoveStrategy.")]
    [SerializeField] private MonoBehaviour moveStrategy;

    [Header("State")]
    [SerializeField] private int currentMoveOrderIndex = 0;
    [SerializeField] private List<HyenaMoveOrder> moveOrders;
    [SerializeField] private Dictionary<MovableUnit, HyenaMoveOrder> moveOrdersByHyena;
    [SerializeField] private bool haltAllRemainingMoves = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;

        Debug.Log("=== HyenasManager initialized and listeners set up. ===");
    }

    public void HandleGameStateChanged(GameState state)
    {
        if (state != GameState.HyenasMoving) return;

        IHyenaMoveStrategy strategy = GetMoveStrategy();
        if (strategy == null)
        {
            GameManager.Instance.OnHyenasFinishMoving();
            return;
        }

        Debug.Log("[HyenasManager] Starting to move hyenas");

        // Iterate over all child hyenas and filter out any that are disabled or missing Unit or MovableUnit components
        List<HyenaUnit> hyenasToMove = GetValidHyenasToMove();

        // Ask the stratgey to calculate move orders
        moveOrders = strategy.CalculateMovementPathForHyenas(hyenasToMove);
        moveOrdersByHyena = moveOrders.ToDictionary(order => order.hyena, order => order);

        currentMoveOrderIndex = 0;
        haltAllRemainingMoves = false;

        if (moveHyenasSimultaneously)
        {
            MoveAllHyenasAtOnce();
        }
        else
        {
            MoveNextHyena();
        }
    }

    private IHyenaMoveStrategy GetMoveStrategy()
    {
        if (moveStrategy == null)
        {
            Debug.LogError("[HyenasManager] Property moveStrategy is not assigned. Hyenas will not move.");
            return null;
        }
        IHyenaMoveStrategy strategy = moveStrategy as IHyenaMoveStrategy;
        if (strategy == null)
        {
            Debug.LogError($"[HyenasManager] Object {moveStrategy.name} is assigned as moveStrategy, but does not implement IHyenaMoveStrategy. Hyenas will not move.");
        }
        return strategy;
    }

    private List<HyenaUnit> GetValidHyenasToMove()
    {
        List<HyenaUnit> validHyenas = new List<HyenaUnit>();
        foreach (Transform child in hyenasParent.transform)
        {
            if (child.gameObject.activeInHierarchy == false) continue;

            var unit = child.GetComponent<HyenaUnit>();
            var hyena = child.GetComponent<MovableUnit>();

            if (unit == null || hyena == null)
            {
                Debug.LogWarning($"[HyenasManager] Hyena {child.name} is missing a HyenaUnit or MovableUnit component.");
                continue;
            }

            validHyenas.Add(unit);
        }
        return validHyenas;
    }

    private void MoveAllHyenasAtOnce()
    {
        if (moveOrders.Count == 0)
        {
            GameManager.Instance.OnHyenasFinishMoving();
            return;
        }

        Sequence allPrepareSeq = DOTween.Sequence();
        Sequence allMovesSeq = DOTween.Sequence();

        int numMoveOrdersCompleted = 0;

        foreach (var moveOrder in moveOrders)
        {
            if (moveOrder.movePath.Any())
            {
                allPrepareSeq.AppendCallback(() => { moveOrder.hyena.PrepareForMove(); });

                Sequence moveSeq = DOTween.Sequence();
                moveSeq
                    //.AppendInterval(Random.Range(0, 0.3f))
                    .AppendCallback(() =>
                    {
                        moveOrder.hyena.MoveAlongPath(moveOrder.movePath, () =>
                        {
                            numMoveOrdersCompleted++;

                            if (numMoveOrdersCompleted >= moveOrders.Count)
                            {
                                GameManager.Instance.OnHyenasFinishMoving();
                            }
                        });
                    });
                allMovesSeq.Join(moveSeq);
            }
            else
            {
                numMoveOrdersCompleted++; // No movement = auto-complete
            }
        }

        if (numMoveOrdersCompleted >= moveOrders.Count)
        {
            GameManager.Instance.OnHyenasFinishMoving();
        }
        else
        {
            allMovesSeq.Prepend(allPrepareSeq); //When moving multiple hyenas at once, we need to prepare all of them before starting the moves (ie, unregister from board position)
            allMovesSeq.Play();
        }
    }

    private void MoveNextHyena()
    {
        if (haltAllRemainingMoves)
        {
            Debug.Log($"[HyenasManager] Halting {moveOrders.Count() - currentMoveOrderIndex} remaining hyena moves. GameManager will not be notified.");
            return;
        }

        if (currentMoveOrderIndex >= moveOrders.Count())
        {
            Debug.Log($"[HyenasManager] All {moveOrders.Count()} movement orders have completed.");
            GameManager.Instance.OnHyenasFinishMoving();
            return;
        }

        Debug.Log($"*** moving hyena {currentMoveOrderIndex + 1}/{moveOrders.Count()}...");

        var moveOrder = moveOrders[currentMoveOrderIndex];
        currentMoveOrderIndex++;

        if (!moveOrder.movePath.Any())
        {
            MoveNextHyena();
        }
        else
        {
            DOTween.Sequence()
                .AppendInterval(delayBetweenMoves)
                .AppendCallback(() => { moveOrder.hyena.PrepareForMove(); })
                .AppendCallback(() => { moveOrder.hyena.MoveAlongPath(moveOrder.movePath, MoveNextHyena); })
                .Play();
        }
    }

    public void HaltAllRemainingHyenaMovesAndDoNotNotifyGameManager()
    {
        haltAllRemainingMoves = true;
    }

    public List<HyenaUnit> GetAllHyenas()
    {
        List<HyenaUnit> hyenas = hyenasParent.transform
            .Cast<Transform>()
            .Where(child => child.gameObject.activeInHierarchy)
            .Select(child => child.GetComponent<HyenaUnit>())
            .Where(unit => unit != null)
            .Where(unit => unit.GetFaction() == Faction.Hyenas)
            .ToList();

        return hyenas;
    }

    #region Debug

    public Dictionary<MovableUnit, HyenaMoveOrder> GetLastMoveOrders()
    {
        return moveOrdersByHyena;
    }

    private Dictionary<MovableUnit, Color> gizmoLineColors = new();
    private Dictionary<MovableUnit, Vector3> gizmoOffsets = new();

    private void OnDrawGizmos()
    {
        // iterate over all move orders and for each draw a line connecting the points in movePath
        if (moveOrders == null || moveOrders.Count == 0) return;
        Gizmos.color = Color.red;
        foreach (var moveOrder in moveOrders)
        {
            if (moveOrder.movePath == null || !moveOrder.movePath.Any()) continue;

            if (!gizmoLineColors.ContainsKey(moveOrder.hyena))
            {
                gizmoLineColors[moveOrder.hyena] = RandomColor();
                gizmoOffsets[moveOrder.hyena] = RandomOffset();
            }

            Gizmos.color = gizmoLineColors[moveOrder.hyena];

            Vector3 previousPoint = BoardManager.Instance.BoardCellToWorld(moveOrder.origin);
            foreach (var cell in moveOrder.movePath)
            {
                Vector3 currentPoint = BoardManager.Instance.BoardCellToWorld(cell);
                Vector3 offset = gizmoOffsets[moveOrder.hyena];
                Gizmos.DrawLine(previousPoint + offset, currentPoint + offset);
                previousPoint = currentPoint;
                if (cell == moveOrder.movePath.Last())
                {
                    Gizmos.DrawCube(currentPoint + offset, new Vector3(0.1f, 0.1f, 0.1f));
                }
            }
        }
    }

    private Color RandomColor()
    {
        return new Color(Random.value, Random.value, Random.value, 1f);
    }

    private Vector3 RandomOffset()
    {
        return new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(-0.3f, 0.3f), 0f);
    }

    #endregion
}
