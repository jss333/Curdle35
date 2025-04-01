using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Unit))]
public class MovableUnit : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private float moveSpeed = 4f;

    private Unit unit;

    public void Start()
    {
        unit = GetComponent<Unit>();
    }

    public IEnumerator MoveToCell(Vector2Int destination)
    {
        GameManager.Instance.SetState(GameState.UnitIsMoving);

        Vector3 targetWorld = GridHelper.Instance.GridToWorld(destination);
        while (Vector3.Distance(transform.position, targetWorld) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetWorld, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetWorld;
        unit.UpdateBoardPosition(destination);

        GameManager.Instance.SetState(GameState.PlayerInput);
    }
}
