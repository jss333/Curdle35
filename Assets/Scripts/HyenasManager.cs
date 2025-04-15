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

    private static HyenaMovementAI MovementAI = new HyenaMovementAI();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
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
        Debug.Log($"*** moving next hyena {currentMoveOrderIndex}/{moveOrders.Count()}...");

        if (currentMoveOrderIndex >= moveOrders.Count())
        {
            Debug.Log($"*** calling OnHyenasFinishMoving()...");
            GameManager.Instance.OnHyenasFinishMoving();
            return;
        }

        var moveOrder = moveOrders[currentMoveOrderIndex];
        currentMoveOrderIndex++;

        if(moveOrder.movePath.Count() == 0)
        {
            Debug.Log($"*** hyena {moveOrder.hyena.name} has no place to move to / currentHyenaIndex has been updated to {currentMoveOrderIndex}.");

            MoveNextHyena();
        }
        else
        {
            Debug.Log($"*** moving hyena {moveOrder.hyena.name} to {moveOrder.movePath.Last()} / currentHyenaIndex has been updated to {currentMoveOrderIndex}.");

            DOTween.Sequence()
                .AppendInterval(delayBetweenMoves)
                .AppendCallback(() => { moveOrder.hyena.MoveAlongPath(moveOrder.movePath, MoveNextHyena); })
                .Play();
        }
    }

    public List<Unit> GetAllHyenas()
    {
        List<Unit> hyenas = new List<Unit>();
        foreach (Transform child in transform)
        {
            if(child.gameObject.activeInHierarchy && child.TryGetComponent<Unit>(out Unit hyena))
            {
                hyenas.Add(hyena);
            }
        }
        return hyenas;
    }
}
