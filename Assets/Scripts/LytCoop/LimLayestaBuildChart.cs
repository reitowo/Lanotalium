using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LimLayestaBuildChart : MonoBehaviour 
{
    public string Difficulty
    {
        get
        {
            return DifficultyText.text;
        }
    }
    [HideInInspector]
    public string ChartPath;
    public InputField DifficultyText;
    public Text DifficultyHolder, Delete;

    public void Initialize(string chartPath)
    {
        ChartPath = chartPath;
        SetTexts();
    }
    public void SetTexts()
    {
        DifficultyHolder.text = LimLanguageManager.TextDict["Layesta_Build_Chart_Difficulty"];
        Delete.text = LimLanguageManager.TextDict["Layesta_Build_Chart_Delete"];
    }
    public void DeleteThis()
    {
        LimLayestaManager.Instance.BuildRemoveChart(this);
    }
}
