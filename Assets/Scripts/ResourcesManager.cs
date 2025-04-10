using UnityEngine;
using TMPro;

public class ResourcesManager : MonoBehaviour
{
    public static ResourcesManager Instance { get; private set; }

    public event System.Action<int> OnPlayerResourcesChanged;
    public event System.Action<int> OnHyenasResourcesChanged;

    [Header("State")]
    [SerializeField] private int initialPlayerResources;
    [SerializeField] private int initialHyenasResources;

    [Header("State")]
    [SerializeField] private int playerResources;
    [SerializeField] private int hyenasResources;

    private void Awake()
    {
        Instance = this;

        playerResources = initialPlayerResources;
        hyenasResources = initialHyenasResources;
    }

    public int PlayerResources
    {
        get => playerResources;
        set
        {
            playerResources = value;
            OnPlayerResourcesChanged?.Invoke(playerResources);
        }
    }

    public int HyenasResources
    {
        get => hyenasResources;
        set
        {
            hyenasResources = value;
            OnHyenasResourcesChanged?.Invoke(hyenasResources);
        }
    }
}
