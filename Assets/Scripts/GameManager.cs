using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameState CurrentState { get; private set; } = GameState.PlayerInput;

    void Awake()
    {
        Instance = this;
    }

    public void SetState(GameState newState)
    {
        CurrentState = newState;
    }

    public bool IsPlayerInputState()
    {
        return CurrentState == GameState.PlayerInput;
    }
}
