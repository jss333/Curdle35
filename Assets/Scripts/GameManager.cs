using UnityEngine;
using System.Collections;
using DG.Tweening;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public event System.Action OnNewPlayerTurnStarted;
    public event System.Action<GameState> OnGameStateChanged;

    [Header("State")]
    [SerializeField] private GameState _currentState = GameState.GameInitializing;
    [SerializeField] private bool firstTurn = true;

    public GameState CurrentState
    {
        get => _currentState;
        private set
        {
            _currentState = value;
            Debug.Log($"Game state changed to: {value}");
        }
    }

    private void SetState(GameState newState)
    {
        CurrentState = newState;
        OnGameStateChanged?.Invoke(CurrentState);
    }

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (firstTurn)
        {
            firstTurn = false;
            StartNewPlayerTurnAndSetGameStateToPlayerInput(false);
        }
    }

    private void StartNewPlayerTurnAndSetGameStateToPlayerInput(bool playSFX = false)
    {
        // This marks the begining of a new player turn so we send out the appropriate event BEFORE changing the game state
        Debug.Log("New player turn has started, firing OnNewPlayerTurnStarted event now");
        OnNewPlayerTurnStarted?.Invoke();

        SoundsManager.Instance.PlayMusic(BGM.Day);

        if (playSFX)
        {
            SoundsManager.Instance.PlaySFX(SFX.Day_Begins);
        }

        SetState(GameState.PlayerInput);
    }

    public void OnPlayerUnitStartsMoving()
    {
        if (CurrentState != GameState.PlayerInput)
        {
            Debug.LogWarning($"Cannot start moving, current state is {CurrentState} not in PlayerInput state.");
            return;
        }

        SetState(GameState.PlayerUnitMoving);
    }

    public void OnPlayerUnitFinishesMoving()
    {
        if (CurrentState != GameState.PlayerUnitMoving)
        {
            Debug.LogWarning($"Cannot finish moving, current state is {CurrentState} not in PlayerUnitIsMoving state.");
            return;
        }

        SetState(GameState.PlayerInput);
    }

    public void OnPlayerEndsTurn()
    {
        if (CurrentState != GameState.PlayerInput)
        {
            Debug.LogWarning($"Cannot end turn, current state is {CurrentState} not in PlayerInput state.");
            return;
        }

        SetState(GameState.PlayerTurretShooting); // TurretsManager object observes this state change
    }

    public void OnPlayerTurretsFinishShooting()
    {
        if (CurrentState != GameState.PlayerTurretShooting)
        {
            Debug.LogWarning($"Cannot finish turret shooting, current state is {CurrentState} not in PlayerTurretShooting state.");
            return;
        }

        SetState(GameState.PlayerHarvesting); // ResourcesManager object observes this state change
    }

    public void OnPlayerFinishesHarvesting()
    {
        if (CurrentState != GameState.PlayerHarvesting)
        {
            Debug.LogWarning($"Cannot finish player harvesting, current state is {CurrentState} not in PlayerHarvesting state.");
            return;
        }

        SoundsManager.Instance.PlaySFX(SFX.Night_Begins);
        SoundsManager.Instance.PlayMusic(BGM.Night);

        SetState(GameState.DayToNightAnimation); // Day Night Indicator object observes this state change to play the animation
    }

    public void OnDayToNightAnimationEnds()
    {
        if (CurrentState != GameState.DayToNightAnimation)
        {
            Debug.LogWarning($"Cannot end day-to-night animation, current state is {CurrentState} not in DayToNightAnimation state.");
            return;
        }

        SetState(GameState.HyenasSpawning); // HyenasSpawnManager object observes this state change
    }

    public void OnHyenasFinishSpawningFromMarkers()
    {
        if (CurrentState != GameState.HyenasSpawning)
        {
            Debug.LogWarning($"Cannot finish hyenas spawning, current state is {CurrentState} not in HyenasSpawning state.");
            return;
        }

        SetState(GameState.HyenasMoving); // HyenasManager object observes this state change to start moving hyenas
    }

    public void OnHyenasFinishMoving()
    {
        if (CurrentState != GameState.HyenasMoving)
        {
            Debug.LogWarning($"Cannot finish hyenas moving, current state is {CurrentState} not in HyenasAreMoving state.");
            return;
        }

        SetState(GameState.HyenasHarvesting); // ResourcesManager object observes this state change
    }

    public void OnHyenasFinishHarvesting()
    {
        if (CurrentState != GameState.HyenasHarvesting)
        {
            Debug.LogWarning($"Cannot finish hyenas harvesting, current state is {CurrentState} not in HyenasHarvesting state.");
            return;
        }

        SetState(GameState.HyenasGenerateNewSpawnMarkers); // HyenasSpawnManager object observes this state change
    }

    public void OnHyenasFinishGeneratingNewSpawnMarkers()
    {
        if (CurrentState != GameState.HyenasGenerateNewSpawnMarkers)
        {
            Debug.LogWarning($"Cannot finish generating spawn markers, current state is {CurrentState} not in HyenasGenerateNewSpawnMarkers state.");
            return;
        }

        SetState(GameState.NightToDayAnimation); // Day Night Indicator object observes this state change to play the animation
    }

    public void OnNightToDayAnimationEnds()
    {
        if (CurrentState != GameState.NightToDayAnimation)
        {
            Debug.LogWarning($"Cannot end night-to-day animation, current state is {CurrentState} not in NightToDayAnimation state.");
            return;
        }

        StartNewPlayerTurnAndSetGameStateToPlayerInput(true);
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
