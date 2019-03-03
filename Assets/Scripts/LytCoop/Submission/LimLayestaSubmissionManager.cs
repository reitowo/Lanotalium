using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class LimLayestaSubmissionManager : MonoBehaviour
{
    public static LimLayestaSubmissionManager Instance { get; private set; }
    private void Awake()
    {
        LimLanguageManager.OnLanguageChanged.AddListener(SetTexts);
        Instance = this;
    }

    public GameObject LoginPanel;
    public InputField Username, Password, ConfirmPassword, Email;
    public Text TitleText, UsernameText, PasswordText, ConfirmPasswordText, EmailText, SwitchText, DoText;
    public Text Message;
    public GameObject Cell;
    public RectTransform CellRect;

    private List<LimLayestaSubmissionLevel> cells = new List<LimLayestaSubmissionLevel>();
    private bool isRegisterMode = false;

    public string UserInfoPath
    {
        get
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/Lanotalium/SchwarzerWangAuth.json";
        }
    }
    public string LoginBasicAuth
    {
        get
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(Username.text + ":" + Password.text));
        }
    }
    public string RegisterBasicAuth
    {
        get
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(Username.text + ":" + Password.text + ":" + Email.text));
        }
    }
    [HideInInspector]
    public string Bearer;
    [HideInInspector]
    public string Id;

    private void OnDestroy()
    {
        SaveUserInfo();
        LimLanguageManager.OnLanguageChanged.RemoveListener(SetTexts);
    }
    private void Start()
    {
        LoadUserInfo();
        if (!string.IsNullOrWhiteSpace(Bearer) && !string.IsNullOrWhiteSpace(Id))
        {
            LoginPanel.SetActive(false);
            Refresh();
        }
    }

    public void TokenInvalid()
    {
        Bearer = null;
        EmptyList();
        LoginPanel.SetActive(true);
        Message.text = LimLanguageManager.TextDict["Layesta_Submission_User_InvalidToken"];
    }
    public void SetTexts()
    {
        TitleText.text = isRegisterMode ? LimLanguageManager.TextDict["Layesta_Submission_User_Register"] : LimLanguageManager.TextDict["Layesta_Submission_User_Login"];
        UsernameText.text = LimLanguageManager.TextDict["Layesta_Submission_User_Username"];
        PasswordText.text = LimLanguageManager.TextDict["Layesta_Submission_User_Password"];
        ConfirmPasswordText.text = LimLanguageManager.TextDict["Layesta_Submission_User_ConfirmPassword"];
        EmailText.text = LimLanguageManager.TextDict["Layesta_Submission_User_Email"];
        SwitchText.text = isRegisterMode ? LimLanguageManager.TextDict["Layesta_Submission_User_Switch_Login"] : LimLanguageManager.TextDict["Layesta_Submission_User_Switch_Register"];
        DoText.text = isRegisterMode ? LimLanguageManager.TextDict["Layesta_Submission_User_Register"] : LimLanguageManager.TextDict["Layesta_Submission_User_Login"];
    }
    public void Login()
    {
        StartCoroutine(LoginCoroutine());
    }
    IEnumerator LoginCoroutine()
    {
        Message.text = LimLanguageManager.TextDict["Layesta_Submission_User_Wait"];
        UnityWebRequest web = new UnityWebRequest
        {
            downloadHandler = new DownloadHandlerBuffer(),
            url = "https://la.schwarzer.wang/auth/login",
            method = "GET"
        };
        web.SetRequestHeader("Authorization", $"Basic {LoginBasicAuth}");
        yield return web.SendWebRequest();
        if (!string.IsNullOrWhiteSpace(web.error))
        {
            Message.text = web.error;
            yield break;
        }
        string ret = web.downloadHandler.text;
        LoginAuthResponse response = JsonConvert.DeserializeObject<LoginAuthResponse>(ret);
        if (!response.Succeed)
        {
            Message.text = response.ErrorMessage;
            yield break;
        }
        Bearer = response.AccessToken;
        Id = response.Id;
        LoginPanel.SetActive(false);
        Refresh();
    }
    public void Register()
    {
        StartCoroutine(RegisterCoroutine());
    }
    IEnumerator RegisterCoroutine()
    {
        Message.text = LimLanguageManager.TextDict["Layesta_Submission_User_Wait"];
        UnityWebRequest web = new UnityWebRequest
        {
            downloadHandler = new DownloadHandlerBuffer(),
            url = "https://la.schwarzer.wang/auth/register",
            method = "GET"
        };
        web.SetRequestHeader("Authorization", $"Basic {RegisterBasicAuth}");
        yield return web.SendWebRequest();
        if (!string.IsNullOrWhiteSpace(web.error))
        {
            Message.text = web.error;
            yield break;
        }
        string ret = web.downloadHandler.text;
        LoginAuthResponse response = JsonConvert.DeserializeObject<LoginAuthResponse>(ret);
        if (!response.Succeed)
        {
            Message.text = response.ErrorMessage;
            yield break;
        }
        Bearer = response.AccessToken;
        Id = response.Id;
        LoginPanel.SetActive(false);
        yield return null;
    }
    public void Refresh()
    {
        StartCoroutine(RefreshCoroutine());
    }
    IEnumerator RefreshCoroutine()
    {
        UnityWebRequest web = new UnityWebRequest
        {
            downloadHandler = new DownloadHandlerBuffer(),
            url = "https://la.schwarzer.wang/layestalevel/list/current",
            method = "GET"
        };
        web.SetRequestHeader("Authorization", $"Bearer {Bearer}");
        yield return web.SendWebRequest();
        if (web.responseCode == 401)
        {
            TokenInvalid();
            yield break;
        }
        if (!string.IsNullOrWhiteSpace(web.error))
        {
            yield break;
        }
        string ret = web.downloadHandler.text;
        JObject response = JObject.Parse(ret);
        if (!response["Succeed"].Value<bool>())
        {
            MessageBoxManager.Instance.ShowMessage(((ErrorCode)response["ErrorCode"].Value<int>()).ToString());
            yield break;
        }
        List<LayestaLevelDto> list = response["Levels"].ToObject<List<LayestaLevelDto>>();
        EmptyList();
        float t = 0;
        foreach (var l in list)
        {
            GameObject g = Instantiate(Cell, CellRect);
            LimLayestaSubmissionLevel c = g.GetComponent<LimLayestaSubmissionLevel>();
            c.Initialize(l);
            cells.Add(c);
            g.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, t);
            t -= g.GetComponent<RectTransform>().sizeDelta.y;
        }
        CellRect.sizeDelta = new Vector2(0, -t);
        yield return null;
    }
    public void Add()
    {
        StartCoroutine(AddCoroutine());
    }
    IEnumerator AddCoroutine()
    {
        UnityWebRequest web = new UnityWebRequest
        {
            downloadHandler = new DownloadHandlerBuffer(),
            url = "https://la.schwarzer.wang/layestalevel/create",
            method = "GET"
        };
        web.SetRequestHeader("Authorization", $"Bearer {Bearer}");
        yield return web.SendWebRequest();
        if (web.responseCode == 401)
        {
            TokenInvalid();
            yield break;
        }
        if (!string.IsNullOrWhiteSpace(web.error))
        {
            yield break;
        }
        string ret = web.downloadHandler.text;
        JObject response = JObject.Parse(ret);
        if (response["Succeed"] == null || !response["Succeed"].Value<bool>())
        {
            switch (response["ErrorCode"].Value<int>())
            {
                case 201:
                    MessageBoxManager.Instance.ShowMessage(LimLanguageManager.TextDict["Layesta_Submission_IntervalLimit"]);
                    break;
                default:
                    MessageBoxManager.Instance.ShowMessage(((ErrorCode)response["ErrorCode"].Value<int>()).ToString());
                    break;
            }
            yield break;
        }
        List<LayestaLevelDto> list = response["Levels"].ToObject<List<LayestaLevelDto>>();
        EmptyList();
        float t = 0;
        foreach (var l in list)
        {
            GameObject g = Instantiate(Cell, CellRect);
            LimLayestaSubmissionLevel c = g.GetComponent<LimLayestaSubmissionLevel>();
            c.Initialize(l);
            cells.Add(c);
            g.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, t);
            t -= 200;
        }
        CellRect.sizeDelta = new Vector2(0, -t);
        yield return null;
    }

    public void EmptyList()
    {
        foreach (var l in cells)
        {
            Destroy(l.gameObject);
        }
        cells.Clear();
    }

    public void Switch()
    {
        isRegisterMode = !isRegisterMode;
        ConfirmPassword.gameObject.SetActive(isRegisterMode);
        Email.gameObject.SetActive(isRegisterMode);
        SetTexts();
    }
    public void Do()
    {
        if (isRegisterMode) Register();
        else Login();
    }

    private void LoadUserInfo()
    {
        if (File.Exists(UserInfoPath))
        {
            try
            {
                LayestaSubmissionPerf pref = JsonConvert.DeserializeObject<LayestaSubmissionPerf>(File.ReadAllText(UserInfoPath));
                Bearer = pref.Bearer;
                Id = pref.Id;
            }
            catch (Exception)
            {

            }
        }
    }
    private void SaveUserInfo()
    {
        File.WriteAllText(UserInfoPath, JsonConvert.SerializeObject(new LayestaSubmissionPerf { Bearer = Bearer, Id = Id }));
    }
}

