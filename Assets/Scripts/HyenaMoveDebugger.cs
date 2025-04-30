using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MovableUnit))]
public class HyenaMoveDebugger : Editor
{
    private void OnSceneGUI()
    {
        //MovableUnit hyena = (MovableUnit)target;
        //HyenaMoveOrder moveOrder = HyenasManager.Instance.GetLastMoveOrders()[hyena];

        //if (hyena == null)
        //{
        //    return;
        //}

        //foreach (var pathPoint in moveOrder.movePath)
        //{
        //    Handles.color = Color.green;
        //    Handles.DrawWireDisc(BoardManager.Instance.BoardCellToWorld(pathPoint), Vector3.up, 0.8f);
        //}
    }
}
