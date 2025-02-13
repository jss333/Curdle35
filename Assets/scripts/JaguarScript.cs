using System;
using UnityEngine;

public class JaguarController : CatController
{
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void Start()
    {
        base.team = Team.cats;

        base.canPlaceTower = false;
        base.movementType = Movement.Type.Jaguar;

        base.Start();
        PrecalculateMovementOffsets();
    } 

    void PrecalculateMovementOffsets(){
        movementOffsets = new Vector3Int[28];

        int currentIter = 0;
        Vector3Int placement = Vector3Int.zero;
        for(placement.x = -3; placement.x <= 3; placement.x++){
            if(placement.x == 0){
                continue;
            }
            movementOffsets[currentIter] = placement;
            currentIter++;
        }
        //currentIter should be 8 right now
        placement.x = 0;

        for(placement.y = -4; placement.y <= 4; placement.y++){
            if(placement.y == 0){
                continue;
            }
            movementOffsets[currentIter] = placement;
            currentIter++;
        }
        //currentIter should be 16 right now
        
        for(placement.x = -3; placement.x <= 3; placement.x++){
            if(placement.x == 0){
                continue;
            }
            placement.y = placement.x;
            movementOffsets[currentIter] = placement;
            currentIter++;
            placement.y = -placement.x;
            movementOffsets[currentIter] = placement;
            currentIter++;
        }
    }
}
/*
[CustomEditor(typeof(JaguarController))]
public class JaguarControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draws the default Inspector fields for CatController
        DrawDefaultInspector();

        JaguarController jaguar = (JaguarController)target;
        // Get reference to the CatController
        CatController catController = jaguar.GetComponent<CatController>();

        // Ensure there are inherited fields
        if (catController != null)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Cat UI Elements", EditorStyles.boldLabel);

            // Manually assign UI elements as Unity won't serialize them
            catController.moveButton = EditorGUILayout.ObjectField<UnityEngine.UIElements.Button>(catController.moveButton);
            catController.placeTowerButton = (UnityEngine.UIElements.Button)EditorGUILayout.ObjectField(catController.placeTowerButton, typeof(UnityEngine.UIElements.Button));
            catController.moveImage = (UnityEngine.UIElements.Image)EditorGUILayout.ObjectField(catController.moveImage, typeof(UnityEngine.UIElements.Image));
            catController.placeTowerImage = (UnityEngine.UIElements.Image)EditorGUILayout.ObjectField(catController.placeTowerImage, typeof(UnityEngine.UIElements.Image));
        }
        else{
            EditorGUILayout.HelpBox("No CatController found! Attach a CatController component to view its properties.", MessageType.Warning);
        }
    }
}
*/

