using UnityEngine;
using System.Collections;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public event System.Action OnNewPlayerTurnStarted;
    public event System.Action<GameState> OnGameStateChanged;

    public GameState CurrentState { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        StartNewPlayerTurnAndSetGameStateToPlayerInput();
    }

    private void SetState(GameState newState)
    {
        CurrentState = newState;
        OnGameStateChanged?.Invoke(CurrentState);
    }

    private void StartNewPlayerTurnAndSetGameStateToPlayerInput()
    {
        // This marks the begining of a new player turn so we send out the appropriate event BEFORE changing the game state
        Debug.Log("New player turn has started");
        OnNewPlayerTurnStarted?.Invoke();
        SetState(GameState.PlayerInput);
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

        SetState(GameState.PlayerTurretShooting); // TurretsManager object observes this state change
    }

    public void OnPlayerTurretsFinishShooting()
    {
        if (CurrentState != GameState.PlayerTurretShooting)
        {
            Debug.LogWarning("Cannot finish turret shooting, not in PlayerTurretShooting state.");
            return;
        }

        SetState(GameState.PlayerHarvesting); // ResourcesManager object observes this state change
    }

    public void OnPlayerFinishesHarvesting()
    {
        if (CurrentState != GameState.PlayerHarvesting)
        {
            Debug.LogWarning("Cannot finish player harvesting, not in PlayerHarvesting state.");
            return;
        }

        SoundsManager.Instance.UpdateCurrentMusic(SoundsManager.MusicType.Night);

        SetState(GameState.DayToNightAnimation); // Day Night Indicator object observes this state change to play the animation
    }

    public void OnDayToNightAnimationEnds()
    {
        if (CurrentState != GameState.DayToNightAnimation)
        {
            Debug.LogWarning("Cannot end day-to-night animation, not in DayToNightAnimation state.");
            return;
        }

        SetState(GameState.HyenasSpawning); // HyenasSpawnManager object observes this state change
    }

    public void OnHyenasFinishSpawningFromMarkers()
    {
        if (CurrentState != GameState.HyenasSpawning)
        {
            Debug.LogWarning("Cannot finish hyenas spawning, not in HyenasSpawning state.");
            return;
        }

        SetState(GameState.HyenasMoving); // HyenasManager object observes this state change to start moving hyenas
    }

    public void OnHyenasFinishMoving()
    {
        if (CurrentState != GameState.HyenasMoving)
        {
            Debug.LogWarning("Cannot finish hyenas moving, not in HyenasAreMoving state.");
            return;
        }

        SetState(GameState.HyenasHarvesting); // ResourcesManager object observes this state change
    }

    public void OnHyenasFinishHarvesting()
    {
        if (CurrentState != GameState.HyenasHarvesting)
        {
            Debug.LogWarning("Cannot finish hyenas harvesting, not in HyenasHarvesting state.");
            return;
        }

        SoundsManager.Instance.UpdateCurrentMusic(SoundsManager.MusicType.Day);

        SetState(GameState.HyenasGenerateNewSpawnMarkers); // HyenasSpawnManager object observes this state change
    }

    public void OnHyenasFinishGeneratingNewSpawnMarkers()
    {
        if (CurrentState != GameState.HyenasGenerateNewSpawnMarkers)
        {
            Debug.LogWarning("Cannot finish generating spawn markers, not in HyenasGenerateNewSpawnMarkers state.");
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

        StartNewPlayerTurnAndSetGameStateToPlayerInput();
    }

    public void OnPlayerVictory()
    {
        SetState(GameState.Victory);
    }

    public void OnPlayerDefeat()
    {
        SetState(GameState.Defeat);
    }
}
