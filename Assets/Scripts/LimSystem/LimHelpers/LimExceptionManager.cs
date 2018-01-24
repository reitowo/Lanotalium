using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

public class DummyException
{
    public string Message, StackTrace;
    public DummyException(string Message, string StackTrace)
    {
        this.Message = Message;
        this.StackTrace = StackTrace;
    }
    public DummyException(Exception Ex)
    {
        Message = Ex.Message;
        StackTrace = Ex.StackTrace;
    }
}

public class LimExceptionManager : MonoBehaviour
{
    public Text ExceptionText, MessageText, StackTraceText, QueuedExceptionCountText, ShowNextText, RestartText, IgnoreText;
    public Button ShowNextExceptionBtn;
    public RectTransform Banner, StackTraceRect;
    public LimProjectManager ProjectManager;

    private bool IsOpened = false;
    private string ExceptionMessage
    {
        get
        {
            return MessageText.text;
        }
        set
        {
            MessageText.text = value;
        }
    }
    private string ExceptionStackTrace
    {
        get
        {
            return StackTraceText.text;
        }
        set
        {
            StackTraceText.text = value;
            StackTraceRect.sizeDelta = new Vector2(0, value.Split('\n').Length * 21);
        }
    }
    private List<DummyException> QueuedException = new List<DummyException>();
    private Coroutine BannerAnimationCoroutine;

    private void Start()
    {
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        Application.logMessageReceived += OnLogMessageReceived;
    }
    private void Update()
    {
        if (IsOpened)
        {
            if (QueuedException.Count == 0)
            {
                QueuedExceptionCountText.text = "";
                if (ShowNextExceptionBtn.interactable) ShowNextExceptionBtn.interactable = false;
            }
            else
            {
                QueuedExceptionCountText.text = string.Format(LimLanguageManager.TextDict["Exception_QueuedCount"], QueuedException.Count);
                if (!ShowNextExceptionBtn.interactable) ShowNextExceptionBtn.interactable = true;
            }
        }
    }
    public void SetTexts()
    {
        ExceptionText.text = LimLanguageManager.TextDict["Exception_Exception"];
        ShowNextText.text = LimLanguageManager.TextDict["Exception_Next"];
        RestartText.text = LimLanguageManager.TextDict["Exception_Restart"];
        IgnoreText.text = LimLanguageManager.TextDict["Exception_Ignore"];
    }
    private void OnLogMessageReceived(string condition, string stackTrace, LogType type)
    {
        if (type != LogType.Exception) return;
        DummyException Ex = new DummyException(condition, stackTrace);
        if (IsOpened)
        {
            QueuedException.Add(Ex);
            return;
        }
        ExceptionMessage = Ex.Message;
        ExceptionStackTrace = Ex.StackTrace;
        StartBannerAnimation(true);
    }
    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        Exception Ex = e.ExceptionObject as Exception;
        if (IsOpened)
        {
            QueuedException.Add(new DummyException(Ex));
            return;
        }
        ExceptionMessage = Ex.Message;
        ExceptionStackTrace = Ex.StackTrace;
        StartBannerAnimation(true);
    }
    public void ShowNextException()
    {
        if (QueuedException.Count != 0)
        {
            ExceptionMessage = QueuedException[0].Message;
            ExceptionStackTrace = QueuedException[0].StackTrace;
            QueuedException.RemoveAt(0);
        }
    }
    public void CloseExceptionBanner()
    {
        QueuedException.Clear();
        StartBannerAnimation(false);
    }
    public void SaveAndRestartLanotalium()
    {
        ProjectManager.SaveProject();
        if (Application.platform == RuntimePlatform.WindowsEditor) return;
        ProcessStartInfo processStartInfo = new ProcessStartInfo
        {
            FileName = Environment.GetCommandLineArgs()[0],
            Arguments = LimProjectManager.LapPath
        };
        Process.Start(processStartInfo);
    }
    public void StartBannerAnimation(bool Show)
    {
        if (BannerAnimationCoroutine != null) StopCoroutine(BannerAnimationCoroutine);
        if (Show) BannerAnimationCoroutine = StartCoroutine(ShowBannerCoroutine());
        else BannerAnimationCoroutine = StartCoroutine(HideBannerCoroutine());
    }
    IEnumerator ShowBannerCoroutine()
    {
        IsOpened = true;
        Banner.gameObject.SetActive(true);
        float Start = Banner.sizeDelta.y;
        float Delta = 600 - Start;
        float Duration = 0.3f;
        while (Duration > 0)
        {
            Banner.sizeDelta = new Vector2(Banner.sizeDelta.x, Start + Delta * (0.3f - Duration) / 0.3f);
            Duration -= Time.deltaTime;
            yield return null;
        }
        Banner.sizeDelta = new Vector2(Banner.sizeDelta.x, 600);
    }
    IEnumerator HideBannerCoroutine()
    {
        IsOpened = false;
        float Start = Banner.sizeDelta.y;
        float Delta = 0 - Start;
        float Duration = 0.3f;
        while (Duration > 0)
        {
            Banner.sizeDelta = new Vector2(Banner.sizeDelta.x, Start + Delta * (0.3f - Duration) / 0.3f);
            Duration -= Time.deltaTime;
            yield return null;
        }
        Banner.sizeDelta = new Vector2(Banner.sizeDelta.x, 0);
        Banner.gameObject.SetActive(false);
    }
}
