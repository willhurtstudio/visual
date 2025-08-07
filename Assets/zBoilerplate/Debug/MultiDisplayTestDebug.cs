
using UnityEngine;
using Unity.Properties;


public sealed class MultiDisplayTestDebug : MonoBehaviour
{
    [CreateProperty]
    public string InfoText { get; private set; }

    void Start()
    {
        int displayCount = Display.displays.Length;
        //Display display = Display.displays[0] as Display;

        if (displayCount > 0)
        {
            InfoText += displayCount + " displays are connected" + "\n";

            for (int i = 0; i < displayCount; i++)
            {
                Display display = Display.displays[i];
                InfoText += "Display" + i + ": " + display.systemWidth + "x" + display.systemHeight + "\n";
            }


        }
        else
        {
            InfoText += "NoDisplaysConneted";
        }

    }
}
