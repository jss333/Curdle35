using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BoardOuterInnerCellsCalculator
{
    private CellData[,] board;
    private int width;
    private int height;

    private HashSet<Vector2Int> cellsCloseToHQ;
    private List<Vector2Int> outerCells;
    private HashSet<Vector2Int> outerCellsSet;
    private List<Vector2Int> innerCells;

    private Tuple<Vector2Int, int>[] directionsToCheck;
    private Vector2Int currentCell;
    private int indexNextDirectionToCheck;

    public BoardOuterInnerCellsCalculator(Vector2Int hqCell, int minDistFromHq)
    {
        BoardManager boardMngr = BoardManager.Instance;
        board = boardMngr.GetBoard();
        width = boardMngr.GetBoardWidth();
        height = boardMngr.GetBoardHeight();

        CalculateCellsCloseToHQ(hqCell, minDistFromHq);
        CalculateOuterCellsAwayFromHQ();
        CalculateInnerCellsAwayFromHQ();
    }

    private void CalculateCellsCloseToHQ(Vector2Int hqCell, int minDistFromHQ)
    {
        cellsCloseToHQ = new HashSet<Vector2Int>();
        for (int x = -minDistFromHQ; x <= minDistFromHQ; x++)
        {
            int maxY = minDistFromHQ - Math.Abs(x);
            for (int y = -maxY; y <= maxY; y++)
            {
                cellsCloseToHQ.Add(new Vector2Int(hqCell.x + x, hqCell.y + y));
            }
        }
    }

    #region Outer Cells Calculation

    private void CalculateOuterCellsAwayFromHQ()
    {
        InitializeDirectionsToCheck();

        // find all the outer cells of the map (ie, those that have at least a neighbour that is a void cell)
        // starting from a cell that has a void northern border and going clockwise

        outerCells = new List<Vector2Int>();
        outerCellsSet = new HashSet<Vector2Int>();

        Vector2Int startOuterCellWithNorthenBorder = FindOuterCellWithNorthernBorder();

        currentCell = startOuterCellWithNorthenBorder;
        outerCells.Add(currentCell);
        outerCellsSet.Add(currentCell);
        indexNextDirectionToCheck = 0;

        CheckDirectionsForLandTiles();

        // continue checking until we come back to the starting cell
        while (!currentCell.Equals(startOuterCellWithNorthenBorder))
        {
            CheckDirectionsForLandTiles();
        }
    }

    private void InitializeDirectionsToCheck()
    {
        // First element is the direction to check, second element is the index of the next direction to check from the next tile
        directionsToCheck = new Tuple<Vector2Int, int>[8];
        directionsToCheck[0] = Tuple.Create(new Vector2Int(1, 1), 6);
        directionsToCheck[1] = Tuple.Create(new Vector2Int(1, 0), 0);
        directionsToCheck[2] = Tuple.Create(new Vector2Int(1, -1), 0);
        directionsToCheck[3] = Tuple.Create(new Vector2Int(0, -1), 2);
        directionsToCheck[4] = Tuple.Create(new Vector2Int(-1, -1), 2);
        directionsToCheck[5] = Tuple.Create(new Vector2Int(-1, 0), 4);
        directionsToCheck[6] = Tuple.Create(new Vector2Int(-1, 1), 4);
        directionsToCheck[7] = Tuple.Create(new Vector2Int(0, 1), 6);
    }

    private Vector2Int FindOuterCellWithNorthernBorder()
    {
        //Iterate checking each northern row in turn until we find a cell that is not void which has a void cell above it
        for (int y = height - 1; y >= 0; y--)
        {
            for (int x = 0; x < width; x++)
            {
                if (!board[x, y].IsVoidCell())
                {
                    return new Vector2Int(x, y);
                }
            }
        }

        Debug.LogError("No outer cell found with northern border");
        return new Vector2Int();
    }

    private void CheckDirectionsForLandTiles()
    {
        for (int n = 0; n < directionsToCheck.Length; n++)
        {
            Tuple<Vector2Int, int> nextDirectionToCheck = directionsToCheck[(indexNextDirectionToCheck + n) % directionsToCheck.Length];
            Vector2Int candidateCell = currentCell + nextDirectionToCheck.Item1;

            if (BoardManager.Instance.IsValidCellForUnitMovement(candidateCell))
            {
                if (!outerCells.Contains(candidateCell) && !cellsCloseToHQ.Contains(candidateCell))
                {
                    outerCells.Add(candidateCell);
                    outerCellsSet.Add(candidateCell);
                }

                currentCell = candidateCell;
                indexNextDirectionToCheck = nextDirectionToCheck.Item2; // The index of the next direction to check from the next tile
                break;
            }
        }
    }

    #endregion

    #region Inner Cells Calculation

    private void CalculateInnerCellsAwayFromHQ()
    {
        // build a list of all inner cells that are a minimun distance away from HQ
        innerCells = new List<Vector2Int>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2Int candidateCell = new Vector2Int(x, y);
                bool isValidCell = BoardManager.Instance.IsValidCellForUnitMovement(candidateCell);
                bool isCloseToHQ = cellsCloseToHQ.Contains(candidateCell);
                bool isOuterCell = outerCellsSet.Contains(candidateCell);

                if (isValidCell && !isCloseToHQ && !isOuterCell)
                {
                    innerCells.Add(candidateCell);
                }
            }
        }
    }

    #endregion

    public List<Vector2Int> GetOuterCellsAwayFromHQ()
    {
        return outerCells;
    }

    public List<Vector2Int> GetInnerCellsAwayFromHQ()
    {
        return innerCells;
    }
}
