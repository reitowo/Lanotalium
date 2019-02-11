using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LimTunerWindowManager : MonoBehaviour
{
    public LimWindowManager BaseWindow;
    public RenderTexture TunerRenderTexture;
    public Camera TunerCamera;
    public Text SkinText, FullScreenText, CameraMotionText, StretchBGAText, ChartSpeedText, DemoText;
    public GameObject SkinPanel;
    public Image RitmoImg, FisicaImg;
    public SpriteRenderer TunerBackground, TunerBorder;
    public Sprite RitmoU, RitmoP, FisicaU, FisicaP, RitmoBg, FisicaBg, RitmoBd, FisicaBd;
    public LimDisplayManager DisplayManager;
    public LimTunerHeadManager TunerHeadManager;
    public GameObject TunerSettingPanel;
    public Toggle EnableMotionToggle, StretchBGAToggle, ChartSpeedToggle;
    public LimCameraManager CameraManager;
    public LimBoxSelectionManager BoxSelectionManager;
    public LimTunerManager TunerManager;
    public LimOperationManager OperationManager;

    private void Start()
    {
        StretchBGAToggle.isOn = LimSystem.Preferences.StretchBGA;
        EnsureSkinUIWorksProperly();
    }
    public void SetTexts()
    {
        BaseWindow.WindowName = LimLanguageManager.TextDict["Window_Tuner_Label"];
        SkinText.text = LimLanguageManager.TextDict["Window_Tuner_Skin"];
        FullScreenText.text = LimLanguageManager.TextDict["Window_Tuner_FullScreen"];
        CameraMotionText.text = LimLanguageManager.TextDict["Window_Tuner_Motion"];
        StretchBGAText.text = LimLanguageManager.TextDict["Window_Tuner_StretchBGA"];
        ChartSpeedText.text = LimLanguageManager.TextDict["Window_Tuner_ChartSpeed"];
        DemoText.text = LimLanguageManager.TextDict["Window_Tuner_Demo"];
    }
    private void Update()
    {
        SyncRenderTextureWithWindowSize();
        SelectNotesInBoxArea();
    }
    private void SelectNotesInBoxArea()
    {
        if (!TunerManager.isInitialized) return;
        if (BoxSelectionManager.Size < 50) return;
        foreach (Lanotalium.Chart.LanotaTapNote Tap in TunerManager.TapNoteManager.TapNote)
        {
            if (Tap.TapNoteGameObject.activeInHierarchy)
            {
                if (BoxSelectionManager.IsNoteInBoxArea(Tap.TapNoteGameObject))
                {
                    if (!Tap.OnSelect)
                    {
                        OperationManager.SelectTapNote(Tap, true);
                    }
                }
                else if (!Input.GetKey(KeyCode.LeftControl))
                {
                    if (Tap.OnSelect)
                    {
                        OperationManager.DeSelectTapNote(Tap);
                    }
                }
            }
        }
        foreach (Lanotalium.Chart.LanotaHoldNote Hold in TunerManager.HoldNoteManager.HoldNote)
        {
            if (Hold.HoldNoteGameObject.activeInHierarchy)
            {
                if (BoxSelectionManager.IsNoteInBoxArea(Hold.HoldNoteGameObject))
                {
                    if (!Hold.OnSelect)
                    {
                        OperationManager.SelectHoldNote(Hold, true);
                    }
                }
                else if (!Input.GetKey(KeyCode.LeftControl))
                {
                    if (Hold.OnSelect)
                    {
                        OperationManager.DeSelectHoldNote(Hold);
                    }
                }
            }
        }
    }
    private void SyncRenderTextureWithWindowSize()
    {
        if (new Vector2(TunerRenderTexture.width, TunerRenderTexture.height) != BaseWindow.WindowRectTransform.sizeDelta)
        {
            TunerRenderTexture.Release();
            TunerRenderTexture.width = Mathf.RoundToInt(BaseWindow.WindowRectTransform.sizeDelta.x);
            TunerRenderTexture.height = Mathf.RoundToInt(BaseWindow.WindowRectTransform.sizeDelta.y);
            TunerRenderTexture.Create();
            TunerCamera.rect = new Rect(0, 0, 1, 1);
        }
    }
    public void AdjustTo16W9H()
    {
        BaseWindow.WindowRectTransform.sizeDelta = new Vector2(BaseWindow.WindowRectTransform.sizeDelta.x, BaseWindow.WindowRectTransform.sizeDelta.x * 0.5625f);
    }
    public void AdjustTo4W3H()
    {
        BaseWindow.WindowRectTransform.sizeDelta = new Vector2(BaseWindow.WindowRectTransform.sizeDelta.x, BaseWindow.WindowRectTransform.sizeDelta.x * 0.75f);
    }
    public void AdjustWindowRatio(float Ratio)
    {
        BaseWindow.WindowRectTransform.sizeDelta = new Vector2(BaseWindow.WindowRectTransform.sizeDelta.x, BaseWindow.WindowRectTransform.sizeDelta.x * Ratio);
    }
    public void ChangeDisplayMode()
    {
        if (DisplayManager.FullScreenTuner) DisplayManager.FullScreenTuner = false;
        else DisplayManager.FullScreenTuner = true;
    }

    public void EnsureSkinUIWorksProperly()
    {
        switch (LimSystem.Preferences.TunerSkin)
        {
            case Lanotalium.Editor.TunerSkin.Ritmo:
                UseRitmoSkin();
                break;
            case Lanotalium.Editor.TunerSkin.Fisica:
                UseFisicaSkin();
                break;
        }
    }
    public void ShowSkinPanel()
    {
        if (SkinPanel.activeInHierarchy) SkinPanel.SetActive(false);
        else
        {
            EnsureSkinUIWorksProperly();
            SkinPanel.SetActive(true);
        }
    }
    public void UseRitmoSkin()
    {
        LimSystem.Preferences.TunerSkin = Lanotalium.Editor.TunerSkin.Ritmo;
        RitmoImg.sprite = RitmoP;
        FisicaImg.sprite = FisicaU;
        TunerBackground.sprite = RitmoBg;
        TunerBorder.sprite = RitmoBd;
    }
    public void UseFisicaSkin()
    {
        LimSystem.Preferences.TunerSkin = Lanotalium.Editor.TunerSkin.Fisica;
        RitmoImg.sprite = RitmoU;
        FisicaImg.sprite = FisicaP;
        TunerBackground.sprite = FisicaBg;
        TunerBorder.sprite = FisicaBd;
    }
    public void SwitchTunerSettingPanel()
    {
        if (TunerSettingPanel.activeInHierarchy) TunerSettingPanel.SetActive(false);
        else TunerSettingPanel.SetActive(true);
    }
    public void OnEnableMotionToggled()
    {
        if (EnableMotionToggle == null) return;
        CameraManager.DisableMotion = !EnableMotionToggle.isOn;
    }
    public void OnStretchBGAToggled()
    {
        if (StretchBGAToggle == null) return;
        LimSystem.Preferences.StretchBGA = StretchBGAToggle.isOn;
    }
    public void OnChartSpeedToggled()
    {
        if (ChartSpeedToggle == null) return;
        TunerManager.ScrollManager.DisableChartSpeed = !ChartSpeedToggle.isOn;
    }
    public void OnDemoToggle(Toggle toggle)
    {
        LimNoteEase.Instance.DemoMode = toggle.isOn;
    }
}
