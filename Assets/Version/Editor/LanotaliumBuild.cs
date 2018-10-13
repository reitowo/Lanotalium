using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using System.Diagnostics;

public class LanotaliumBuild
{
    [MenuItem("Lanotalium/Build")]
    public static void BuildLanotalium()
    {
        if (BuildPipeline.BuildPlayer(
             new BuildPlayerOptions()
             {
                 locationPathName = @"E:\Lanotalium\LanotaliumReleases\Latest\x86\Lanotalium\Lanotalium.exe",
                 scenes = new string[] { "Assets/_Scenes/LimFirstRun.unity", "Assets/_Scenes/LimLaunch.unity", "Assets/_Scenes/LimTuner.unity", "Assets/_Scenes/LimChartZone.unity" },
                 target = BuildTarget.StandaloneWindows,
                 options = BuildOptions.None
             }).summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            UnityEngine.Debug.Log("x86 built successfully.");
        if (BuildPipeline.BuildPlayer(
             new BuildPlayerOptions()
             {
                 locationPathName = @"E:\Lanotalium\LanotaliumReleases\Latest\x64\Lanotalium\Lanotalium.exe",
                 scenes = new string[] { "Assets/_Scenes/LimFirstRun.unity", "Assets/_Scenes/LimLaunch.unity", "Assets/_Scenes/LimTuner.unity", "Assets/_Scenes/LimChartZone.unity" },
                 target = BuildTarget.StandaloneWindows64,
                 options = BuildOptions.None
             }).summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            UnityEngine.Debug.Log("x64 built successfully.");
    }

    [MenuItem("Lanotalium/Update")]
    public static void UpdateLanotalium()
    {
        Process.Start("powershell", @"E:\Lanotalium\LanotaliumReleases\Latest\push.ps1");
    }

    [MenuItem("Lanotalium/One Key")]
    public static void OneKeyLanotalium()
    {
        BuildLanotalium();
        UpdateLanotalium();
    }
}

