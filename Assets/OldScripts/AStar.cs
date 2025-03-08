using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Astar
{
    public Spot[,] Spots;
    public Astar(Vector3Int[,] grid, int columns, int rows)
    {
        Spots = new Spot[columns, rows];
    }
    private bool IsValidPath(Vector3Int[,] grid, Spot start, Spot end)
    {
        if (end == null)
            return false;
        if (start == null)
            return false;
        if (end.Height >= 1)
            return false;
        return true;
    }
    public List<Spot> CreatePath(Vector3Int[,] grid, Vector2Int start, Vector2Int end, int length)
    {
        //if (!IsValidPath(grid, start, end))
        //     return null;

        Spot End = null;
        Spot Start = null;
        var columns = Spots.GetUpperBound(0) + 1;
        var rows = Spots.GetUpperBound(1) + 1;
        Spots = new Spot[columns, rows];

        for (int i = 0; i < columns; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                Spots[i, j] = new Spot(grid[i, j].x, grid[i, j].y, grid[i, j].z);
            }
        }

        for (int i = 0; i < columns; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                Spots[i, j].AddNeighbors(Spots, i, j, true); //if you dont want hyenas to hit HQ tower from diagonal, figure out how to make its neighbors not have diagonal neighbors
                if (Spots[i, j].X == start.x && Spots[i, j].Y == start.y){
                    Start = Spots[i, j];
                    //Debug.Log("found start - " + Start.X + ":" + Start.Y);
                }
                else if (Spots[i, j].X == end.x && Spots[i, j].Y == end.y){
                    End = Spots[i, j];
                    
                    //Debug.Log("found end - " + End.X + ":" + End.Y);
                }
            }
        }
        if (!IsValidPath(grid, Start, End))
            return null;
        List<Spot> OpenSet = new List<Spot>();
        List<Spot> ClosedSet = new List<Spot>();

        OpenSet.Add(Start);

        while (OpenSet.Count > 0)
        {
            //Find shortest step distance in the direction of your goal within the open set
            int winner = 0;
            for (int i = 0; i < OpenSet.Count; i++)
                if (OpenSet[i].F < OpenSet[winner].F)
                    winner = i;
                else if (OpenSet[i].F == OpenSet[winner].F)//tie breaking for faster routing
                    if (OpenSet[i].H < OpenSet[winner].H)
                        winner = i;

            var current = OpenSet[winner];

            //Found the path, creates and returns the path
            if (End != null && OpenSet[winner] == End)
            {
                List<Spot> Path = new List<Spot>();
                var temp = current;
                Path.Add(temp);
                while (temp.previous != null)
                {
                    Path.Add(temp.previous);
                    temp = temp.previous;
                }
                //Debug.Log("pre remove range path\n\n");
                //for(int z = 0 ; z < Path.Count; z++){
                //    Debug.Log("astar raw path - " + z + " : (" + Path[z].X + ":" + Path[z].Y + ")");
                //}

                if (length - (Path.Count - 1) < 0)
                {
                    Path.RemoveRange(0, (Path.Count - 1) - length);
                }
                //Debug.Log("post remove range path\n\n");
                //for(int z = 0 ; z < Path.Count; z++){
                //    Debug.Log("astar filtered path - " + z + " : (" + Path[z].X + ":" + Path[z].Y + ")");
                //}


                return Path;
            }

            OpenSet.Remove(current);
            ClosedSet.Add(current);


            //Finds the next closest step on the grid
            var neighbors = current.Neighbors;
            for (int i = 0; i < neighbors.Count; i++)//look threw our current spots neighboors (current spot is the shortest F distance in openSet
            {
                var n = neighbors[i];
                if (!ClosedSet.Contains(n) && n.Height < 1)//Checks to make sure the neighboor of our current tile is not within closed set, and has a height of less than 1
                {
                    var tempG = current.G + 1;//gets a temp comparison integer for seeing if a route is shorter than our current path

                    bool newPath = false;
                    if (OpenSet.Contains(n)) //Checks if the neighboor we are checking is within the openset
                    {
                        if (tempG < n.G)//The distance to the end goal from this neighboor is shorter so we need a new path
                        {
                            n.G = tempG;
                            newPath = true;
                        }
                    }
                    else//if its not in openSet or closed set, then it IS a new path and we should add it too openset
                    {
                        n.G = tempG;
                        newPath = true;
                        OpenSet.Add(n);
                    }
                    if (newPath)//if it is a newPath caclulate the H and F and set current to the neighboors previous
                    {
                        n.H = Heuristic(n, End);
                        n.F = n.G + n.H;
                        n.previous = current;
                    }
                }
            }

        }
        return null;
    }

    private int Heuristic(Spot a, Spot b)
    {
        //manhattan
        var dx = Math.Abs(a.X - b.X);
        var dy = Math.Abs(a.Y - b.Y);
        return 1 * (dx + dy);

        #region diagonal
        //diagonal
        // Chebyshev distance
        //var D = 1;
        // var D2 = 1;
        //octile distance
        //var D = 1;
        //var D2 = 1;
        //var dx = Math.Abs(a.X - b.X);
        //var dy = Math.Abs(a.Y - b.Y);
        //var result = (int)(1 * (dx + dy) + (D2 - 2 * D));
        //return result;// *= (1 + (1 / 1000));
        //return (int)Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        #endregion
    }
}
public class Spot
{
    public int X;
    public int Y;
    public int F;
    public int G;
    public int H;
    public int Height = 0;
    public List<Spot> Neighbors;
    public Spot previous = null;
    public Spot(int x, int y, int height)
    {
        X = x;
        Y = y;
        F = 0;
        G = 0;
        H = 0;
        Neighbors = new List<Spot>();
        Height = height;
    }
    public void AddNeighbors(Spot[,] grid, int x, int y, bool diagonal)
    {
        int maxX = grid.GetUpperBound(0);
        int maxY = grid.GetUpperBound(1);
        if (x < maxX)
            Neighbors.Add(grid[x + 1, y]);
        if (x > 0)
            Neighbors.Add(grid[x - 1, y]);
        if (y < maxY)
            Neighbors.Add(grid[x, y + 1]);
        if (y > 0)
            Neighbors.Add(grid[x, y - 1]);

        if(diagonal){
            if (x > 0 && y > 0)
                Neighbors.Add(grid[x - 1, y - 1]);
            if (x < maxX && y > 0)
                Neighbors.Add(grid[x + 1, y - 1]);
            if (x > 0 && y < maxY)
                Neighbors.Add(grid[x - 1, y + 1]);
            if (x < maxX && y < maxY)
                Neighbors.Add(grid[x + 1, y + 1]);
        }
    }


}