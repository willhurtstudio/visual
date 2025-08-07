using UnityEngine;
using Unity.Properties;

public sealed class MicTest : MonoBehaviour
{
    [CreateProperty]
    public string InfoText { get; private set; }

    void Start()
    {
        string mic = Microphone.devices[0];

        if (mic != null)
        {
            InfoText += "Internal Mic is connected: " + mic;

        }
        else
        {
            InfoText += "Internal Mic is not connected";
        }
    }

    public void RecieveMicDetails(string s)
    {
        InfoText += s + "\n";
    }
}