using UnityEngine;

public enum GameState
{
    GameInitializing,
    PlayerInput,
    PlayerUnitMoving,
    PlayerTurretShooting,
    PlayerHarvesting,
    DayToNightAnimation,
    HyenasSpawning,
    HyenasMoving,
    HyenasHarvesting,
    HyenasGenerateNewSpawnMarkers,
    NightToDayAnimation,
    Victory,
    Defeat
}
