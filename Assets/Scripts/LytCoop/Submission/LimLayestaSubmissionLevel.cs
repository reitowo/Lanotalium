using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class LimLayestaSubmissionLevel : MonoBehaviour
{
    public Text Title, Artist, Difficulties, Online;
    public Toggle ShouldDisplay;

    private LayestaLevelDto level;

    public void Initialize(LayestaLevelDto level)
    {
        this.level = level;
        Title.text = string.IsNullOrWhiteSpace(level.Title) ? LimLanguageManager.TextDict["Cloud_GetMTime_NotUploadedBefore"] : level.Title;
        Artist.text = level.SongArtist;
        Difficulties.text = level.Difficulties;
        Online.text = string.Format(LimLanguageManager.TextDict["Layesta_Submission_Online"], level.DownloadCount);
        ShouldDisplay.isOn = level.ShouldDisplay;
    }

    public void Upload()
    {
        StartCoroutine(UploadCoroutine());
    }
    public void Download()
    {
        StartCoroutine(DownloadCoroutine());
    }
    public void Delete()
    {
        MessageBoxManager.Instance.ShowMessage(LimLanguageManager.TextDict["Layesta_Submission_ConfirmDelete"], () => StartCoroutine(DeleteCoroutine()));
    }
    public void UpdateInfo()
    {
        StartCoroutine(UpdateInfoCoroutine());
    }

    private void UpdateUploadStatus(string langCode)
    {
        MessageBoxManager.Instance.Message.text += LimLanguageManager.TextDict[langCode].Replace("<br>", "\n");
    }
    IEnumerator UploadCoroutine()
    {
        MessageBoxManager.Instance.ShowMessage(LimLanguageManager.TextDict["Layesta_Submission_Upload1"].Replace("<br>", "\n"));
        yield return null;
        string path = WindowsDialogUtility.OpenFileDialog(LimLanguageManager.TextDict["Layesta_Submission_Load"],
#if UNITY_EDITOR
            "layesta"
#else
            "Layesta File|*.layesta"
#endif
            , null);
        if (path == null) yield break;
        bool validate = LimLayestaReader.Validate(path);
        if (!validate)
        {
            UpdateUploadStatus("Layesta_Submission_UploadErr1");
            yield break;
        }
        byte[] img = LimLayestaReader.LoadBackgroundImage(path);
        MemoryStream ms = new MemoryStream(img);
        MemoryStream msOut = new MemoryStream();
        Kaliko.ImageLibrary.KalikoImage k = new Kaliko.ImageLibrary.KalikoImage(ms);
        k = k.Scale(new Kaliko.ImageLibrary.Scaling.PadScaling(640, 360));
        k.SaveJpg(msOut, 80);
        ms.Close();
        byte[] cov = msOut.GetBuffer();
        msOut.Close();
        UpdateUploadStatus("Layesta_Submission_Upload2");
        #region Get Layesta Upload
        UnityWebRequest web = new UnityWebRequest
        {
            downloadHandler = new DownloadHandlerBuffer(),
            url = $"https://la.schwarzer.wang/auth/oss/upload/layesta/{level.Guid}",
            method = "GET"
        };
        web.SetRequestHeader("Authorization", $"Bearer {LimLayestaSubmissionManager.Instance.Bearer}");
        yield return web.SendWebRequest();
        if (web.responseCode == 401)
        {
            LimLayestaSubmissionManager.Instance.TokenInvalid();
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
        #endregion
        string layestaUrl = response["Uri"].Value<string>();
        string layestaCb = response["Callback"].Value<string>();
        UpdateUploadStatus("Layesta_Submission_Upload3");
        #region Get Cover Upload
        web = new UnityWebRequest
        {
            downloadHandler = new DownloadHandlerBuffer(),
            url = $"https://la.schwarzer.wang/auth/oss/upload/cover/{level.Guid}",
            method = "GET"
        };
        web.SetRequestHeader("Authorization", $"Bearer {LimLayestaSubmissionManager.Instance.Bearer}");
        yield return web.SendWebRequest();
        if (web.responseCode == 401)
        {
            LimLayestaSubmissionManager.Instance.TokenInvalid();
            yield break;
        }
        if (!string.IsNullOrWhiteSpace(web.error))
        {
            yield break;
        }
        ret = web.downloadHandler.text;
        response = JObject.Parse(ret);
        if (!response["Succeed"].Value<bool>())
        {
            MessageBoxManager.Instance.ShowMessage(((ErrorCode)response["ErrorCode"].Value<int>()).ToString());
            yield break;
        }
        #endregion
        string coverUrl = response["Uri"].Value<string>();
        string coverCb = response["Callback"].Value<string>();
        UpdateUploadStatus("Layesta_Submission_Upload4");
        #region Upload Layesta
        web = new UnityWebRequest
        {
            downloadHandler = new DownloadHandlerBuffer(),
            uploadHandler = new UploadHandlerRaw(File.ReadAllBytes(path)),
            url = layestaUrl,
            method = "PUT"
        };
        web.SetRequestHeader("Content-Type", "");
        web.SetRequestHeader("User-Agent", LimLayestaSubmissionManager.Instance.Id);
        web.SetRequestHeader("x-oss-callback", layestaCb);
        yield return web.SendWebRequest();
        ProgressBarManager.Instance.ShowProgress(() => web.isDone, () => web.uploadProgress);
        if (!string.IsNullOrWhiteSpace(web.error))
        {
            UpdateUploadStatus("Layesta_Submission_UploadErr2");
            Debug.Log(web.downloadHandler.text);
            yield break;
        }
        ret = web.downloadHandler.text;
        Debug.Log(ret);
        response = JObject.Parse(ret);
        if (response["Message"].Value<string>() != "Succeed")
        {
            UpdateUploadStatus("Layesta_Submission_UploadErr2");
            MessageBoxManager.Instance.Message.text += response["Message"].Value<string>();
            yield break;
        }
        #endregion
        UpdateUploadStatus("Layesta_Submission_Upload5");
        #region Upload Cover
        web = new UnityWebRequest
        {
            downloadHandler = new DownloadHandlerBuffer(),
            uploadHandler = new UploadHandlerRaw(cov),
            url = coverUrl,
            method = "PUT"
        };
        web.SetRequestHeader("Content-Type", "");
        web.SetRequestHeader("User-Agent", LimLayestaSubmissionManager.Instance.Id);
        web.SetRequestHeader("x-oss-callback", coverCb);
        yield return web.SendWebRequest();
        ProgressBarManager.Instance.ShowProgress(() => web.isDone, () => web.uploadProgress);
        if (!string.IsNullOrWhiteSpace(web.error))
        {
            UpdateUploadStatus("Layesta_Submission_UploadErr2");
            Debug.Log(web.downloadHandler.text);
            yield break;
        }
        ret = web.downloadHandler.text;
        Debug.Log(ret);
        response = JObject.Parse(ret);
        if (response["Message"].Value<string>() != "Succeed")
        {
            UpdateUploadStatus("Layesta_Submission_UploadErr2");
            MessageBoxManager.Instance.Message.text += response["Message"].Value<string>();
            yield break;
        }
        #endregion
        UpdateUploadStatus("Layesta_Submission_Upload6");
        #region Update Info 
        byte[] buf = lzip.entry2Buffer(path, "info.bytes");
        ms = new MemoryStream(buf);
        BinaryReader br = new BinaryReader(ms);
        level.ShouldDisplay = ShouldDisplay.isOn;
        level.Title = br.ReadString();
        br.ReadString();
        int count = br.ReadInt32();
        for (int i = 0; i < count; ++i)
        {
            level.Difficulties += (br.ReadString() + ((i == count - 1) ? "" : " "));
        }
        br.ReadInt32();
        br.ReadInt32();
        if (br.BaseStream.Length > br.BaseStream.Position)
        {
            level.SongArtist = br.ReadString();
        }
        if (br.BaseStream.Length > br.BaseStream.Position)
        {
            br.ReadInt32();
        }
        br.Close();
        ms.Close();
        web = new UnityWebRequest
        {
            downloadHandler = new DownloadHandlerBuffer(),
            uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(level))),
            url = $"https://la.schwarzer.wang/layestalevel/update",
            method = "POST"
        };
        web.SetRequestHeader("Authorization", $"Bearer {LimLayestaSubmissionManager.Instance.Bearer}");
        web.SetRequestHeader("Content-Type", $"application/json; charset=utf-8");
        web.SetRequestHeader("Accept", $"application/json; charset=utf-8");
        yield return web.SendWebRequest();
        if (!string.IsNullOrWhiteSpace(web.error))
        {
            UpdateUploadStatus("Layesta_Submission_UploadErr2");
            yield break;
        }
        Debug.Log(web.downloadHandler.text);
        #endregion
        UpdateUploadStatus("Layesta_Submission_Upload7");
        LimLayestaSubmissionManager.Instance.EmptyList();
        LimLayestaSubmissionManager.Instance.Refresh();
    }
    IEnumerator DownloadCoroutine()
    {
        string path = WindowsDialogUtility.SaveFileDialog(LimLanguageManager.TextDict["Layesta_Submission_Save"],
#if UNITY_EDITOR
            "layesta"
#else
            "Layesta File|*.layesta"
#endif
            , null);
        if (path == null) yield break;
        UnityWebRequest web = new UnityWebRequest
        {
            downloadHandler = new DownloadHandlerBuffer(),
            url = $"https://la.schwarzer.wang/auth/oss/download/layesta/{level.Guid}",
            method = "GET"
        };
        web.SetRequestHeader("Authorization", $"Bearer {LimLayestaSubmissionManager.Instance.Bearer}");
        yield return web.SendWebRequest();
        if (web.responseCode == 401)
        {
            LimLayestaSubmissionManager.Instance.TokenInvalid();
            yield break;
        }
        if (!string.IsNullOrWhiteSpace(web.error))
        {
            Debug.Log(web.error);
            yield break;
        }
        string ret = web.downloadHandler.text;
        JObject response = JObject.Parse(ret);
        if (!response["Succeed"].Value<bool>())
        {
            MessageBoxManager.Instance.ShowMessage(((ErrorCode)response["ErrorCode"].Value<int>()).ToString());
            yield break;
        }
        string url = response["Uri"].Value<string>(); 
        web = new UnityWebRequest
        {
            downloadHandler = new DownloadHandlerFile(path),
            url = url,
            method = "GET"
        };
        web.SetRequestHeader("User-Agent", LimLayestaSubmissionManager.Instance.Id);
        ProgressBarManager.Instance.ShowProgress(() => web.isDone, () => web.downloadProgress);
        yield return web.SendWebRequest();
        if (!string.IsNullOrWhiteSpace(web.error))
        {
            Debug.Log(web.error);
            yield break;
        }
        WindowsDialogUtility.OpenExplorer(path);
    }
    IEnumerator DeleteCoroutine()
    {
        UnityWebRequest web = new UnityWebRequest
        {
            downloadHandler = new DownloadHandlerBuffer(),
            url = $"https://la.schwarzer.wang/auth/oss/delete/{level.Guid}",
            method = "GET"
        };
        web.SetRequestHeader("Authorization", $"Bearer {LimLayestaSubmissionManager.Instance.Bearer}");
        yield return web.SendWebRequest();
        if (web.responseCode == 401)
        {
            LimLayestaSubmissionManager.Instance.TokenInvalid();
            yield break;
        }
        if (!string.IsNullOrWhiteSpace(web.error))
        {
            Debug.Log(web.error);
            yield break;
        }
        web = new UnityWebRequest
        {
            downloadHandler = new DownloadHandlerBuffer(),
            url = $"https://la.schwarzer.wang/layestalevel/remove/{level.Guid}",
            method = "GET"
        };
        web.SetRequestHeader("Authorization", $"Bearer {LimLayestaSubmissionManager.Instance.Bearer}");
        yield return web.SendWebRequest();
        if (web.responseCode == 401)
        {
            LimLayestaSubmissionManager.Instance.TokenInvalid();
            yield break;
        }
        if (!string.IsNullOrWhiteSpace(web.error))
        {
            Debug.Log(web.error);
            yield break;
        }
        Debug.Log(web.downloadHandler.text);
        LimLayestaSubmissionManager.Instance.EmptyList();
        LimLayestaSubmissionManager.Instance.Refresh();
    }
    IEnumerator UpdateInfoCoroutine()
    {
        level.ShouldDisplay = ShouldDisplay.isOn;
        UnityWebRequest web = new UnityWebRequest
        {
            downloadHandler = new DownloadHandlerBuffer(),
            uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(level))),
            url = $"https://la.schwarzer.wang/layestalevel/update",
            method = "POST"
        };
        web.SetRequestHeader("Authorization", $"Bearer {LimLayestaSubmissionManager.Instance.Bearer}");
        web.SetRequestHeader("Content-Type", $"application/json; charset=utf-8");
        web.SetRequestHeader("Accept", $"application/json; charset=utf-8");
        yield return web.SendWebRequest();
        if (!string.IsNullOrWhiteSpace(web.error))
        {
            UpdateUploadStatus("Layesta_Submission_UploadErr2");
            yield break;
        }
        Debug.Log(web.downloadHandler.text);
    }
}
