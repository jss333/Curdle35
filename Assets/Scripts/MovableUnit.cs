using DG.Tweening;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Unit))]
public class MovableUnit : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private Ease moveEaseType = Ease.InOutCubic;

    private Unit unit;

    public void Start()
    {
        unit = GetComponent<Unit>();
    }

    public void MoveToCell(Vector2Int destination)
    {
        GameManager.Instance.SetState(GameState.UnitIsMoving);

        Vector3 targetWorld = GridHelper.Instance.GridToWorld(destination);
        transform.DOMove(targetWorld, moveSpeed)
            .SetEase(moveEaseType)
            .SetSpeedBased() // Use speed instead of time
            .OnComplete(() =>
            {
                unit.UpdateBoardPosition(destination);
                GameManager.Instance.SetState(GameState.PlayerInput);
            });
    }
}
