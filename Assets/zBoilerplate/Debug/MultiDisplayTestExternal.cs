
using UnityEngine;
using Unity.Properties;


public sealed class MultiDisplayTestExternal : MonoBehaviour
{
    [CreateProperty]
    public string InfoText { get; private set; }

    void Start()
    {
        Display display = Display.displays[1] as Display;

        if (display != null)
        {
            InfoText += "External Display : " + display.systemWidth + "x" + display.systemHeight;
        }
        else
        {
            InfoText += "External Display is not connected";
        }

    }
}
