using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schwarzer.Chart;

public class LimChartConverting : MonoBehaviour
{
    public WindowsDialogUtility WindowsDialogUtility;
    private Coroutine convertingCoroutine;

    IEnumerator ConvertFromBmsCoroutine()
    {
        string BmsChartPath = WindowsDialogUtility.OpenFileDialog(LimLanguageManager.TextDict["ChartConverting_BmsTitle"], LimLanguageManager.TextDict["ChartConverting_BmsExtension"], null);
        if (BmsChartPath == null) yield break;
        ChartConvertTask chartConvertTask = ChartConvert.BmsToLanotaliumAsync(BmsChartPath, false);
        WindowsDialogUtility.ProgressBar.ShowProgress(() => { return chartConvertTask.Task.IsCompleted || chartConvertTask.Task.IsFaulted; }, () => { return chartConvertTask.Percent / 100; });
        while (!chartConvertTask.Task.IsCompleted)
        {
            yield return null;
        }
        if (chartConvertTask.Task.Exception != null) throw chartConvertTask.Task.Exception;
        WindowsDialogUtility.OpenExplorer(chartConvertTask.LapPath);
    }
    IEnumerator ConvertFromArcaeaCoroutine()
    {
        string ArcaeaAffPath = WindowsDialogUtility.OpenFileDialog("Open Aff File", "Arcaea File Format (*.aff)|*.aff", null);
        if(ArcaeaAffPath == null) yield break;
        ChartConvert.ArcaeaToLanota(ArcaeaAffPath);
        WindowsDialogUtility.OpenExplorer(ArcaeaAffPath.Replace(".aff", "_convert.txt"));
    }
    public void ConvertFromBms()
    {
        if (convertingCoroutine != null) StopCoroutine(convertingCoroutine);
        convertingCoroutine = StartCoroutine(ConvertFromBmsCoroutine());
    }
    public void ConvertFromArcaea()
    {
        if (convertingCoroutine != null) StopCoroutine(convertingCoroutine);
        convertingCoroutine = StartCoroutine(ConvertFromArcaeaCoroutine());
    }
}
