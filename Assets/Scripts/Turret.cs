using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Turret : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] int shootingRange = 2;

    public bool TryAcquireTarget(out Unit hyena)
    {
        hyena = HyenasManager.Instance.GetAllHyenas()
            //.Where(h => h.IsAlive)
            //.Where(h => h.Faction == Faction.Hyenas)
            //.Where(h => Vector2Int.Distance(h.BoardPosition, BoardPosition) <= shootingRange)
            //.OrderBy(h => Vector2Int.Distance(h.BoardPosition, BoardPosition))
            .FirstOrDefault();

        if (hyena == null)
        {
            Debug.Log($"Turret {name} could not acquire target.");
            return false;
        }
        else
        {
            Debug.Log($"Turret {name} acquired {hyena.name} at {hyena.GetBoardPosition()} as target.");
            return true;
        }
    }
}
