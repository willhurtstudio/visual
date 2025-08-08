using UnityEngine;
using System;

public class MusicalLFO : MonoBehaviour
{
    [Header("Timing Settings")]
    [SerializeField] private float bpm = 120f;
    [SerializeField] private bool isPlaying = true;

    [Header("LFO Configuration")]
    [SerializeField] private LFOWaveform waveform = LFOWaveform.Sine;
    [SerializeField] private SyncRate syncRate = SyncRate.Beat;

    [Header("Output")]
    [SerializeField, Range(0f, 1f)] private float currentValue = 0f;

    // Private variables
    private float startTime;
    private float currentTime;

    public enum LFOWaveform
    {
        Sine,
        Saw,
        Triangle,
        Square
    }

    public enum SyncRate
    {
        Beat,      // 1/4 note
        Bar,       // 1 bar (4 beats)
        FourBars   // 4 bars (16 beats)
    }

    // Public properties
    public float BPM
    {
        get => bpm;
        set => bpm = Mathf.Max(1f, value);
    }

    public LFOWaveform Waveform
    {
        get => waveform;
        set => waveform = value;
    }

    public SyncRate Rate
    {
        get => syncRate;
        set => syncRate = value;
    }

    public float Value => currentValue;
    public bool IsPlaying
    {
        get => isPlaying;
        set => isPlaying = value;
    }

    void Start()
    {
        ResetLFO();
    }

    void Update()
    {
        if (isPlaying)
        {
            UpdateLFO();
        }
    }

    public void ResetLFO()
    {
        startTime = Time.time;
        currentTime = 0f;
    }

    public void Play()
    {
        if (!isPlaying)
        {
            startTime = Time.time - currentTime;
            isPlaying = true;
        }
    }

    public void Stop()
    {
        isPlaying = false;
    }

    private void UpdateLFO()
    {
        currentTime = Time.time - startTime;

        // Calculate cycle duration based on sync rate
        float cycleDuration = GetCycleDuration();

        // Calculate phase (0 to 1 over one complete cycle)
        float phase = (currentTime % cycleDuration) / cycleDuration;

        // Generate waveform value
        currentValue = GenerateWaveform(phase, waveform);
    }

    private float GetCycleDuration()
    {
        // Duration of one beat in seconds
        float beatDuration = 60f / bpm;

        return syncRate switch
        {
            SyncRate.Beat => beatDuration,
            SyncRate.Bar => beatDuration * 4f,
            SyncRate.FourBars => beatDuration * 16f,
            _ => beatDuration
        };
    }

    private float GenerateWaveform(float phase, LFOWaveform type)
    {
        return type switch
        {
            LFOWaveform.Sine => GenerateSine(phase),
            LFOWaveform.Saw => GenerateSaw(phase),
            LFOWaveform.Triangle => GenerateTriangle(phase),
            LFOWaveform.Square => GenerateSquare(phase),
            _ => GenerateSine(phase)
        };
    }

    private float GenerateSine(float phase)
    {
        return (Mathf.Sin(phase * 2f * Mathf.PI) + 1f) * 0.5f;
    }

    private float GenerateSaw(float phase)
    {
        return phase;
    }

    private float GenerateTriangle(float phase)
    {
        if (phase < 0.5f)
            return phase * 2f;
        else
            return 2f - (phase * 2f);
    }

    private float GenerateSquare(float phase)
    {
        return phase < 0.5f ? 0f : 1f;
    }

    // Utility methods for external access
    public float GetSineValue() => GenerateWaveform(GetCurrentPhase(), LFOWaveform.Sine);
    public float GetSawValue() => GenerateWaveform(GetCurrentPhase(), LFOWaveform.Saw);
    public float GetTriangleValue() => GenerateWaveform(GetCurrentPhase(), LFOWaveform.Triangle);
    public float GetSquareValue() => GenerateWaveform(GetCurrentPhase(), LFOWaveform.Square);

    private float GetCurrentPhase()
    {
        if (!isPlaying) return 0f;

        float cycleDuration = GetCycleDuration();
        return (currentTime % cycleDuration) / cycleDuration;
    }

    // Get beat-synced phase information
    public float GetBeatPhase()
    {
        if (!isPlaying) return 0f;

        float beatDuration = 60f / bpm;
        return (currentTime % beatDuration) / beatDuration;
    }

    public int GetCurrentBeat()
    {
        if (!isPlaying) return 0;

        float beatDuration = 60f / bpm;
        return Mathf.FloorToInt(currentTime / beatDuration) % 4 + 1;
    }

    public int GetCurrentBar()
    {
        if (!isPlaying) return 0;

        float barDuration = (60f / bpm) * 4f;
        return Mathf.FloorToInt(currentTime / barDuration) + 1;
    }
}
