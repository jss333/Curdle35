using DG.Tweening;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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
    [SerializeField] private float delayBeforeEachHyenaMove = 0.2f;
    [SerializeField] private float delayBetweenGroupMoves = 0.3f;
    [SerializeField] private bool moveHyenasInGroups = false;
    [SerializeField] private int numHyenaGroups = 6;
    [Tooltip("Assign a MonoBehaviour that implements IHyenaMoveStrategy.")]
    [SerializeField] private MonoBehaviour moveStrategy;
    [SerializeField] private int hyenaMoveDistance;

    [Header("State")]
    //[SerializeField] private List<HyenaMoveOrder> moveOrders;
    [SerializeField] private List<List<HyenaMoveOrder>> partitionedOrders;
    [SerializeField] private int currentPartitionIndex;
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

        List<HyenaMoveOrder> moveOrders = strategy.CalculateMovementPathForHyenas(GetAllActiveHyenas())
            .Where(order => order.movePath != null && order.movePath.Any())
            .ToList();

        if (moveOrders.Count == 0)
        {
            GameManager.Instance.OnHyenasFinishMoving();
            return;
        }

        allHistoricalMoveOrders.AddRange(moveOrders); // For drawing debug gizmos

        // Call prepareForMove to prepare each hyena before moving
        foreach (var moveOrder in moveOrders)
        {
            moveOrder.hyena.PrepareForMove();
        }

        int numPartitions = moveHyenasInGroups ? numHyenaGroups : moveOrders.Count;
        partitionedOrders = PartitionOrders(moveOrders, numPartitions);
        foreach (var partition in partitionedOrders)
        {
            LogUtils.LogEnumerable("[HyenasManager] Partitioned move order", partition);
        }

        currentPartitionIndex = 0;
        MoveNextPartition();
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

    private static List<List<HyenaMoveOrder>> PartitionOrders(List<HyenaMoveOrder> moveOrders, int numPartitions)
    {
        List<List<HyenaMoveOrder>> partitionedOrders = new();

        // If there are fewer move orders than groupings, just return each order in its own partition.
        if (moveOrders.Count <= numPartitions)
        {
            foreach (var order in moveOrders)
            {
                partitionedOrders.Add(new List<HyenaMoveOrder> { order });
            }

            return partitionedOrders;
        }

        // Otherwise splits moveOrders as evenly as possible into numPartitions partitions, filling the later partitions first.
        // So 10 move orders partitioned into 3 would yield: 3-3-4
        int a = moveOrders.Count / numPartitions;
        int b = a + 1;
        int y = moveOrders.Count % numPartitions;
        int x = numPartitions - y;

        for (int i = 0; i < x; i++)
        {
            partitionedOrders.Add(moveOrders.GetRange(i * a, a));
        }
        for (int i = 0; i < y; i++)
        {
            partitionedOrders.Add(moveOrders.GetRange((x * a) + (i * b), b));
        }

        return partitionedOrders;
    }

    private void MoveNextPartition()
    {
        if (currentPartitionIndex >= partitionedOrders.Count)
        {
            Debug.Log("[HyenasManager] All hyenas have been moved. Notifying GameManager.");
            GameManager.Instance.OnHyenasFinishMoving();
            return;
        }

        List<HyenaMoveOrder> currentPartition = partitionedOrders[currentPartitionIndex];
        Debug.Log($"[HyenasManager] Moving partition {currentPartitionIndex + 1}/{partitionedOrders.Count} with {currentPartition.Count} hyenas.");

        int numHyenasMoved = 0;

        void MoveCompleteCallback()
        {
            numHyenasMoved++;
            if (numHyenasMoved >= currentPartition.Count)
            {
                currentPartitionIndex++;
                MoveNextPartition(); // Move the next partition after all hyenas in this one have moved.
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

        Sequence partitionMoveSeq = DOTween.Sequence();
        for (int i = 0; i < currentPartition.Count; i++)
        {
            var moveOrder = currentPartition[i];

            Sequence hyenaMoveSeq = DOTween.Sequence();
            hyenaMoveSeq
                .AppendInterval(i * delayBeforeEachHyenaMove)
                .AppendCallback(() => ExecuteMoveOrderIfNotHalted(moveOrder));
            partitionMoveSeq.Join(hyenaMoveSeq); // Hyenas in the same partition start moving simultaneously, each with their own initial delay
        }

        if (currentPartitionIndex != 0)
        { 
            partitionMoveSeq.PrependInterval(delayBetweenGroupMoves);
        }

        partitionMoveSeq.Play();
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
