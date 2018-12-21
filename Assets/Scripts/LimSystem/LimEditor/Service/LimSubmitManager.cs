using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Schwarzer.Lanotalium.WebApi.ChartZone;
using UnityEngine;
using UnityEngine.UI;

public class LimSubmitManager : MonoBehaviour
{
    public static LimSubmitManager Instance;
    public Text MySubmits, NewSubmit, SubmitToChartZone;
    public GameObject SubmitPanel;
    public RectTransform SubmitsContent;
    public GameObject SubmissionPrefab;
    public List<LimSubmissionManager> SubmissionList = new List<LimSubmissionManager>();
    public MessageBoxManager MessageBoxManager;

    private void Start()
    {
        Instance = this;
    }
    public void SetTexts()
    {
        MySubmits.text = LimLanguageManager.TextDict["Submit_MySubmit"];
        NewSubmit.text = LimLanguageManager.TextDict["Submit_NewSubmit"];
        SubmitToChartZone.text = LimLanguageManager.TextDict["Submit_ChartZone"];
        foreach (LimSubmissionManager var in SubmissionList)
        {
            var.SetColorAndStatusTexts();
        }
    }
    public void SwitchPanelActive()
    {
        SubmitPanel.SetActive(!SubmitPanel.activeSelf);
    }
    public void GenerateCorrentSubmissions(int Count)
    {
        int CurrentCount = SubmissionList.Count;
        if (CurrentCount < Count)
        {
            for (int i = 0; i < Count - CurrentCount; ++i)
            {
                SubmissionList.Add(Instantiate(SubmissionPrefab, SubmitsContent).GetComponent<LimSubmissionManager>());
            }
        }
        else if (CurrentCount > Count)
        {
            for (int i = CurrentCount - 1; i >= Count; --i)
            {
                Destroy(SubmissionList[i].gameObject);
                SubmissionList.RemoveAt(i);
            }
        }
    }
    public void GetUserSubmissions()
    {
        StartCoroutine(GetUserSubmissionsCoroutine());
    }
    public IEnumerator GetUserSubmissionsCoroutine()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable) yield break;
        WWW Get = new WWW("https://lanotaliumapi.schwarzer.wang/chartzone/submit/enum/" + SystemInfo.deviceUniqueIdentifier);
        yield return Get;
        if (Get.error != null)
        {
            MessageBoxManager.Instance.ShowMessage(LimLanguageManager.TextDict["Error_Network"] + "\n" + Get.error);
            yield break;
        }
        string Response = Get.text;

        List<SubmitDto> UserSubmits = null;
        try
        {
            UserSubmits = JsonConvert.DeserializeObject<List<SubmitDto>>(Response);
        }
        catch (Exception)
        {
            yield break;
        }
        if (UserSubmits == null) yield break;
        UserSubmits.Reverse();

        int Height = 0;
        GenerateCorrentSubmissions(UserSubmits.Count);
        for (int i = 0; i < UserSubmits.Count; ++i)
        {
            SubmissionList[i].Initialize(UserSubmits[i]);
            SubmissionList[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(0, Height);
            Height -= 80;
        }
        SubmitsContent.sizeDelta = new Vector2(0, -Height);
    }
    public void QueryNewSubmission()
    {
        StartCoroutine(QueryNewSubmissionCoroutine());
    }
    public IEnumerator QueryNewSubmissionCoroutine()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable) yield break;
        WWW Get = new WWW("https://lanotaliumapi.schwarzer.wang/chartzone/submit/querynew/" + SystemInfo.deviceUniqueIdentifier);
        yield return Get;
        if (Get.error != null)
        {
            MessageBoxManager.Instance.ShowMessage(LimLanguageManager.TextDict["Error_Network"] + "\n" + Get.error);
            yield break;
        }
        string Response = Get.text;

        int ResponseCode = 0;
        if (!int.TryParse(Response, out ResponseCode)) yield break;

        if (ResponseCode == -1)
        {
            MessageBoxManager.ShowMessage(LimLanguageManager.TextDict["Submit_TimeLimit"]);
        }
        else if (ResponseCode == -3)
        {
            MessageBoxManager.ShowMessage(LimLanguageManager.TextDict["Submit_TooManyPending"]);
        }
        yield return GetUserSubmissionsCoroutine();
    }
    public IEnumerator DeleteSubmissionCoroutine(int Id)
    {
        if (Application.internetReachability == NetworkReachability.NotReachable) yield break;
        WWW Get = new WWW("https://lanotaliumapi.schwarzer.wang/chartzone/submit/delete/" + SystemInfo.deviceUniqueIdentifier + "/" + Id);
        yield return Get;
        if (Get.error != null)
        {
            MessageBoxManager.Instance.ShowMessage(LimLanguageManager.TextDict["Error_Network"] + "\n" + Get.error);
            yield break;
        }
        yield return GetUserSubmissionsCoroutine();
    }
}
