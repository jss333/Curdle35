using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public event System.Action<GameState> OnGameStateChanged;

    public GameState CurrentState { get; private set; } = GameState.PlayerInput;

    void Awake()
    {
        Instance = this;
    }

    private void SetState(GameState newState)
    {
        CurrentState = newState;
        OnGameStateChanged?.Invoke(CurrentState);
    }

    public bool IsPlayerInputState()
    {
        return CurrentState == GameState.PlayerInput;
    }

    public void OnPlayerUnitStartMoving()
    {
        if (CurrentState != GameState.PlayerInput)
        {
            Debug.LogWarning("Cannot start moving, not in PlayerInput state.");
            return;
        }

        SetState(GameState.PlayerUnitIsMoving);
    }

    public void OnPlayerUnitFinishMoving()
    {
        if (CurrentState != GameState.PlayerUnitIsMoving)
        {
            Debug.LogWarning("Cannot finish moving, not in PlayerUnitIsMoving state.");
            return;
        }

        SetState(GameState.PlayerInput);
    }

    public void OnPlayerEndTurn()
    {
        if (CurrentState != GameState.PlayerInput)
        {
            Debug.LogWarning("Cannot end turn, not in PlayerInput state.");
            return;
        }

        SetState(GameState.DayToNightAnimation);
        StartCoroutine(PlayDayToNightAnimation());
    }


    private IEnumerator PlayDayToNightAnimation()
    {
        Debug.Log("Playing day-to-night animation...");
        yield return new WaitForSeconds(1.5f); // Simulate animation time
        Debug.Log("Animation ended.");
        
        SetState(GameState.HyenasAreMovingAttacking);
    }
}
