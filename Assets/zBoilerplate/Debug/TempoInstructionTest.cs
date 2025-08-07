using UnityEngine;
using Unity.Properties;

public class TempoInstructionTest : MonoBehaviour
{

    [CreateProperty]
    public string InfoText { get; private set; }

    TapTempo tempoTapDetector;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        tempoTapDetector = FindFirstObjectByType<TapTempo>();
    }

    // Update is called once per frame
    void Update()
    {
        InfoText = tempoTapDetector.GetInstructions();

    }
}
