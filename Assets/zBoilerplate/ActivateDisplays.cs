using UnityEngine;

public class ActivateDisplays : MonoBehaviour
{
    public Camera extCam;
    public Camera cam;

    void Awake()
    {
        Debug.Log("Connected " + Display.displays.Length);

        int displayCount = Display.displays.Length;

        // Activate all connected displays
        for (int i = 1; i < displayCount; i++)
        {
            Display.displays[i].Activate();
        }
    }

    void Update()
    {
        if (!extCam.isActiveAndEnabled)
        {
            extCam.enabled = Display.displays.Length > 1;
        }
    }
}