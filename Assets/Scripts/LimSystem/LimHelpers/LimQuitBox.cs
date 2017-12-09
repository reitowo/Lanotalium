using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

public class LimQuitBox : MonoBehaviour {

    public GameObject QuitBoxPanel;
    private bool AllowQuitting = false;

    public void ConfirmQuitApplication()
    {
        AllowQuitting = true;
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
