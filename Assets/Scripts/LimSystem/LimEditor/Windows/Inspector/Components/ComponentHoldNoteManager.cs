using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ComponentHoldNoteManager : MonoBehaviour
{
    public RectTransform ViewRect, ComponentRect;
    public int UnFoldHeight;
    public InputField Duration, JCount;
    public Image DurationImg, JCountImg;
    public Text LabelText, DurationText, JCountText, JointsText, AbsoluteText, ChainedText, JointDegreeText, JointTimeText, JointEaseText;
    public Color InvalidColor, ValidColor;
    public LimInspectorManager InspectorManager;
    public LimOperationManager OperationManager;
    public GameObject ComponentJoint;
    public bool isAbsolute = false, isChained = false, EnableValueChange = true;
    public Transform ComponentJointTransform;
    public Toggle Absolute, Chain;

    private bool isFolded = false;
    private Lanotalium.Editor.ComponentHoldNoteMode Mode = Lanotalium.Editor.ComponentHoldNoteMode.Idle;
    private List<ComponentJointManager> JointManagers = new List<ComponentJointManager>();
    private float UiWidth;

    private void Start()
    {
        ComponentRect.sizeDelta = new Vector2(0, ViewRect.sizeDelta.y - ViewRect.anchoredPosition.y);
        RefreshUiWidth();
    }
    private void Update()
    {
        OnUiWidthChange();
    }
    public void RefreshUiWidth()
    {
        UiWidth = ViewRect.rect.width;
        float Ratio = UiWidth / 500f;
        JointDegreeText.GetComponent<RectTransform>().sizeDelta = new Vector2(160 * Ratio, 30);
        JointDegreeText.GetComponent<RectTransform>().anchoredPosition = new Vector2(10 * Ratio, 0);
        JointTimeText.GetComponent<RectTransform>().sizeDelta = new Vector2(160 * Ratio, 30);
        JointTimeText.GetComponent<RectTransform>().anchoredPosition = new Vector2(180 * Ratio, 0);
        JointEaseText.GetComponent<RectTransform>().sizeDelta = new Vector2(60 * Ratio, 30);
        JointEaseText.GetComponent<RectTransform>().anchoredPosition = new Vector2(350 * Ratio, 0);
        Duration.GetComponent<RectTransform>().sizeDelta = new Vector2(200 * Ratio, 30);
        JCount.GetComponent<RectTransform>().sizeDelta = new Vector2(200 * Ratio, 30);
    }
    public void OnUiWidthChange()
    {
        if (UiWidth != ViewRect.rect.width)
        {
            RefreshUiWidth();
        }
    }
    public void SetTexts()
    {
        LabelText.text = LimLanguageManager.TextDict["Component_HoldNote_Label"];
        DurationText.text = LimLanguageManager.TextDict["Component_HoldNote_Duration"];
        JCountText.text = LimLanguageManager.TextDict["Component_HoldNote_JCount"];
        JointsText.text = LimLanguageManager.TextDict["Component_HoldNote_Joints"];
        AbsoluteText.text = LimLanguageManager.TextDict["Component_HoldNote_Joints_Mode_Absolute"];
        ChainedText.text = LimLanguageManager.TextDict["Component_HoldNote_Joints_Mode_Chained"];
        JointDegreeText.text = LimLanguageManager.TextDict["Component_HoldNote_Joints_Degree"];
        JointTimeText.text = LimLanguageManager.TextDict["Component_HoldNote_Joints_Time"];
        JointEaseText.text = LimLanguageManager.TextDict["Component_HoldNote_Joints_Ease"];
    }
    public void Fold()
    {
        if (isFolded)
        {
            ViewRect.sizeDelta = new Vector2(0, UnFoldHeight); isFolded = false;
        }
        else if (!isFolded)
        {
            ViewRect.sizeDelta = new Vector2(0, 0); isFolded = true;
        }
        ComponentRect.sizeDelta = new Vector2(0, ViewRect.sizeDelta.y - ViewRect.anchoredPosition.y);
        InspectorManager.ArrangeComponentsUi();
    }
    public void OnSelectChange()
    {
        EnableValueChange = false;
        if (OperationManager.SelectedTapNote.Count != 0 && OperationManager.SelectedHoldNote.Count == 0)
        {
            Mode = Lanotalium.Editor.ComponentHoldNoteMode.Idle;
            gameObject.SetActive(false);
        }
        else if (OperationManager.SelectedTapNote.Count == 0 && OperationManager.SelectedHoldNote.Count == 1)
        {
            gameObject.SetActive(true);
            Mode = Lanotalium.Editor.ComponentHoldNoteMode.Work;
            Duration.text = OperationManager.SelectedHoldNote[0].Duration.ToString();
            JCount.text = OperationManager.SelectedHoldNote[0].Jcount.ToString();
            RefreshJointList();
        }
        else if (OperationManager.SelectedTapNote.Count == 0 && OperationManager.SelectedHoldNote.Count > 1)
        {
            Mode = Lanotalium.Editor.ComponentHoldNoteMode.NotSupport;
            gameObject.SetActive(false);
        }
        else if (OperationManager.SelectedTapNote.Count == 0 && OperationManager.SelectedHoldNote.Count == 0)
        {
            Mode = Lanotalium.Editor.ComponentHoldNoteMode.Idle;
            gameObject.SetActive(false);
        }
        EnableValueChange = true;
    }
    public void RefreshJointValues(ComponentJointManager Except = null)
    {
        EnableValueChange = false;
        foreach (ComponentJointManager Joint in JointManagers)
        {
            if (Joint != Except) Joint.RefreshJointValue();
        }
        EnableValueChange = true;
    }
    public void RefreshJointList()
    {
        EnableValueChange = false;
        foreach (ComponentJointManager Joint in JointManagers) Destroy(Joint.gameObject);
        JointManagers.Clear();
        if (Mode == Lanotalium.Editor.ComponentHoldNoteMode.Work)
        {
            float Height = 0;
            if (OperationManager.SelectedHoldNote[0].Joints != null)
            {
                foreach (Lanotalium.Chart.LanotaJoints Joint in OperationManager.SelectedHoldNote[0].Joints)
                {
                    GameObject Tmp = Instantiate(ComponentJoint, ComponentJointTransform);
                    ComponentJointManager JointManagerTmp = Tmp.GetComponent<ComponentJointManager>();
                    JointManagerTmp.Initialize(OperationManager, OperationManager.SelectedHoldNote[0], Joint, this);
                    JointManagerTmp.RefreshJointValue();
                    Tmp.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, Height);
                    Height -= 30;
                    JointManagers.Add(JointManagerTmp);
                }
            }
            ViewRect.sizeDelta = new Vector2(0, 130 - Height);
            UnFoldHeight = (int)ViewRect.sizeDelta.y;
            ComponentRect.sizeDelta = new Vector2(0, ViewRect.sizeDelta.y - ViewRect.anchoredPosition.y);
            isFolded = false;
        }
        InspectorManager.ArrangeComponentsUi();
        EnableValueChange = true;
    }
    public void OnDurationChange()
    {
        float DurationTmp;
        if (!float.TryParse(Duration.text, out DurationTmp))
        {
            DurationImg.color = InvalidColor;
            return;
        }
        if (DurationTmp < 0.0001f)
        {
            DurationImg.color = InvalidColor;
            return;
        }
        DurationImg.color = ValidColor;
        OperationManager.SetHoldNoteDuration(OperationManager.SelectedHoldNote[0], DurationTmp);
    }
    public void OnJCountChange()
    {
        int JCountTmp;
        if (!int.TryParse(JCount.text, out JCountTmp))
        {
            JCountImg.color = InvalidColor;
            return;
        }
        JCountImg.color = ValidColor;
        OperationManager.SetHoldNoteJCount(OperationManager.SelectedHoldNote[0], JCountTmp);
    }
    public void OnAbsoluteChange()
    {
        isAbsolute = Absolute.isOn;
        RefreshJointValues();
    }
    public void OnChainChange()
    {
        isChained = Chain.isOn;
    }

}
