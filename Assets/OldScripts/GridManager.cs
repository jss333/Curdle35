using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridManager
{   
    public TilemapManager tmManager;
    Vector3Int[,] spots;
    Astar astar;
    List<Spot> path = new List<Spot>();
    public BoundsInt bounds;
    public void Initialize()
    {
        spots = new Vector3Int[bounds.size.x, bounds.size.y];
        for (int x = bounds.xMin, i = 0; i < (bounds.size.x); x++, i++)
        {
            for (int y = bounds.yMin, j = 0; j < (bounds.size.y); y++, j++)
            {
                if (tmManager.tilemapArray[(int)TilemapManager.MapType.ground].HasTile(new Vector3Int(x, y, 0)))
                {
                    spots[i, j] = new Vector3Int(x, y, 0);
                }
                else
                {
                    spots[i, j] = new Vector3Int(x, y, 1);
                }
            }
        }
        astar = new Astar(spots, bounds.size.x, bounds.size.y);
    }
    public List<Vector3> DetermineOptimalPath(Vector3Int startTile, int pathLength)
    {

        List<Spot> tempPath = new List<Spot>();
        Vector2Int start = new Vector2Int(startTile.x, startTile.y);

        path = astar.CreatePath(spots, start, new Vector2Int(tmManager.hqTowerPosition.x, tmManager.hqTowerPosition.y), 1000);

        //Debug.Log("path to HQ length : " + tempPath.Count);

        //ignoring minor twoers in hyena pathing
        //foreach(var towerPos in tmManager.minorTowers){
        //    tempPath = astar.CreatePath(spots, start, new Vector2Int(towerPos.x, towerPos.y), 1000);
        //    if(tempPath.Count < path.Count){
        //        Debug.Log("found a minor tower closer than HQ : " + tempPath.Count);
        //        path = tempPath;
        //   }
        //}

        if(path != null){
            List<Vector3Int> preParsedPath = new List<Vector3Int>();
            foreach(var sp in path){
                preParsedPath.Add(new Vector3Int(sp.X, sp.Y, 0));
            }
            //Debug.Log("pathing length : " + preParsedPath.Count);

            //Vector3Int begin = preParsedPath[0];
            //Vector3Int end = preParsedPath[1];


            //breaking up path here
            List<Vector3> ret = new List<Vector3>();
            for(int i = 0; (i < (preParsedPath.Count - 1)) && (i < (pathLength)); i++){
                //begin = preParsedPath[i];
                //end = preParsedPath[i + 1];
                Vector3 reinterpretVec = preParsedPath[preParsedPath.Count - 1 - i]; //the list is built backwards, so i need to reverse and crop it here
                reinterpretVec.x += 0.5f;
                reinterpretVec.y += 0.5f;
                ret.Add(reinterpretVec);
            }

            return ret;
        }
        else{
            Debug.Log("failed to find a path");
            return null;
        }
    }
}
