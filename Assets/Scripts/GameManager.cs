using UnityEngine;
using System.Collections;
using DG.Tweening;

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

    public void OnPlayerUnitStartsMoving()
    {
        if (CurrentState != GameState.PlayerInput)
        {
            Debug.LogWarning("Cannot start moving, not in PlayerInput state.");
            return;
        }

        SetState(GameState.PlayerUnitMoving);
    }

    public void OnPlayerUnitFinishesMoving()
    {
        if (CurrentState != GameState.PlayerUnitMoving)
        {
            Debug.LogWarning("Cannot finish moving, not in PlayerUnitIsMoving state.");
            return;
        }

        SetState(GameState.PlayerInput);
    }

    public void OnPlayerEndsTurn()
    {
        if (CurrentState != GameState.PlayerInput)
        {
            Debug.LogWarning("Cannot end turn, not in PlayerInput state.");
            return;
        }

        SetState(GameState.PlayerTurretShooting);
        Simulate("Turrets shooting", 1f, OnPlayerTurretsFinishShooting);
    }

    public void OnPlayerTurretsFinishShooting()
    {
        if (CurrentState != GameState.PlayerTurretShooting)
        {
            Debug.LogWarning("Cannot finish turret shooting, not in PlayerTurretShooting state.");
            return;
        }

        SetState(GameState.PlayerHarvesting);
        Simulate("Player harvesting", 1f, OnPlayerFinishesHarvesting);
    }

    public void OnPlayerFinishesHarvesting()
    {
        if (CurrentState != GameState.PlayerHarvesting)
        {
            Debug.LogWarning("Cannot finish player harvesting, not in PlayerHarvesting state.");
            return;
        }

        SetState(GameState.DayToNightAnimation); // Day Night Indicator object observes this state change to play the animation
    }

    public void OnDayToNightAnimationEnds()
    {
        if (CurrentState != GameState.DayToNightAnimation)
        {
            Debug.LogWarning("Cannot end day-to-night animation, not in DayToNightAnimation state.");
            return;
        }

        SetState(GameState.HyenasMoving);
        Simulate("Hyenas moving", 1f, OnHyenasFinishMoving);
    }

    public void OnHyenasFinishMoving()
    {
        if (CurrentState != GameState.HyenasMoving)
        {
            Debug.LogWarning("Cannot finish hyenas moving, not in HyenasAreMoving state.");
            return;
        }

        SetState(GameState.HyenasHarvesting);
        Simulate("Hyenas harvesting", 1f, OnHyenasFinishHarvesting);
    }

    public void OnHyenasFinishHarvesting()
    {
        if (CurrentState != GameState.HyenasHarvesting)
        {
            Debug.LogWarning("Cannot finish hyenas harvesting, not in HyenasHarvesting state.");
            return;
        }

        SetState(GameState.NightToDayAnimation); // Day Night Indicator object observes this state change to play the animation
    }

    public void OnNightToDayAnimationEnds()
    {
        if (CurrentState != GameState.NightToDayAnimation)
        {
            Debug.LogWarning("Cannot end night-to-day animation, not in NightToDayAnimation state.");
            return;
        }

        SetState(GameState.PlayerInput);
    }

    private static void Simulate(string msg, float duration, System.Action completionCallback)
    {
        DOTween.Sequence()
            .OnStart(() => Debug.Log(msg + " start..."))
            .AppendInterval(duration)
            .AppendCallback(() => Debug.Log(msg + " end."))
            .OnComplete(() => completionCallback?.Invoke())
            .Play();
    }
}
