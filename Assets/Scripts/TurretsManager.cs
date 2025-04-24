using DG.Tweening;
using UnityEngine;
using DG;
using System.Collections.Generic;
using System.Linq;

public class TurretsManager : MonoBehaviour
{
    public static TurretsManager Instance { get; private set; }

    public event System.Action<Turret> OnNewTurretInstantiation;

    [Header("Config - Turret")]
    [SerializeField] private Turret turretPrefab;
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
    [SerializeField] private List<Turret> turrets;
    [SerializeField] private GameObject lastReticleCreated;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;

        Debug.Log("=== TurretsManager initialized and listeners set up. ===");
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
        Turret turret = Instantiate(turretPrefab, worldPos, Quaternion.identity); // Unit's logical board position is registered in BoardManager by Unit.Start()
        turret.transform.SetParent(this.transform);
        turret.name = $"Turret #{nextTurretId++} @ {cell}";

        resourceMngr.PlayerResources -= turretBuildCost;

        OnNewTurretInstantiation?.Invoke(turret);
    }

    public List<Turret> GetAllTurrets()
    {
        List<Turret> turrets = transform
            .Cast<Transform>()
            .Where(child => child.gameObject.activeInHierarchy)
            .Select(child => child.GetComponent<Turret>())
            .Where(turret => turret != null)
            .ToList();

        return turrets;
    }

    public void HandleGameStateChanged(GameState newState)
    {
        if (newState != GameState.PlayerTurretShooting) return;

        turrets = transform
            .Cast<Transform>()
            .Where(child => child.gameObject.activeInHierarchy)
            .Select(child => child.GetComponent<Turret>())
            .Where(turret => turret != null)
            .ToList();
        currentTurretIndex = 0;

        List<Unit> allHyenas = HyenasManager.Instance.GetAllHyenas();

        NextTurretShoots(allHyenas);
    }

    private void NextTurretShoots(List<Unit> allHyenas)
    {
        if (currentTurretIndex >= turrets.Count)
        {
            GameManager.Instance.OnPlayerTurretsFinishShooting();
            return;
        }

        var turret = turrets[currentTurretIndex];
        currentTurretIndex++;

        PanToTurretAndShootHyenaIfTargetAvailable(turret, allHyenas);
    }

    private void PanToTurretAndShootHyenaIfTargetAvailable(Turret turret, List<Unit> allHyenas)
    {
        if (turret.TryAcquireTarget(allHyenas, out Unit hyena))
        {
            SelectableTurret selectableTurret = turret.GetComponent<SelectableTurret>();

            DOTween.Sequence()
                //.AppendCallback(PanCameraToTurret)
                //.AppendInterval(cameraPanTime)
                .AppendCallback(selectableTurret.ShowSelectedEffect)
                .AppendCallback(() => InstantiateReticle(hyena.GetBoardPosition()))
                .AppendInterval(reticleDisplayTime)
                .AppendCallback(() => DamageHyenaAndRemoveReticle(hyena))
                .AppendInterval(delayBetweenTurretShots)
                .AppendCallback(selectableTurret.RemoveSelectedEffect)
                .AppendCallback(() => NextTurretShoots(allHyenas));
        }
        else
        {
            NextTurretShoots(allHyenas);
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
        SoundsManager.Instance.PlaySFX(SFX.Turret_Shoots_Hyena);
        hyena.Die();
        Destroy(lastReticleCreated);
    }
}
