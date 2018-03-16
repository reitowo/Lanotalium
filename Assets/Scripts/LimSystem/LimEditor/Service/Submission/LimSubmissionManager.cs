using System.Collections;
using System.Collections.Generic;
using System.IO;
using Schwarzer.Lanotalium.WebApi;
using Schwarzer.Lanotalium.WebApi.ChartZone;
using UnityEngine;
using UnityEngine.UI;

public class LimSubmissionManager : MonoBehaviour
{
    public Color NotUploadedColor, AcceptedColor, ModifiedColor, PendingColor, RejectedColor;
    public Text StatusText, UploadText;
    public LimEditableText ChartName, Designer, Bilibili;
    public Image PanelBackground;
    public Button RejectReasonBtn;

    private SubmitDto _SubmitDto;
    private bool _IsUploading;

    public void Initialize(SubmitDto Submission)
    {
        _SubmitDto = Submission;
        ChartName.Text = Submission.ChartDto.ChartName;
        Designer.Text = Submission.ChartDto.Designer;
        Bilibili.Text = Submission.ChartDto.BilibiliAvIndex.ToString();
        _SubmitDto.UserId = SystemInfo.deviceUniqueIdentifier;
        if (Submission.Status == SubmissionStatus.Rejected) RejectReasonBtn.gameObject.SetActive(true);
        SetColorAndStatusTexts();
    }
    public void SetColorAndStatusTexts()
    {
        if (_SubmitDto == null) return;
        switch (_SubmitDto.Status)
        {
            case SubmissionStatus.Accepted:
                PanelBackground.color = AcceptedColor;
                StatusText.text = LimLanguageManager.TextDict["Submission_Accepted"];
                break;
            case SubmissionStatus.Modified:
                PanelBackground.color = ModifiedColor;
                StatusText.text = LimLanguageManager.TextDict["Submission_Modified"];
                break;
            case SubmissionStatus.NotUploaded:
                PanelBackground.color = NotUploadedColor;
                StatusText.text = LimLanguageManager.TextDict["Submission_NotUploaded"];
                break;
            case SubmissionStatus.Pending:
                PanelBackground.color = PendingColor;
                StatusText.text = LimLanguageManager.TextDict["Submission_Pending"];
                break;
            case SubmissionStatus.Rejected:
                PanelBackground.color = RejectedColor;
                StatusText.text = LimLanguageManager.TextDict["Submission_Rejected"];
                break;
        }
        UploadText.text = LimLanguageManager.TextDict["Submission_Upload"];
        ChartName.PlaceHolder = LimLanguageManager.TextDict["Submission_ChartName"];
        Designer.PlaceHolder = LimLanguageManager.TextDict["Submission_Designer"];
        Bilibili.PlaceHolder = LimLanguageManager.TextDict["Submission_Bilibili"];
    }
    public void OnDesignerChanged()
    {
        if (_SubmitDto == null) return;
        _SubmitDto.ChartDto.Designer = Designer.Text;
        StartCoroutine(UpdateChartInfo());
    }
    public void OnChartNameChanged()
    {
        if (_SubmitDto == null) return;
        _SubmitDto.ChartDto.ChartName = ChartName.Text;
        StartCoroutine(UpdateChartInfo());
    }
    public void OnBilibiliChanged()
    {
        if (_SubmitDto == null) return;
        int BilibiliAv = 0;
        int.TryParse(Bilibili.Text, out BilibiliAv);
        _SubmitDto.ChartDto.BilibiliAvIndex = BilibiliAv;
        StartCoroutine(UpdateChartInfo());
    }
    public void UploadFile()
    {
        if (_IsUploading) return;
        if (_SubmitDto == null) return;
        MessageBoxManager.Instance.ShowMessage(LimLanguageManager.TextDict["Submission_UploadRequest"], () =>
        {
            string FilePath = WindowsDialogUtility.OpenFileDialog(LimLanguageManager.TextDict["Submission_File_Title"], "*.zip|*.zip", null);
            if (FilePath == null) return;
            StartCoroutine(UploadFileCoroutine(FilePath));
        });
    }
    IEnumerator UploadFileCoroutine(string FilePath)
    {
        if (Application.internetReachability == NetworkReachability.NotReachable) yield break;
        _IsUploading = true;
        yield return UpdateChartInfo();
        byte[] Bytes = File.ReadAllBytes(FilePath);
        WWWForm Form = new WWWForm();
        Form.AddBinaryData("File", Bytes);
        Form.AddField("UserId", SystemInfo.deviceUniqueIdentifier);
        Form.AddField("SubmissionId", _SubmitDto.SubmissionId);
        WWW Post = new WWW("http://api.lanotalium.cn/chartzone/submit/file", Form);
        ProgressBarManager.Instance.ShowProgress(() => Post.isDone, () => Post.uploadProgress);
        yield return Post;
        //Debug.Log(Post.text);
        _IsUploading = false;
        yield return new WaitForSeconds(0.1f);
        yield return LimSubmitManager.Instance.GetUserSubmissionsCoroutine();
    }
    IEnumerator UpdateChartInfo()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable) yield break;
        ObjectWrap<string> Resp = new ObjectWrap<string>();
        yield return WebApiHelper.PostObjectCoroutine("chartzone/submit/edit", _SubmitDto, Resp);
        //Debug.Log(Resp.Reference);
        yield return LimSubmitManager.Instance.GetUserSubmissionsCoroutine();
    }
    public void ShowReason()
    {
        if (_SubmitDto == null) return;
        MessageBoxManager.Instance.ShowMessage(_SubmitDto.RejectReason);
    }
}
