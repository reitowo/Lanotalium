using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LimLayestaDevice : MonoBehaviour
{
    public Text Name;
    public LayestaDevice Device;

    public void Init(LayestaDevice device)
    {
        Device = device;
        Name.text = device.Message;
    }
    public void Play()
    {
        LimLayestaManager.Instance.Play(Device);
    }
}
