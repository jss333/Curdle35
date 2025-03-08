using UnityEngine;
using System.Collections.Generic;

public class LogUtils
{
    public static void LogEnumerable<T>(string msg, IEnumerable<T> enumerable)
    {
        Debug.Log(msg + ": " + string.Join("  ", enumerable));
    }

    public static void Print2DArray(int[,] array)
    {
        int rows = array.GetLength(0);
        int cols = array.GetLength(1);

        string output = "";
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                output += array[i, j] + " ";
            }
            output += "\n"; // New line for rows
        }
        Debug.Log(output);
    }
}
