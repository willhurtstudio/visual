using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEditor;

public class LFOInput : SonicSystem
{
    [Header("Global Settings")]
    [SerializeField] private float bpm = 120f;
    [SerializeField] private bool isPlaying = true;

    [Range(1, 8)]
    public int bpmDivider; // slow it all down


    [Header("LFO Channels")]
    public List<LFOChannel> lfoChannels = new List<LFOChannel>();
    // public SonicEventsManager sonicEventsManager;
    // SonicEventsObject sonicEventsObject;


    private float startTime;
    private float currentTime;
    public void SetBPM(float _bpm)
    {
        bpm = _bpm / bpmDivider;
        ResetLFOs();
    }
    public float BPM
    {
        get => bpm;
        set => bpm = Mathf.Max(1f, value);
    }

    public bool IsPlaying
    {
        get => isPlaying;
        set => isPlaying = value;
    }



    public void OnValidate()
    {
        InitializeChannels();
    }

    public void OnEnable()
    {
        // sonicEventsManager = FindFirstObjectByType<SonicEventsManager>();
        // sonicEventsObject = sonicEventsManager.GetCurrentSonicEventsObject();
        ResetLFOs();
    }

    void Update()
    {
        if (isPlaying)
        {
            UpdateAllLFOs();
        }
    }

    private void InitializeChannels()
    {
        lfoChannels.Clear();
        // Build the LFOs based on the MegaE enum
        //Enums.SonicInput[] sonicInputEnumArray = Enums.SonicInput.values();
        Enums.SonicInput[] sonicInputEnumArray = (Enums.SonicInput[])System.Enum.GetValues(typeof(Enums.SonicInput));

        foreach (Enums.SonicInput e in sonicInputEnumArray)
        {
            lfoChannels.Add(new LFOChannel(e));
        }

        // MegaE enum has Volume, Bass etc, these are tested against insiode the LFOChannel consstructor as marked as enabled = false
        // This loop then removes those items from the list, so we are left with just LFOs
        // for (int i = lfoChannels.Count - 1; i >= 0; i--)
        // {
        //     LFOChannel lfoC = lfoChannels[i];
        //     if (lfoC.enabled == false)
        //     {
        //         lfoChannels.RemoveAt(i);
        //     }
        // }
    }



    public void ResetLFOs()
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

    private void UpdateAllLFOs()
    {
        currentTime = Time.time - startTime;

        for (int i = 5; i < lfoChannels.Count; i++) // ******* 5 is here as the ENUM has 5 (0,1,2,3,4) entries which are NOT LFOS :)
        {
            if (lfoChannels[i].enabled)
            {
                UpdateLFOChannel(lfoChannels[i]);
                //sonicEventsObject.UpdateSonicEvent(lfoChannels[i].type, lfoChannels[i].currentValue);
                player.ID.events[i]?.Invoke(lfoChannels[i].currentValue);
            }
        }
    }

    private void UpdateLFOChannel(LFOChannel channel)
    {
        float cycleDuration = GetCycleDuration(channel.syncRate);
        float phase = (currentTime % cycleDuration) / cycleDuration;
        channel.currentValue = GenerateWaveform(phase, channel.waveform);
    }

    private float GetCycleDuration(MusicalLFO.SyncRate syncRate)
    {
        float beatDuration = 60f / bpm;

        return syncRate switch
        {
            MusicalLFO.SyncRate.Beat => beatDuration,
            MusicalLFO.SyncRate.Bar => beatDuration * 4f,
            MusicalLFO.SyncRate.FourBars => beatDuration * 16f,
            _ => beatDuration
        };
    }

    private float GenerateWaveform(float phase, MusicalLFO.LFOWaveform type)
    {
        return type switch
        {
            MusicalLFO.LFOWaveform.Sine => (Mathf.Sin(phase * 2f * Mathf.PI) + 1f) * 0.5f,
            MusicalLFO.LFOWaveform.Saw => phase,
            MusicalLFO.LFOWaveform.Triangle => phase < 0.5f ? phase * 2f : 2f - (phase * 2f),
            MusicalLFO.LFOWaveform.Square => phase < 0.5f ? 0f : 1f,
            _ => (Mathf.Sin(phase * 2f * Mathf.PI) + 1f) * 0.5f
        };
    }

