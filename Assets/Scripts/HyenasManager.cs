using DG.Tweening;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using static LinqExtensions;

public struct HyenaMoveOrder
{
    public MovableUnit hyena;
    public Vector2Int origin;
    public IEnumerable<Vector2Int> movePath;

    public HyenaMoveOrder(HyenaUnit hyena, IEnumerable<Vector2Int> movePath)
    {
        this.hyena = hyena.GetComponent<MovableUnit>();
        this.origin = hyena.GetBoardPosition();
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

    public int HyenaMoveDistance => hyenaMoveDistance;

    [Header("Config")]
    [SerializeField] private Transform hyenasParent;
    [SerializeField] private float maxRandomDelayBeforeEachHyenaMove = 0.2f;
    [SerializeField] private float delayBetweenGroupMoves = 0.3f;
    [SerializeField] private bool moveHyenasInGroups = false;
    [SerializeField] private int numHyenaGroups = 6;
    [Tooltip("Assign a MonoBehaviour that implements IHyenaMoveStrategy.")]
    [SerializeField] private MonoBehaviour moveStrategy;
    [SerializeField] private int hyenaMoveDistance;

    [Header("State")]
    [SerializeField] private List<HyenaMoveOrder> moveOrders;
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

    public List<HyenaUnit> GetAllActiveHyenas()
    {
        List<HyenaUnit> hyenas = hyenasParent.transform
            .ChildrenWithComponent<HyenaUnit>(activeOnly: true)
            .ToList();

        return hyenas;
    }

    public void HandleGameStateChanged(GameState state)
    {
        if (state != GameState.HyenasMoving) return;

        if (!TryGetMoveStrategy(out IHyenaMoveStrategy strategy))
        {
            GameManager.Instance.OnHyenasFinishMoving();
            return;
        }

        Debug.Log($"[HyenasManager] Starting to move hyenas with strategy {moveStrategy.name}");

        moveOrders = strategy.CalculateMovementPathForHyenas(GetAllActiveHyenas())
            .Where(order => order.movePath != null && order.movePath.Any())
            .ToList();

        if (moveOrders.Count == 0)
        {
            GameManager.Instance.OnHyenasFinishMoving();
            return;
        }

        allHistoricalMoveOrders.AddRange(moveOrders); // For drawing debug gizmos

        if (moveHyenasInGroups)
        {
            MoveHyenasInGroups(numHyenaGroups);
        }
        else
        {
            MoveHyenasInGroups(moveOrders.Count); 
        }
    }

    private bool TryGetMoveStrategy(out IHyenaMoveStrategy strategy)
    {
        if (moveStrategy == null)
        {
            Debug.LogError("[HyenasManager] Property moveStrategy is not assigned. Hyenas will not move.");
            strategy = null;
            return false;
        }

        if (moveStrategy is not IHyenaMoveStrategy)
        {
            Debug.LogError($"[HyenasManager] Object {moveStrategy.name} is assigned as moveStrategy, but does not implement IHyenaMoveStrategy. Hyenas will not move.", moveStrategy);
            strategy = null;
            return false;
        }

        strategy = moveStrategy as IHyenaMoveStrategy;
        return true;
    }

    private void MoveHyenasInGroups(int numGroupings)
    {
        List<List<HyenaMoveOrder>> partitionedOrders = PartitionOrders(numGroupings);

        int numMoveOrdersCompleted = 0;
        void MoveCompleteCallback()
        {
            numMoveOrdersCompleted++;
            if (numMoveOrdersCompleted >= moveOrders.Count)
            {
                GameManager.Instance.OnHyenasFinishMoving();
            }
        }

        void ExecuteMoveOrderIfNotHalted(HyenaMoveOrder moveOrder)
        {
            if (haltAllRemainingMoves)
            {
                Debug.Log($"[HyenasManager] All hyena movement has been halted. Won't move {moveOrder.hyena.name}.");
                return;
            }
            moveOrder.hyena.MoveAlongPath(moveOrder.movePath, MoveCompleteCallback);
        }

        Sequence allPrepareSeq = DOTween.Sequence();
        Sequence allMovesSeq = DOTween.Sequence();

        foreach (var partition in partitionedOrders)
        {
            Sequence partitionMoveSeq = DOTween.Sequence();

            foreach (var moveOrder in partition)
            {
                allPrepareSeq.AppendCallback(moveOrder.hyena.PrepareForMove);

                Sequence hyenaMoveSeq = DOTween.Sequence();
                hyenaMoveSeq
                    .AppendInterval(Random.Range(0, maxRandomDelayBeforeEachHyenaMove))
                    .AppendCallback(() => ExecuteMoveOrderIfNotHalted(moveOrder));
                partitionMoveSeq.Join(hyenaMoveSeq); // Hyenas in the same partition move simultaneously.
            }

            allMovesSeq.Append(partitionMoveSeq); // Partitions move sequentially.
            allMovesSeq.AppendInterval(delayBetweenGroupMoves);
        }

        allMovesSeq.Prepend(allPrepareSeq); // All move preparation actions should happen before any moves start.
        allMovesSeq.Play();
    }

    private List<List<HyenaMoveOrder>> PartitionOrders(int numGroupings)
    {
        // Splits moveOrders as evenly as possible into numGroupings partitions, filling the later partitions first.
        // So 10 move orders partitioned into 3 groupings would yield: 3-3-4

        List<List<HyenaMoveOrder>> partitionedOrders = new();

        int a = moveOrders.Count / numGroupings;
        int b = a + 1;
        int y = moveOrders.Count % numGroupings;
        int x = numGroupings - y;

        Debug.Log($"CALCS: {moveOrders.Count} = ({a} * {x})  +  ({b} * {y})");

        for (int i = 0; i < x; i++)
        {
            partitionedOrders.Add(moveOrders.GetRange(i * a, a));
        }
        for (int i = 0; i < y; i++)
        {
            Debug.Log($"[HyenasManager] i={i} Range({(x * a) + (i * b)} , {b})");

            partitionedOrders.Add(moveOrders.GetRange((x * a) + (i * b), b));
        }

        foreach (var partition in partitionedOrders)
        {
            LogUtils.LogEnumerable("[HyenasManager] Partitioned move order", partition);
        }

        return partitionedOrders;
    }

    public void HaltAllRemainingHyenaMovesAndDoNotNotifyGameManager()
    {
        haltAllRemainingMoves = true;
    }

    #region Debug

    private List<HyenaMoveOrder> allHistoricalMoveOrders = new();
    private Dictionary<MovableUnit, Color> gizmoLineColors = new();
    private Dictionary<MovableUnit, Vector3> gizmoOffsets = new();

    private void OnDrawGizmos()
    {
        // iterate over all move orders and for each draw a line connecting the points in movePath
        if (allHistoricalMoveOrders == null || allHistoricalMoveOrders.Count == 0) return;

        Gizmos.color = Color.red;
        foreach (var moveOrder in allHistoricalMoveOrders)
        {
            if (moveOrder.movePath == null || !moveOrder.movePath.Any()) continue;

            if (!gizmoLineColors.ContainsKey(moveOrder.hyena))
            {
                gizmoLineColors[moveOrder.hyena] = RandomColor();
                gizmoOffsets[moveOrder.hyena] = RandomOffset();
            }

            DrawMoveOrderPath(moveOrder, gizmoOffsets[moveOrder.hyena], gizmoLineColors[moveOrder.hyena]);
        }
    }

    private Vector3 RandomOffset()
    {
        return new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(-0.3f, 0.3f), 0f);
    }

    private Color RandomColor()
    {
        return new Color(Random.value, Random.value, Random.value, 1f);
    }

    private static void DrawMoveOrderPath(HyenaMoveOrder moveOrder, Vector3 offset, Color color)
    {
        Gizmos.color = color;

        Vector3 previousPoint = BoardManager.Instance.BoardCellToWorld(moveOrder.origin);
        foreach (var cell in moveOrder.movePath)
        {
            Vector3 currentPoint = BoardManager.Instance.BoardCellToWorld(cell);

            Gizmos.DrawLine(previousPoint + offset, currentPoint + offset);
            previousPoint = currentPoint;
            if (cell == moveOrder.movePath.Last())
            {
                Gizmos.DrawCube(currentPoint + offset, new Vector3(0.1f, 0.1f, 0.1f));
            }
        }
    }

    #endregion
}
