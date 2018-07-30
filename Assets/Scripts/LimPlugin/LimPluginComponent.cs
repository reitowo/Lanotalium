using System.Collections;
using System.Collections.Generic;
using Lanotalium.Plugin;
using UnityEngine;
using UnityEngine.UI;

public class LimPluginComponent : MonoBehaviour
{
    public Text Name;
    public Button ShowDescription, Execute;
    public ILanotaliumPlugin Interface;
}