    // Utility methods to get specific channel values
    public float GetChannelValue(int channelIndex)
    {
        if (channelIndex >= 0 && channelIndex < lfoChannels.Count)
            return lfoChannels[channelIndex].currentValue;
        return 0f;
    }

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


// Multi-LFO Manager for handling multiple LFOs
[System.Serializable]
public class LFOChannel
{
    public string name;
    [HideInInspector]
    public MusicalLFO.LFOWaveform waveform = MusicalLFO.LFOWaveform.Sine;
    //[HideInInspector]
    public MusicalLFO.SyncRate syncRate = MusicalLFO.SyncRate.Beat;
    public Enums.SonicInput type;
    [HideInInspector]
    public bool enabled = true;
    [Range(0, 1)]
    public float currentValue;

    // public int enumIntHack;
    public LFOChannel(Enums.SonicInput _type)
    {
        this.type = _type;
        this.name = _type.ToString();

        switch (_type)
        {
            case Enums.SonicInput.SineBeat:
                this.waveform = MusicalLFO.LFOWaveform.Sine;
                this.syncRate = MusicalLFO.SyncRate.Beat;
                //this.enumIntHack = 5;
                break;

            case Enums.SonicInput.SineBar:
                this.waveform = MusicalLFO.LFOWaveform.Sine;
                this.syncRate = MusicalLFO.SyncRate.Bar;
                //this.enumIntHack = 6;

                break;
            case Enums.SonicInput.Sine4Bar:
                this.waveform = MusicalLFO.LFOWaveform.Sine;
                this.syncRate = MusicalLFO.SyncRate.FourBars;
                // this.enumIntHack = 7;

                break;
            case Enums.SonicInput.TriBeat:
                this.waveform = MusicalLFO.LFOWaveform.Triangle;
                this.syncRate = MusicalLFO.SyncRate.Beat;
                //  this.enumIntHack = 8;

                break;
            case Enums.SonicInput.TriBar:
                this.waveform = MusicalLFO.LFOWaveform.Triangle;
                this.syncRate = MusicalLFO.SyncRate.Bar;
                // this.enumIntHack = 9;
                break;
            case Enums.SonicInput.Tri4Bar:
                this.waveform = MusicalLFO.LFOWaveform.Triangle;
                this.syncRate = MusicalLFO.SyncRate.FourBars;
                // this.enumIntHack = 10;
                break;
            case Enums.SonicInput.SawBeat:
                this.waveform = MusicalLFO.LFOWaveform.Saw;
                this.syncRate = MusicalLFO.SyncRate.Beat;
                // this.enumIntHack = 11;
                break;
            case Enums.SonicInput.SawBar:
                this.waveform = MusicalLFO.LFOWaveform.Saw;
                this.syncRate = MusicalLFO.SyncRate.Bar;
                // this.enumIntHack = 12;
                break;
            case Enums.SonicInput.Saw4Bar:
                this.waveform = MusicalLFO.LFOWaveform.Saw;
                this.syncRate = MusicalLFO.SyncRate.FourBars;
                // this.enumIntHack = 13;
                break;
            case Enums.SonicInput.SquareBeat:
                this.waveform = MusicalLFO.LFOWaveform.Square;
                this.syncRate = MusicalLFO.SyncRate.Beat;
                // this.enumIntHack = 14;
                break;
            case Enums.SonicInput.SquareBar:
                this.waveform = MusicalLFO.LFOWaveform.Square;
                this.syncRate = MusicalLFO.SyncRate.Bar;
                // this.enumIntHack = 15;
                break;
            case Enums.SonicInput.Square4Bar:
                this.waveform = MusicalLFO.LFOWaveform.Square;
                this.syncRate = MusicalLFO.SyncRate.FourBars;
                // this.enumIntHack = 16;
                break;
            default:
                this.enabled = false;
                break;

        }
    }
}
