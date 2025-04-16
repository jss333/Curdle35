using UnityEngine;
using DG.Tweening;

public class VictoryPanel : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private GameObject panel;
    [SerializeField] private float fadeDuration = 1f;

    void Start()
    {
        GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
        panel.SetActive(false);
    }

    private void HandleGameStateChanged(GameState state)
    {
        if(state == GameState.Victory)
        {
            CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0;
            panel.SetActive(true);
            canvasGroup.DOFade(1f, fadeDuration);
        }
    }
}
