using UnityEngine;
using UnityEngine.EventSystems;

public class MainMenuItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private GameObject arrow;

    void Start()
    {
        if(transform.childCount > 0)
        {
            arrow = transform.GetChild(0).gameObject;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (arrow != null)
        {
            arrow.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (arrow != null)
        {
            arrow.SetActive(false);
        }
    }
}
