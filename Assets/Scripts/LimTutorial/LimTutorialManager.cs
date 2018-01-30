using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Newtonsoft.Json;
using System.IO;
using System;

public class LimTutorialManager : MonoBehaviour
{
    public List<LimTutorialAsset> TutorialAssets;
    public GameObject FullScreenMask;
    public Text ContentText;

    public static Dictionary<string, RectTransform> UIRectTransformDict = new Dictionary<string, RectTransform>();
    private static bool IsShowing = false;
    private static Text ContentTextStatic;
    private static GameObject FullScreenMaskStatic;
    private static LimTutorialAsset[] TutorialList;
    private static UnityEvent UnHighlightEvent = new UnityEvent();
    private static List<string> ShownTutorial = new List<string>();
    private static string ShownTutorialSavePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/Lanotalium/Tutorial.json";
    private static float ShownTime = 0;
    private static LimTutorialAsset ShowingTutorial;

    private void Start()
    {
        ContentTextStatic = ContentText;
        FullScreenMaskStatic = FullScreenMask;
        TutorialList = TutorialAssets.ToArray();
        if (Application.platform != RuntimePlatform.WindowsEditor)
            if (File.Exists(ShownTutorialSavePath))
                ShownTutorial = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(ShownTutorialSavePath));
    }
    private void Update()
    {
        if (IsShowing)
        {
            if (Input.GetMouseButtonUp(0))
            {
                HideTutorial();
            }
        }
    }
    private void OnDestroy()
    {
        File.WriteAllText(ShownTutorialSavePath, JsonConvert.SerializeObject(ShownTutorial));
    }

    private static RectTransform FindRectTransformById(string UIRectTransformId)
    {
        RectTransform rectTransform = null;
        if (UIRectTransformDict.TryGetValue(UIRectTransformId, out rectTransform)) return rectTransform;
        else return null;
    }
    private static LimTutorialAsset FindTutorialById(string TutorialId)
    {
        foreach (LimTutorialAsset t in TutorialList)
        {
            if (t.Id == TutorialId) return t;
        }
        return null;
    }
    public static void ShowTutorial(string TutorialId)
    {
        try
        {
            if (ShownTutorial.Contains(TutorialId)) return;
            ShowTutorial(FindTutorialById(TutorialId).RectTransformId.ToArray(), TutorialId);
        }
        catch (Exception)
        {

        }
    }
    public static void ShowTutorial(string UIRectTransformId, string TutorialId)
    {
        if (ShownTutorial.Contains(TutorialId)) return;
        ShowTutorial(new string[] { UIRectTransformId }, TutorialId);
    }
    public static void ShowTutorial(string[] UIRectTransformId, string TutorialId)
    {
        if (ShownTutorial.Contains(TutorialId)) return;
        List<RectTransform> rectTransforms = new List<RectTransform>();
        foreach (string Id in UIRectTransformId)
        {
            RectTransform rectTransform = FindRectTransformById(Id);
            if (rectTransform != null) rectTransforms.Add(rectTransform);
        }
        ShowTutorial(rectTransforms.ToArray(), TutorialId);
    }
    public static void ShowTutorial(RectTransform UIRectTransform, string TutorialId)
    {
        if (ShownTutorial.Contains(TutorialId)) return;
        ShowTutorial(new RectTransform[] { UIRectTransform }, TutorialId);
    }
    public static void ShowTutorial(RectTransform[] UIRectTransform, string TutorialId)
    {
        if (TutorialList == null) return;
        LimTutorialAsset tutorial = FindTutorialById(TutorialId);
        if (tutorial == null) return;
        if (ShownTutorial.Contains(TutorialId)) return;

        #region Highlighting
        UnHighlightEvent.RemoveAllListeners();
        foreach (RectTransform rectTransform in UIRectTransform)
        {
            Canvas canvas = rectTransform.GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = rectTransform.gameObject.AddComponent<Canvas>();

                canvas.overrideSorting = true;
                canvas.sortingLayerID = SortingLayer.NameToID("Tutorial");
                canvas.sortingOrder = 10000;

                UnHighlightEvent.AddListener(() =>
                {
                    Destroy(canvas);
                });
            }
            else
            {
                int sortingLayerId = canvas.sortingLayerID;
                int sortingOrder = canvas.sortingOrder;
                bool overrideSorting = canvas.overrideSorting;

                canvas.overrideSorting = true;
                canvas.sortingLayerID = SortingLayer.NameToID("Tutorial");
                canvas.sortingOrder = 10000;

                UnHighlightEvent.AddListener(() =>
                {
                    canvas.overrideSorting = overrideSorting;
                    canvas.sortingLayerID = sortingLayerId;
                    canvas.sortingOrder = sortingOrder;
                });
            }
        }
        #endregion

        ShownTutorial.Add(TutorialId);
        ContentTextStatic.text = LimLanguageManager.TutorialDict[tutorial.Id];
        FullScreenMaskStatic.SetActive(true);
        ShownTime = Time.time;
        ShowingTutorial = tutorial;
        IsShowing = true;
    }
    public static void HideTutorial()
    {
        if (Time.time - ShownTime < 0.8f) return;
        UnHighlightEvent.Invoke();
        UnHighlightEvent.RemoveAllListeners();
        FullScreenMaskStatic.SetActive(false);
        IsShowing = false;
        File.WriteAllText(ShownTutorialSavePath, JsonConvert.SerializeObject(ShownTutorial));
        if (!string.IsNullOrEmpty(ShowingTutorial.NextId)) ShowTutorial(ShowingTutorial.NextId);
    }
}