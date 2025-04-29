using UnityEngine;
using System.Collections.Generic;

public interface IHyenaMoveStrategy
{
    List<HyenaMoveOrder> CalculateMovementPathForHyenas(List<HyenaUnit> hyenas);
}
