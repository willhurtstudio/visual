using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using Unity.Properties;

public class TapTempo : MonoBehaviour
{
    [Header("UI Element Names")]
    //public string tempoLabelName = "tempo-label";
    //public string instructionLabelName = "instruction-label";

    [CreateProperty]
    public string InfoTextInstruction { get; private set; }

    [CreateProperty]
    public string InfoText { get; private set; }
    //private Label tempoLabel;
    //private Label instructionLabel;
    //private UIDocument uiDocument;

    [Header("Settings")]
    public int maxTaps = 4; // Number of taps to average
    public float resetTime = 1f; // Reset if no tap for this many seconds

    private List<float> tapTimes = new List<float>();
    private float lastTapTime;
    private float currentTempo;

    LFOInput multiLFOManager;

    void Start()
    {
        multiLFOManager = FindFirstObjectByType<LFOInput>();
        // Get UIDocument component
        // uiDocument = GetComponent<UIDocument>();
        // if (uiDocument == null)
        // {
        //     Debug.LogError("UIDocument component not found! Please attach this script to a GameObject with UIDocument.");
        //     return;
        // }

        // Get UI elements
        //var root = uiDocument.rootVisualElement;
        //tempoLabel = root.Q<Label>(tempoLabelName);
        //instructionLabel = root.Q<Label>(instructionLabelName);

        // if (tempoLabel == null)
        //     Debug.LogWarning($"Label with name '{tempoLabelName}' not found in UI Document!");

        // if (instructionLabel != null)
        //     instructionLabel.text = "Tap the screen to detect tempo";
        InfoTextInstruction = "Tap the screen to detect tempo";

        // Register for pointer events on the entire document

        UpdateTempoDisplay();
    }

    void Update()
    {
        // For mobile touch input (UIElements handles mouse automatically)
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            RegisterTap();
        }

        // Reset if too much time has passed
        if (Time.time - lastTapTime > resetTime && tapTimes.Count > 0)
        {
            ResetTaps();
        }
    }

    // void OnPointerDown(PointerDownEvent evt)
    // {
    //     RegisterTap();
    // }

    public void RegisterTap()
    {
        Debug.Log("Tap registered");
        float currentTime = Time.time;

        // Add current tap time
        tapTimes.Add(currentTime);
        lastTapTime = currentTime;

        // Remove oldest tap if we have too many
        if (tapTimes.Count > maxTaps)
        {
            tapTimes.RemoveAt(0);
        }

        // Calculate tempo if we have at least 2 taps
        if (tapTimes.Count >= 4)
        {
            CalculateTempo();
            multiLFOManager.SetBPM(currentTempo);

        }

        UpdateTempoDisplay();
    }

    void CalculateTempo()
    {
        if (tapTimes.Count < 2) return;

        // Calculate average time between taps
        float totalInterval = 0f;
        for (int i = 1; i < tapTimes.Count; i++)
        {
            totalInterval += tapTimes[i] - tapTimes[i - 1];
        }

        float avgInterval = totalInterval / (tapTimes.Count - 1);

        // Convert to BPM (beats per minute)
        currentTempo = 60f / avgInterval;

        // Clamp to reasonable range
        currentTempo = Mathf.Clamp(currentTempo, 70f, 200f);
    }

    void ResetTaps()
    {
        tapTimes.Clear();
        //currentTempo = 0f;
        UpdateTempoDisplay();

        //        if (instructionLabel != null)
        //            instructionLabel.text = "Tap the screen to detect tempo";
        InfoTextInstruction = "Tap the screen to detect tempo";
    }

    void UpdateTempoDisplay()
    {
        // if (tempoLabel == null) return;

        if (currentTempo > 0)
        {
            //tempoLabel.text = $"Tempo: {currentTempo:F0} BPM";
            InfoText = $"Tempo: {currentTempo:F0} BPM";
            //if (instructionLabel != null)
            //{
            //instructionLabel.text = $"Taps: {tapTimes.Count}/{maxTaps}";
            InfoTextInstruction = $"Taps: {tapTimes.Count}/{maxTaps}";
            //}
        }
        else
        {
            //tempoLabel.text = "Tempo: -- BPM";
            InfoText = "Tempo: -- BPM";
        }
    }

    // Public method to get current tempo
    public float GetTempo()
    {
        return currentTempo;
    }

    public float GetBeatDurationMultiplied(float multipler)
    {
        float value = 1f / currentTempo * multipler;
        return value;
    }

    public string GetInstructions()
    {
        return InfoTextInstruction;
    }

    // Public method to reset manually
    public void Reset()
    {
        ResetTaps();
    }
}