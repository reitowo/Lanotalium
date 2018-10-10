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
    private static bool hasShown = false;
    public RectTransform Root;
    public static LimEventManager Instance { get; private set; }

    private void Start()
    {
        Instance = this;
        if (hasShown) return;
        Root.gameObject.SetActive(true);
    }
    public void Close()
    {
        Root.gameObject.SetActive(false);
        hasShown = true;
    }
}
