using System;
using System.Collections.Generic;
using UnityEngine;

public class BoardOuterInnerCellsCalculator
{
    private List<Tuple<int, int>> outerCells;
    private HashSet<Tuple<int, int>> outerCellsSet;
    private List<Tuple<int, int>> innerCellsAwayFromHQ;

    private int[,] board;
    private int width;
    private int height;

    private Tuple<int, int, int>[] directionsToCheck;
    private Tuple<int, int> currentCell;
    private int indexNextDirectionToCheck;


    public BoardOuterInnerCellsCalculator(int[,] board, Tuple<int, int> hqCell, int minDistFromHQ)
    {
        this.board = board;
        this.width = board.GetLength(0);
        this.height = board.GetLength(1);

        CalculateOuterCellsInClockwiseOrder();
        CalculateInnerCellsAwayFromHQ(hqCell, minDistFromHQ);
    }

    public List<Tuple<int, int>> GetOuterCells()
    {
        return outerCells;
    }

    public List<Tuple<int, int>> GetInnerCellsAwayFromHQ()
    {
        return innerCellsAwayFromHQ;
    }


    private void CalculateOuterCellsInClockwiseOrder()
    {
        outerCells = new List<Tuple<int, int>>();
        outerCellsSet = new HashSet<Tuple<int, int>>();

        Tuple<int, int> startOuterCellWithNorthenBorder = FindOuterCellWithNorthernBorder();
        
        currentCell = startOuterCellWithNorthenBorder;
        outerCells.Add(currentCell);
        outerCellsSet.Add(currentCell);
        indexNextDirectionToCheck = 0;

        InitializeDirectionsToCheck();
        CheckDirectionsForLandTiles();

        while (!currentCell.Equals(startOuterCellWithNorthenBorder))
        {
            CheckDirectionsForLandTiles();
        }
    }

    private Tuple<int, int> FindOuterCellWithNorthernBorder()
    {
        //Iterate checking each northern row in turn until we find a cell that has value > -1 and no cell or a -1 cell above it
        //Debug.Log("findin nTuple...");
        for (int y = height - 1; y >= 0; y--)
        {
            for (int x = 0; x < width; x++)
            {
                //Debug.Log("checking " + x + ", " + y + "= " + board[x, y]);
                if (board[x, y] > 0)
                {
                    //Debug.Log("checking above....");
                    //Debug.Log("--- y+1 > logHeight-1 === " + (y + 1 > logHeight - 1));
                    //Debug.Log("--- board[x, y+1] == -1 === " + (board[x, y + 1] == -1));
                    if ((y + 1 > height - 1) || (board[x, y + 1] == -1))
                    {
                        return Tuple.Create(x, y);
                    }
                }
            }
        }
        return null;
    }

    private void InitializeDirectionsToCheck()
    {
        directionsToCheck = new Tuple<int, int, int>[8];
        directionsToCheck[0] = Tuple.Create(1, 1, 6);
        directionsToCheck[1] = Tuple.Create(1, 0, 0);
        directionsToCheck[2] = Tuple.Create(1, -1, 0);
        directionsToCheck[3] = Tuple.Create(0, -1, 2);
        directionsToCheck[4] = Tuple.Create(-1, -1, 2);
        directionsToCheck[5] = Tuple.Create(-1, 0, 4);
        directionsToCheck[6] = Tuple.Create(-1, 1, 4);
        directionsToCheck[7] = Tuple.Create(0, 1, 6);
    }

    private void CheckDirectionsForLandTiles()
    {
        for (int n = 0; n < directionsToCheck.Length; n++)
        {
            Tuple<int, int, int> nextDirectionToCheck = directionsToCheck[(indexNextDirectionToCheck + n) % directionsToCheck.Length];
            int nextX = currentCell.Item1 + nextDirectionToCheck.Item1;
            int nextY = currentCell.Item2 + nextDirectionToCheck.Item2;

            if (ThereIsLandTileAtPosition(nextX, nextY))
            {
                currentCell = Tuple.Create(nextX, nextY);
                if (!outerCells.Contains(currentCell))
                {
                    outerCells.Add(currentCell);
                    outerCellsSet.Add(currentCell);
                }
                indexNextDirectionToCheck = nextDirectionToCheck.Item3; // The index of the next direction to check from the next tile
                break;
            }
        }
    }

    private void CalculateInnerCellsAwayFromHQ(Tuple<int, int> hqCell, int minDistFromHQ)
    {
        // build a list of all inner cells that are a minimun distance away from HQ
        innerCellsAwayFromHQ = new List<Tuple<int, int>>();

        // determine cells that are too close to HQ
        HashSet<Tuple<int, int>> cellsCloseToHQ = CalculateCellsCloseToHQ(hqCell, minDistFromHQ);

        for (int x = 0; x < board.GetLength(0); x++)
        {
            for (int y = 0; y < board.GetLength(1); y++)
            {
                Tuple<int, int> candidateCell = Tuple.Create(x, y);
                if (ThereIsLandTileAtPosition(x, y) && !cellsCloseToHQ.Contains(candidateCell) && !outerCellsSet.Contains(candidateCell))
                {
                    innerCellsAwayFromHQ.Add(candidateCell);
                }
            }
        }
    }

    private static HashSet<Tuple<int, int>> CalculateCellsCloseToHQ(Tuple<int, int> hqCell, int minDistFromHQ)
    {
        HashSet<Tuple<int, int>> cellsCloseToHQ = new HashSet<Tuple<int, int>>();
        for (int x = -minDistFromHQ; x <= minDistFromHQ; x++)
        {
            int maxY = minDistFromHQ - Math.Abs(x);
            for (int y = -maxY; y <= maxY; y++)
            {
                cellsCloseToHQ.Add(Tuple.Create(hqCell.Item1 + x, hqCell.Item2 + y));
            }
        }

        //LogUtils.LogEnumerable("cells Close To HQ", cellsCloseToHQ);
        return cellsCloseToHQ;
    }

    private bool ThereIsLandTileAtPosition(int nextX, int nextY)
    {
        if (nextX < 0 || nextX > width - 1)
        {
            return false;
        }
        if (nextY < 0 || nextY > height - 1)
        {
            return false;
        }

        return board[nextX, nextY] >= 0;
    }
}
