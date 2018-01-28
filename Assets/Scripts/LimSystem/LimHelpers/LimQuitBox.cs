using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LimQuitBox : MonoBehaviour {

    public GameObject QuitBoxPanel;
    public static UnityEvent OnQuitBoxConfirmed = new UnityEvent();
    private bool AllowQuitting = false;

    private void Awake()
    {
        OnQuitBoxConfirmed.RemoveAllListeners();
    }
    public void ConfirmQuitApplication()
    {
        AllowQuitting = true;
        if (OnQuitBoxConfirmed != null) OnQuitBoxConfirmed.Invoke();
        Process.GetCurrentProcess().Kill();
    }
    public void CancelQuitApplication()
    {
        AllowQuitting = false;
        QuitBoxPanel.SetActive(false);
    }
    private void OnApplicationQuit()
    {
        if(!AllowQuitting)
        {
            Application.CancelQuit();
            QuitBoxPanel.SetActive(true);
        }
    }
}
