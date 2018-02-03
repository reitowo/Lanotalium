using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schwarzer.Chart;

public class LimChartConverting : MonoBehaviour
{
    public WindowsDialogUtility WindowsDialogUtility;
    private Coroutine ConvertingCoroutine;

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
    public void ConvertFromBms()
    {
        if (ConvertingCoroutine != null) StopCoroutine(ConvertingCoroutine);
        ConvertingCoroutine = StartCoroutine(ConvertFromBmsCoroutine());
    }
}
