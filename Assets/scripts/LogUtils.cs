using UnityEngine;
using System.Collections.Generic;

public class LogUtils
{
    public static void LogEnumerable<T>(string msg, IEnumerable<T> enumerable)
    {
        Debug.Log(msg + ": " + string.Join("  ", enumerable));
    }
}
