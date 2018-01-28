using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Tutorial_", menuName = "Lanotalium/Tutorial")]
public class LimTutorialAsset : ScriptableObject
{
    public string Id;
    public List<string> RectTransformId;
    public string NextId;
}