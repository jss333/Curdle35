using UnityEngine;
using UnityEngine.UI;

public class UnitPortrait : MonoBehaviour
{
    private Image image;

    void Start()
    {
        image = GetComponent<Image>();
        image.enabled = false;

        UnitSelectionManager.Instance.OnUnitSelected += HandleUnitSelected;
        UnitSelectionManager.Instance.OnUnitDeselected += HandleUnitDeselected;
    }

    public void HandleUnitSelected(SelectableUnit selectedUnit)
    {
        image.enabled = true;
        image.sprite = selectedUnit.GetUnitPortrait();
    }

    public void HandleUnitDeselected(SelectableUnit deselectedUnit)
    {
        image.enabled = false;
    }
}
