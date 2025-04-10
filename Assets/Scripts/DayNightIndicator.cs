using UnityEngine;

public class DayNightIndicator : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();

        GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
    }

    public void HandleGameStateChanged(GameState newState)
    {
        if(newState == GameState.DayToNightAnimation)
        {
            animator.SetTrigger("Play Day-to-night");
        }

        if (newState == GameState.NightToDayAnimation)
        {
            animator.SetTrigger("Play Night-to-day");
        }
    }

    public void OnDayToNightAnimationFinished()
    {
        GameManager.Instance.OnDayToNightAnimationEnds();
    }

    public void OnNightToDayAnimationFinished()
    {
        GameManager.Instance.OnNightToDayAnimationEnds();
    }
}
