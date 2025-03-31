using UnityEngine;
using static DG.Tweening.DOTweenModuleUtils;

public class UnitSelectionManager : MonoBehaviour
{
    [Header("State")]
    [SerializeField] private Unit selectedUnit;

    void Update()
    {
        HandleSelection();
    }

    private void HandleSelection()
    {
        if (Input.GetMouseButtonDown(0)) // Left-click
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero); //distance of 0 means just check under the point

            if (hit.collider != null)
            {
                Unit unit = hit.collider.GetComponent<Unit>();

                if (unit != null)
                {
                    SelectUnit(unit);
                }
                else
                {
                    DeselectCurrentUnit();
                }
            }
            else
            {
                DeselectCurrentUnit();
            }
        }
    }

    private void SelectUnit(Unit unit)
    {
        if (selectedUnit != null)
        {
            selectedUnit.ShowDeselected();
        }

        selectedUnit = unit;
        selectedUnit.ShowSelected();
    }

    private void DeselectCurrentUnit()
    {
        if (selectedUnit != null)
        {
            selectedUnit.ShowDeselected();
            selectedUnit = null;
        }
    }
}
