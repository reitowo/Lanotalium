﻿using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FMEp1Controller : MonoBehaviour
{ 
    public void OpenUrl(string url)
    {
        Application.OpenURL(url);
    }
}