public class LayestaSubmissionPerf
{
    public string Bearer;
    public string Id;
}

public class LoginAuthResponse
{
    public string Id;
    public string AccessToken;
    public bool Succeed;
    public ErrorCode ErrorCode;
    public string ErrorMessage;
}

public class OSSAuthRequest
{
    public string LevelGuid;
}

public class OSSAuthResponse
{
    public string Uri;
    public string Callback;
    public bool Succeed;
    public ErrorCode ErrorCode;
}

public enum ErrorCode
{
    NotSet = 0,
    InvalidAuth = -1,
    Banned = -2,

    //Auth
    SchwarzerUserNotExist = 100,
    WrongPassword = 101,
    UserIsNotLevelCreator = 102,
    OSSAuthError = 103,
    InvalidType = 104,
    LevelNotFound = 105,
    MissingInfo = 106,

    IntervalLimit = 201,
    LayestaUserNotFound = 202,
    InvalidDto = 203
}

public class LayestaLevelResponse
{
    public bool Succeed;
    public ErrorCode ErrorCode;
    public string Message;
    public LayestaLevelDto Level;
}

public class LayestaLevelDto
{
    public string Title { get; set; }
    public string Guid { get; set; }
    public string SongArtist { get; set; }
    public string Difficulties { get; set; }
    public int DownloadCount { get; set; }
    public bool ShouldDisplay { get; set; }

    public bool ParticipantCurrentContest { get; set; }
}

public class LevelListResponse
{
    public bool Succeed;
    public List<LayestaLevelDto> Levels;
    public ErrorCode ErrorCode;
}