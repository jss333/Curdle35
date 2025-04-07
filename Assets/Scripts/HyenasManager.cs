using DG.Tweening;
using UnityEngine;
using System.Collections.Generic;

public struct HyenaMove
{
    public MovableUnit hyena;
    public IEnumerable<Vector2Int> path;

    public HyenaMove(MovableUnit hyena, IEnumerable<Vector2Int> path)
    {
        this.hyena = hyena;
        this.path = path;
    }
}

public class HyenasManager : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private float delayBetweenMoves = 0.3f;

    [Header("State")]
    [SerializeField] private int currentHyenaIndex = 0;
    [SerializeField] private List<HyenaMove> hyenasToMove = new List<HyenaMove>();

    private static HyenaMovementAI MovementAI = new HyenaMovementAI();

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
            if (child.gameObject.activeInHierarchy == false) continue;

            var unit = child.GetComponent<Unit>();
            var hyena = child.GetComponent<MovableUnit>();

            if (unit == null || hyena == null)
            {
                Debug.LogWarning($"--- skipping child '{child.name}' as it does not have Unit and MovableUnit components.");
                continue;
            }

            IEnumerable<Vector2Int> path = MovementAI.CalculateMovementPath(unit.GetBoardPosition());

            hyenasToMove.Add(new HyenaMove(hyena, path));
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
            .AppendCallback(() => { move.hyena.MoveAlongPath(move.path, MoveNextHyena); })
            .Play();
    }
}
