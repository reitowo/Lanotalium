using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Diagnostics;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class LimEventManager : MonoBehaviour
{
    public RectTransform EventRoot;
    public Font DengXian;
    private static LimEventManager instance;
    public static LimEventManager Instance
    {
        get
        {
            return instance;
        }
    }

    private IEnumerator Start()
    {
        instance = this;
        yield return FetchEventAssetBundle();
    }

    IEnumerator FetchEventAssetBundle()
    {
        WWW www = new WWW($"https://gitee.com/Schwarzer/LanotaliumStorage/raw/master/Events/{(Environment.Is64BitProcess ? "64" : "32")}.event");
        yield return www;
        if (www.error != null) yield break;
        AssetBundleCreateRequest request = AssetBundle.LoadFromMemoryAsync(www.bytes);
        yield return request;
        if (request.assetBundle == null) yield break;
        AssetBundle ab = request.assetBundle;
        AssetBundleRequest request2 = ab.LoadAssetAsync<GameObject>("EventRoot");
        yield return request2;
        if (request2.asset == null) yield break;
        GameObject e = request2.asset as GameObject;
        GameObject latest = Instantiate(e, EventRoot);
        foreach (Text t in latest.GetComponentsInChildren<Text>(true))
        {
            t.font = DengXian;
        }
        EventRoot.gameObject.SetActive(true);
    }

#if UNITY_EDITOR
    [MenuItem("Lanotalium/Build Events")]
    public static void BuildEvents()
    {
        BuildPipeline.BuildAssetBundles("Events/32", BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
        BuildPipeline.BuildAssetBundles("Events/64", BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);
        File.Copy(@"E:\Lanotalium\Lanotalium\Events\32\event\latest", @"H:\LanotaliumStorage\Events\32.event", true);
        File.Copy(@"E:\Lanotalium\Lanotalium\Events\64\event\latest", @"H:\LanotaliumStorage\Events\64.event", true);
    }
#endif
}
