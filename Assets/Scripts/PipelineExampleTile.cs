using System;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace UnityEngine.Tilemaps
{
   /// <summary>
   /// Pipeline Tiles are tiles which take into consideration its orthogonal neighboring tiles and displays a sprite depending on whether the neighboring tile is the same tile.
   /// </summary>
   [Serializable]
   public class PipelineExampleTile : TileBase
   {
       /// <summary>
       /// The Sprites used for defining the Pipeline.
       /// </summary>
       [SerializeField]
       public Sprite[] m_Sprites;


       /// <summary>
       /// This method is called when the tile is refreshed. The PipelineExampleTile will refresh all neighboring tiles to update their rendering data if they are the same tile.
       /// </summary>
       /// <param name="position">Position of the tile on the Tilemap.</param>
       /// <param name="tilemap">The Tilemap the tile is present on.</param>
       public override void RefreshTile(Vector3Int position, ITilemap tilemap)
       {
           for (int yd = -1; yd <= 1; yd++)
               for (int xd = -1; xd <= 1; xd++)
               {
                   Vector3Int pos = new Vector3Int(position.x + xd, position.y + yd, position.z);
                   if (TileValue(tilemap, pos))
                       tilemap.RefreshTile(pos);
               }
       }


       /// <summary>
       /// Retrieves any tile rendering data from the scripted tile.
       /// </summary>
       /// <param name="position">Position of the tile on the Tilemap.</param>
       /// <param name="tilemap">The Tilemap the tile is present on.</param>
       /// <param name="tileData">Data to render the tile.</param>
       public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
       {
           UpdateTile(position, tilemap, ref tileData);
       }


       /// <summary>
       /// Checks the orthogonal neighbouring positions of the tile and generates a mask based on whether the neighboring tiles are the same. The mask will determine the according Sprite and transform to be rendered at the given position. The Sprite and Transform is then filled into TileData for the Tilemap to use. The Flags lock the color and transform to the data provided by the tile. The ColliderType is set to the shape of the Sprite used.
       /// </summary>
       private void UpdateTile(Vector3Int position, ITilemap tilemap, ref TileData tileData)
       {
           tileData.transform = Matrix4x4.identity;
           tileData.color = Color.white;


           int mask = TileValue(tilemap, position + new Vector3Int(0, 1, 0)) ? 8 : 0;
           mask += TileValue(tilemap, position + new Vector3Int(1, 0, 0)) ? 1 : 0;
           mask += TileValue(tilemap, position + new Vector3Int(0, -1, 0)) ? 2 : 0;
           
           
           mask += TileValue(tilemap, position + new Vector3Int(-1, 0, 0)) ? 4 : 0;


           if (mask >= 0 && mask < m_Sprites.Length && TileValue(tilemap, position))
           {
               tileData.sprite = m_Sprites[mask];
               tileData.transform = Matrix4x4.identity;
               tileData.flags = TileFlags.LockTransform | TileFlags.LockColor;
               tileData.colliderType = Tile.ColliderType.Sprite;
           }
       }


       /// <summary>
       /// Determines if the tile at the given position is the same tile as this.
       /// </summary>
       private bool TileValue(ITilemap tileMap, Vector3Int position)
       {
           TileBase tile = tileMap.GetTile(position);
           return (tile != null && tile == this);
       }
   }
  
#if UNITY_EDITOR
   /// <summary>
   /// Custom Editor for a PipelineExampleTile. This is shown in the Inspector window when a PipelineExampleTile asset is selected.
   /// </summary>
   [CustomEditor(typeof(PipelineExampleTile))]
   public class PipelineExampleTileEditor : Editor
   {
       private PipelineExampleTile tile { get { return (target as PipelineExampleTile); } }


       public void OnEnable()
       {
           if (tile.m_Sprites == null || tile.m_Sprites.Length != 16)
               tile.m_Sprites = new Sprite[16];
       }


       /// <summary>
       /// Draws an Inspector for the PipelineExampleTile.
       /// </summary>
       public override void OnInspectorGUI()
       {
           EditorGUILayout.LabelField("Place sprites shown based on the number of tiles bordering it.");
           EditorGUILayout.Space();
          
           EditorGUI.BeginChangeCheck();
           for(int i = 0; i < 16; i++){
           tile.m_Sprites[i] = (Sprite) EditorGUILayout.ObjectField("x", tile.m_Sprites[i], typeof(Sprite), false, null);
           }
           if (EditorGUI.EndChangeCheck())
               EditorUtility.SetDirty(tile);
       }


       /// <summary>
       /// The following is a helper that adds a menu item to create a PipelineExampleTile Asset in the project.
       /// </summary>
       [MenuItem("Assets/Create/PipelineExampleTile")]
       public static void CreatePipelineExampleTile()
       {
           string path = EditorUtility.SaveFilePanelInProject("Save Pipeline Example Tile", "New Pipeline Example Tile", "Asset", "Save Pipeline Example Tile", "Assets");
           if (path == "")
               return;                           
           AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<PipelineExampleTile>(), path);
        }
   }
#endif
}

