using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class HyenasSpawnManager : MonoBehaviour
{
    [Header("Config - Spawn markers")]
    [SerializeField] private SpawnMarker spawnMarkerPrefab;
    [SerializeField] private int startInnerSpawnRate = 1;

    [Header("Config - Hyenas")]
    [SerializeField] private GameObject hyenaPrefab;
    [SerializeField] private Transform hyenasManager;
    [SerializeField] private float maxDelayBeforeSpawning = 0.5f;

    [Header("State")]
    [SerializeField] private int currentInnerSpawnRate;
    [SerializeField] private int nextHyenaId = 0;

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

        if (state == GameState.HyenasSpawning)
        {
            SpawnHyenasFromMarkers();
        }
    }

    private void InstantiateSpawnMarkers()
    {
        // Determine where hyenas will spawn next
        List<Vector2Int> spawnPoints = HyenasSpawnAI.GetSpawnPoints(currentInnerSpawnRate);

        // Instantiate spawn markers at each location
        foreach (Vector2Int spawnPoint in spawnPoints)
        {
            SpawnMarker spawnMarker = Instantiate(spawnMarkerPrefab, GridHelper.Instance.GridToWorld(spawnPoint), Quaternion.identity);
            spawnMarker.transform.SetParent(transform);
            spawnMarker.SetBoardPosition(spawnPoint);
            spawnMarker.name = "Spawn marker @ " + spawnPoint;
        }

        GameManager.Instance.OnHyenasFinishGeneratingNewSpawnMarkers();
    }

    private void SpawnHyenasFromMarkers()
    {
        Debug.Log("Spawning hyenas from markers");

        Sequence allSpawnSeq = DOTween.Sequence();

        foreach (Transform child in transform)
        {
            SpawnMarker spawnMarker = child.GetComponent<SpawnMarker>();
            if (spawnMarker != null)
            {
                Sequence spawnSeq = DOTween.Sequence();
                spawnSeq
                    .AppendInterval(Random.Range(0, maxDelayBeforeSpawning))
                    .AppendCallback(() => SpawnHyenaAtMarkerLocation(spawnMarker))
                    .AppendInterval(0.1f); // Give the Start() code in Unit time to run and register the hyena's position in BoardManager
                allSpawnSeq.Join(spawnSeq);
            }
        }

        allSpawnSeq.onComplete = GameManager.Instance.OnHyenasFinishSpawningFromMarkers;
        allSpawnSeq.Play();
    }

    private void SpawnHyenaAtMarkerLocation(SpawnMarker spawnMarker)
    {
        Vector2Int pos = spawnMarker.GetBoardPosition();

        // Destroy the spawn marker
        Destroy(spawnMarker.gameObject);

        // TODO also spawn a dust cloud that self-destructs once the animation ends
        // TODO hadle situation when hyena spawns on a cell occupied by another unit

        // Spawns a hyena at the spawn marker location
        GameObject hyena = Instantiate(hyenaPrefab, GridHelper.Instance.GridToWorld(pos), Quaternion.identity);
        hyena.transform.SetParent(hyenasManager); // Unit's logical board position is registered in BoardManager by Unit.Start()
        hyena.name = "Hyena #" + nextHyenaId++;
    }
}
