using DG.Tweening;
using UnityEngine;
using System.Collections.Generic;

public struct HyenaMove
{
    public MovableUnit hyena;
    public Vector2Int targetCell;

    public HyenaMove(MovableUnit hyena, Vector2Int targetCell)
    {
        this.hyena = hyena;
        this.targetCell = targetCell;
    }
}

public class HyenasManager : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private float delayBetweenMoves = 0.3f;

    [Header("State")]
    [SerializeField] private int currentHyenaIndex = 0;
    [SerializeField] private List<HyenaMove> hyenasToMove = new List<HyenaMove>();

    void Start()
    {
        GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
    }

    public void HandleGameStateChanged(GameState state)
    {
        if (state != GameState.HyenasMoving) return;

        Debug.Log("Starting to move hyenas");
        hyenasToMove.Clear();
        currentHyenaIndex = 0;

        // Iterate over all child hyena to pick a target cell and move it there
        foreach (Transform child in transform)
        {
            var movementAI = child.GetComponent<MovementAI>();
            var hyena = child.GetComponent<MovableUnit>();

            if (movementAI == null || hyena == null)
            {
                Debug.LogWarning($"--- skipping child '{child.name}' as it does not have both MovementAI and MovableUnit components.");
                continue;
            }

            Vector2Int targetCell = movementAI.CalculateTargetCell();
            hyenasToMove.Add(new HyenaMove(hyena, targetCell));
        }

        MoveNextHyena();
    }

    private void MoveNextHyena()
    {
        if (currentHyenaIndex >= hyenasToMove.Count)
        {
            GameManager.Instance.OnHyenasFinishMoving();
            return;
        }

        var move = hyenasToMove[currentHyenaIndex];
        currentHyenaIndex++;

        DOTween.Sequence()
            .AppendInterval(delayBetweenMoves)
            .AppendCallback(() => { move.hyena.MoveToCell(move.targetCell, MoveNextHyena); })
            .Play();
    }
}
