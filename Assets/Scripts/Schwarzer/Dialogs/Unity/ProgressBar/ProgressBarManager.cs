using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBarManager : MonoBehaviour
{
    public static ProgressBarManager Instance;
    public Text ProgressText;
    public Slider ProgressSlider;
    public GameObject Canvas;
    [HideInInspector]
    public float Percent;

    private Coroutine UpdateProgressCoroutine = null;

    public delegate float AskProgressCallback();
    public delegate bool AskFinishCallback();

    private void Start()
    {
        Instance = this;
    }
    public void ShowProgress(AskFinishCallback Finish, AskProgressCallback Callback = null)
    {
        if (UpdateProgressCoroutine != null) StopCoroutine(UpdateProgressCoroutine);
        UpdateProgressCoroutine = StartCoroutine(UpdateProgress(Finish, Callback));
    }
    IEnumerator UpdateProgress(AskFinishCallback Finish, AskProgressCallback Callback)
    {
        Canvas.SetActive(true);
        while (!Finish())
        {
            if (Callback != null) Percent = Callback();
            ProgressSlider.value = Percent;
            ProgressText.text = (Percent * 100).ToString("f2") + "%";
            yield return null;
        }
        Canvas.SetActive(false);
    }
}
