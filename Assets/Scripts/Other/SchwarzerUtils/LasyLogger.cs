using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LasyLogger
{
    public static void Log(this object Object)
    {
        Debug.Log(Object);
    }
}
