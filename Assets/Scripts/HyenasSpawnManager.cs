using UnityEngine;
using System.Collections.Generic;

public class HyenasSpawnManager : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private GameObject spawnMarkerPrefab;
    [SerializeField] private int startInnerSpawnRate = 1;

    [Header("State")]
    [SerializeField] private int currentInnerSpawnRate;

    private static HyenasSpawnAI HyenasSpawnAI = new HyenasSpawnAI();

    void Start()
    {
        GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;

        currentInnerSpawnRate = startInnerSpawnRate;
    }

    private void HandleGameStateChanged(GameState state)
    {
        if (state == GameState.HyenasGenerateNewSpawnMarkers)
        {
            InstantiateSpawnMarkers();
        }
    }

    private void InstantiateSpawnMarkers()
    {
        List<Vector2Int> spawnPoints = HyenasSpawnAI.GetSpawnPoints(currentInnerSpawnRate);

        foreach (Vector2Int spawnPoint in spawnPoints)
        {
            GameObject spawnMarker = Instantiate(spawnMarkerPrefab, GridHelper.Instance.GridToWorld(spawnPoint), Quaternion.identity);
            spawnMarker.transform.SetParent(transform);
        }

        GameManager.Instance.OnHyenasFinishGeneratingNewSpawnMarkers();
    }
}
