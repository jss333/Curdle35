using DG.Tweening;
using UnityEngine;
using DG;
using System.Collections.Generic;

public class TurretsManager : MonoBehaviour
{
    public static TurretsManager Instance { get; private set; }

    [Header("Config")]
    [SerializeField] private GameObject turretPrefab;
    [SerializeField] private int turretBuildCost = 15;
    [SerializeField] private float delayBetweenTurretShots = 0.5f;

    //[Header("Config - Camera pan")]
    //[SerializeField] public float cameraPanTime = 0.4f;

    [Header("Config - Reticle")]
    [SerializeField] private GameObject reticlePrefab;
    [SerializeField] public float reticleStartingScale = 5f;
    [SerializeField] public float reticleDisplayTime = 0.4f;
    [SerializeField] public Ease reticleScaleDownEaseType = Ease.OutExpo;

    [Header("State")]
    [SerializeField] private int nextTurretId = 0;
    [SerializeField] private int currentTurretIndex;
    [SerializeField] private List<Turret> turretsToShoot;
    [SerializeField] private GameObject lastReticleCreated;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
    }

    public int GetTurretBuildCost()
    {
        return turretBuildCost;
    }

    public void BuildTurretAt(Vector2Int cell)
    {
        BoardManager boardMngr = BoardManager.Instance;
        ResourcesManager resourceMngr = ResourcesManager.Instance;

        if (boardMngr.TryGetUnitAt(cell, out Unit unit))
        {
            Debug.LogError($"Trying to build a new turret at {cell} but {unit.name} is already there. Will ignore build.");
            return;
        }

        if (resourceMngr.PlayerResources < turretBuildCost)
        {
            Debug.LogError($"Not enough resources to build a turret at {cell}. Will ignore build.");
            return;
        }

        Vector3 worldPos = boardMngr.BoardCellToWorld(cell);
        GameObject turret = Instantiate(turretPrefab, worldPos, Quaternion.identity); // Unit's logical board position is registered in BoardManager by Unit.Start()
        turret.transform.SetParent(this.transform);
        turret.name = $"Turret #{nextTurretId++} @ {cell}";

        resourceMngr.PlayerResources -= turretBuildCost;
    }

    public void HandleGameStateChanged(GameState newState)
    {
        if (newState != GameState.PlayerTurretShooting) return;

        turretsToShoot = new List<Turret>();
        foreach (var turret in GetComponentsInChildren<Turret>())
        {
            turretsToShoot.Add(turret);
        }
        currentTurretIndex = 0;

        NextTurretShoot();
    }

    private void NextTurretShoot()
    {
        if (currentTurretIndex >= turretsToShoot.Count)
        {
            GameManager.Instance.OnPlayerTurretsFinishShooting();
            return;
        }

        var turret = turretsToShoot[currentTurretIndex];
        currentTurretIndex++;

        PanToTurretAndShootHyenaIfTargetAvailable(turret);
    }

    private void PanToTurretAndShootHyenaIfTargetAvailable(Turret turret)
    {
        if (turret.TryAcquireTarget(out Unit hyena))
        {
            var cell = hyena.GetBoardPosition();

            DOTween.Sequence()
                //.AppendCallback(PanCameraToTurret)
                //.AppendInterval(cameraPanTime)
                .AppendCallback(() => { InstantiateReticle(cell); })
                .AppendInterval(reticleDisplayTime)
                .AppendCallback(() => { DamageHyenaAndRemoveReticle(hyena); })
                .AppendInterval(delayBetweenTurretShots)
                .AppendCallback(NextTurretShoot);
        }
        else
        {
            NextTurretShoot();
        }
    }

    private void PanCameraToTurret()
    {

    }

    private void InstantiateReticle(Vector2Int cell)
    {
        lastReticleCreated = Instantiate(reticlePrefab, BoardManager.Instance.BoardCellToWorld(cell), Quaternion.identity);
    }

    private void DamageHyenaAndRemoveReticle(Unit hyena)
    {
        hyena.Die();
        Destroy(lastReticleCreated);
    }
}
