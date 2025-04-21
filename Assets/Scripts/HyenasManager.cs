using DG.Tweening;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public struct HyenaMoveOrder
{
    public MovableUnit hyena;
    public IEnumerable<Vector2Int> movePath;

    public HyenaMoveOrder(MovableUnit hyena, IEnumerable<Vector2Int> movePath)
    {
        this.hyena = hyena;
        this.movePath = movePath;
    }
}

public class HyenasManager : MonoBehaviour
{
    public static HyenasManager Instance { get; private set; }

    [Header("Config")]
    [SerializeField] private float delayBetweenMoves = 0.3f;

    [Header("State")]
    [SerializeField] private int currentMoveOrderIndex = 0;
    [SerializeField] private List<HyenaMoveOrder> moveOrders;
    [SerializeField] private bool haltAllRemainingMoves = false;

    private static HyenaMovementAI MovementAI = new HyenaMovementAI();

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

        Debug.Log("Starting to move hyenas");

        // Iterate over all child hyenas and filter out any that are disabled or missing Unit or MovableUnit components
        List<Unit> hyenasToMove = GetValidHyenasToMove();

        // Ask the AI component to calculate move orders
        moveOrders = MovementAI.CalculateMovementPathForHyenas(hyenasToMove);
        currentMoveOrderIndex = 0;
        haltAllRemainingMoves = false;

        MoveNextHyena();
    }

    private List<Unit> GetValidHyenasToMove()
    {
        List<Unit> validHyenas = new List<Unit>();
        foreach (Transform child in transform)
        {
            if (child.gameObject.activeInHierarchy == false) continue;

            var unit = child.GetComponent<Unit>();
            var hyena = child.GetComponent<MovableUnit>();

            if (unit == null || hyena == null)
            {
                Debug.LogWarning($"Hyena {child.name} is missing an Unit or MovableUnit component.");
                continue;
            }

            validHyenas.Add(unit);
        }
        return validHyenas;
    }

    private void MoveNextHyena()
    {
        if (haltAllRemainingMoves)
        {
            Debug.Log($"Halting {moveOrders.Count() - currentMoveOrderIndex} remaining hyena moves. GameManager will not be notified.");
            return;
        }

        if (currentMoveOrderIndex >= moveOrders.Count())
        {
            Debug.Log($"All {moveOrders.Count()} movement orders have completed.");
            GameManager.Instance.OnHyenasFinishMoving();
            return;
        }

        Debug.Log($"*** moving hyena {currentMoveOrderIndex + 1}/{moveOrders.Count()}...");

        var moveOrder = moveOrders[currentMoveOrderIndex];
        currentMoveOrderIndex++;

        if(moveOrder.movePath.Count() == 0)
        {
            MoveNextHyena();
        }
        else
        {
            DOTween.Sequence()
                .AppendInterval(delayBetweenMoves)
                .AppendCallback(() => { moveOrder.hyena.MoveAlongPath(moveOrder.movePath, MoveNextHyena); })
                .Play();
        }
    }

    public void HaltAllRemainingHyenaMovesAndDoNotNotifyGameManager()
    {
        haltAllRemainingMoves = true;
    }

    public List<Unit> GetAllHyenas()
    {
        List<Unit> hyenas = transform
            .Cast<Transform>()
            .Where(child => child.gameObject.activeInHierarchy)
            .Select(child => child.GetComponent<Unit>())
            .Where(unit => unit != null)
            .Where(unit => unit.GetFaction() == Faction.Hyenas)
            .ToList();

        return hyenas;
    }
}
