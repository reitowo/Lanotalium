using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimDisplayManager : MonoBehaviour
{
    public RenderTexture TunerRenderTexture;
    public Camera TunerCamera, MainCamera;
    public List<GameObject> GameObjectsToSetActiveFalse;
    public LimTunerHeadManager TunerHeadManager;
    public GameObject ReplayKitController;
    public Dictionary<int, bool> RecoverActives = new Dictionary<int, bool>();

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
                foreach (GameObject G in GameObjectsToSetActiveFalse)
                {
                    if (!RecoverActives.ContainsKey(G.GetInstanceID())) RecoverActives.Add(G.GetInstanceID(), false);
                    RecoverActives[G.GetInstanceID()] = G.activeSelf;
                    G.SetActive(false);
                }
                TunerHeadManager.Mode = Lanotalium.Editor.TunerHeadMode.InTuner;
                if (Application.platform == RuntimePlatform.IPhonePlayer) ReplayKitController.SetActive(true);
            }
            else
            {
                TunerCamera.targetTexture = TunerRenderTexture;
                MainCamera.gameObject.SetActive(true);
                foreach (GameObject G in GameObjectsToSetActiveFalse)
                {
                    G.SetActive(RecoverActives[G.GetInstanceID()]);
                }
                TunerHeadManager.Mode = Lanotalium.Editor.TunerHeadMode.InEditor;
                if (Application.platform == RuntimePlatform.IPhonePlayer) ReplayKitController.SetActive(false);
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
