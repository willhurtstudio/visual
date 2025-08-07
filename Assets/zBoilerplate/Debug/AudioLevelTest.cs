using UnityEngine;
using Unity.Properties;
using System.Collections.Generic;

public class AudioLevelTest : MonoBehaviour
{

    [CreateProperty]
    public string InfoText { get; private set; }

    public List<string> strings = new List<string>() { "", "|", "||", "|||", "||||", "|||||", "||||||", "|||||||", "||||||||", "|||||||||", "|||||||||||||" };
    TapTempo tempoTapDetector;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        tempoTapDetector = FindFirstObjectByType<TapTempo>();
    }

    // Update is called once per frame
    void Update()
    {
        InfoText = tempoTapDetector.GetTempo().ToString();
    }

    public void SetLevels(float levelV, float levelB, float levelM, float levelH)
    {
        int levelIntV = Mathf.RoundToInt(levelV * 10f);
        int levelIntB = Mathf.RoundToInt(levelB * 10f);
        int levelIntM = Mathf.RoundToInt(levelM * 10f);
        int levelIntH = Mathf.RoundToInt(levelH * 10f);
        InfoText = strings[levelIntV] + "\n" + strings[levelIntB] + "\n" + strings[levelIntM] + "\n" + strings[levelIntH];
    }
}
