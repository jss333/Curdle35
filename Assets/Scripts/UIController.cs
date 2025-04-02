using UnityEngine;

public class UIController : MonoBehaviour
{
    public void OnEndTurnClicked()
    {
        GameManager.Instance.OnPlayerEndTurn();
    }
}
