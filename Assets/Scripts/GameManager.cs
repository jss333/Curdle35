using UnityEngine;
using System.Collections;

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

    public void OnPlayerEndTurn()
    {
        if (CurrentState != GameState.PlayerInput) return;

        SetState(GameState.DayToNightAnimation);
        StartCoroutine(PlayDayToNightAnimationThenSpawn());
    }

    private IEnumerator PlayDayToNightAnimationThenSpawn()
    {
        Debug.Log("Playing day-to-night animation...");
        yield return new WaitForSeconds(3f); // Simulate animation time
        Debug.Log("Animation ended.");
        
        //SpawnHyenas();

        SetState(GameState.HyenasAreMovingAttacking);
    }
}
