using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimDisplayManager : MonoBehaviour
{
    public RenderTexture TunerRenderTexture;
    public Camera TunerCamera, MainCamera;
    public List<GameObject> GameObjectsToSetActiveFalse;
    public LimTunerHeadManager TunerHeadManager;

    public bool FullScreenTuner
    {
        get
        {
            if (TunerCamera.targetTexture == null) return true;
            else return false;
        }
        set
        {
            if (value)
            {
                TunerCamera.targetTexture = null;
                MainCamera.gameObject.SetActive(false);
                foreach (GameObject G in GameObjectsToSetActiveFalse) G.SetActive(false);
                TunerHeadManager.Mode = Lanotalium.Editor.TunerHeadMode.InTuner;
            }
            else
            {
                TunerCamera.targetTexture = TunerRenderTexture;
                MainCamera.gameObject.SetActive(true);
                foreach (GameObject G in GameObjectsToSetActiveFalse) G.SetActive(true);
                TunerHeadManager.Mode = Lanotalium.Editor.TunerHeadMode.InEditor;
            }
        }
    }
    private void Update()
    {
        if (FullScreenTuner)
        {
            if (Input.GetKeyDown(KeyCode.Escape)) FullScreenTuner = false;
        }
    }
}
