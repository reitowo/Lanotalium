using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class LasyLogger
{
    public static void Log(this object Object)
    {
        Debug.Log(Object);
    }
}

public static class SchwarzerUtility
{
    public static string GetRelativePath(string Path, string RelativeTo)
    {
        Uri Full = new Uri(Path);
        Uri Relative = new Uri(RelativeTo);

        Uri Relatived = Relative.MakeRelativeUri(Full);
        return Relatived.OriginalString;
    }
}