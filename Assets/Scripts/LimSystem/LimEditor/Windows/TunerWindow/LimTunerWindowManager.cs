using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LimTunerWindowManager : MonoBehaviour
{
    public LimWindowManager BaseWindow;
    public RenderTexture TunerRenderTexture;
    public Camera TunerCamera;
    public Text SkinText, CapturerText, FullScreenText, CameraMotionText;
    public GameObject SkinPanel;
    public Image RitmoImg, FisicaImg;
    public SpriteRenderer TunerBackground, TunerBorder;
    public Sprite RitmoU, RitmoP, FisicaU, FisicaP, RitmoBg, FisicaBg, RitmoBd, FisicaBd;
    public LimDisplayManager DisplayManager;
    public LimTunerHeadManager TunerHeadManager;
    public GameObject TunerSettingPanel;
    public Toggle EnableMotionToggle;
    public LimCameraManager CameraManager;

    void Start()
    {
        EnsureSkinUIWorksProperly();
    }
    public void SetTexts()
    {
        BaseWindow.WindowName = LimLanguageManager.TextDict["Window_Tuner_Label"];
        SkinText.text = LimLanguageManager.TextDict["Window_Tuner_Skin"];
        CapturerText.text = LimLanguageManager.TextDict["Window_Tuner_Capturer"];
        FullScreenText.text = LimLanguageManager.TextDict["Window_Tuner_FullScreen"];
        CameraMotionText.text = LimLanguageManager.TextDict["Window_Tuner_Motion"];
    }
    private void Update()
    {
        SyncRenderTextureWithWindowSize();
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
        CameraManager.DisableMotion = !EnableMotionToggle.isOn;
    }
}
