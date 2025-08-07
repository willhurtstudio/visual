using UnityEngine;
using Unity.Properties;

public sealed class MicTestExternal : MonoBehaviour
{
    [CreateProperty]
    public string InfoText { get; private set; }

    void Start()
    {
        // foreach (var device in Microphone.devices)
        // {
        //     InfoText += "Mic Device Name : " + device + "\n";
        // }

        string mic = Microphone.devices[1];

        if (mic != null)
        {
            InfoText += "External Audio Device is connected: " + mic;

        }
        else
        {
            InfoText += "External Audio Device is not connected";
        }
    }
}