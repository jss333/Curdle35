using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class GodWeaponVictory : MonoBehaviour
{
    [Header("Config - God Weapon")]
    [SerializeField] private int resourcesToUnlock = 300;
    [SerializeField] private string godWeaponButtonLabel = "Deploy God Weapon to WIN! (xxx)";
    [SerializeField] private Button godWeaponButton;

    [Header("Config - Flash Settings")]
    [SerializeField] private Color flashColor = Color.cyan;
    [SerializeField] private float flashDuration = 0.8f;

    private Image buttonImage;
    private Tween flashTween;

    void Start()
    {
        buttonImage = godWeaponButton.GetComponent<Image>();
        godWeaponButton.GetComponentInChildren<TextMeshProUGUI>().text = godWeaponButtonLabel.Replace("xxx", resourcesToUnlock.ToString());

        GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
        HandleGameStateChanged(GameManager.Instance.CurrentState); // Set initial button state

        ResourcesManager resourceMngr = ResourcesManager.Instance;
        resourceMngr.OnPlayerResourcesChanged += HandlePlayerResourcesChanged;

        godWeaponButton.onClick.AddListener(OnGodWeaponClicked);

        UpdateButtonInteractability();
    }

    public void HandleGameStateChanged(GameState state)
    {
        UpdateButtonInteractability();
    }

    public void HandlePlayerResourcesChanged(int newResources)
    {
        UpdateButtonInteractability();
    }

    private void UpdateButtonInteractability()
    {
        bool interactable = GameManager.Instance.CurrentState == GameState.PlayerInput
            && ResourcesManager.Instance.PlayerResources >= resourcesToUnlock;

        godWeaponButton.interactable = interactable;

        if (interactable)
        {
            StartFlashing();
        }
        else
        {
            StopFlashing();
        }
    }

    private void StartFlashing()
    {
        if (flashTween == null)
        {
            flashTween = buttonImage.DOColor(flashColor, flashDuration)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }
    }

    private void StopFlashing()
    {
        if (flashTween != null)
        {
            flashTween.Kill();
            flashTween = null;
            buttonImage.color = Color.white; // reset to default
        }
    }

    private void OnGodWeaponClicked()
    {
        Debug.Log("///////  God Weapon Deployed!  ///////");
    }
}
