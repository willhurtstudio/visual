using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SonicID", menuName = "Scriptable Objects/SonicID")]
public class SonicID : ScriptableObject
{
    //StorePersistentDataHere
    public string playerName;
    public Color playerColour;
    public int lives;
    public List<Action<float>> events = new List<Action<float>>();


    // All can be raised by different class to the one they are declared in, this sits inside the player class at plater.ID.events.allEvents
    // These are hardcoded by me in response to the count of Enums in Enums.SonicEvents
    // Input Events
    public Action<float> OnNone;
    public Action<float> OnVolumeChanged;
    public Action<float> OnBassChanged;
    public Action<float> OnMidsChanged;
    public Action<float> OnHisChanged;

    public Action<float> OnLFOSineBeat;
    public Action<float> OnLFOSineBar;
    public Action<float> OnLFOSine4Bar;

    public Action<float> OnLFOSawBeat;
    public Action<float> OnLFOSawBar;
    public Action<float> OnLFOSaw4Bar;

    public Action<float> OnLFOTriBeat;
    public Action<float> OnLFOTriBar;
    public Action<float> OnLFOTri4Bar;

    public Action<float> OnLFOSquareBeat;
    public Action<float> OnLFOSquareBar;
    public Action<float> OnLFOSquare4Bar;

    public Action<float> OnMicrophoneDeviceHasChanged;

    // public List<Action<float>> allEvents;

    public void Initialise()
    {
        Debug.Log("Initialise");
        events.Add(OnNone);
        events.Add(OnVolumeChanged);
        events.Add(OnBassChanged);
        events.Add(OnMidsChanged);
        events.Add(OnHisChanged);

        events.Add(OnLFOSineBeat);
        events.Add(OnLFOSineBar);
        events.Add(OnLFOSine4Bar);

        events.Add(OnLFOTriBeat);
        events.Add(OnLFOTriBeat);
        events.Add(OnLFOTriBeat);

        events.Add(OnLFOSawBeat);
        events.Add(OnLFOSawBar);
        events.Add(OnLFOSaw4Bar);


        events.Add(OnLFOSquareBeat);
        events.Add(OnLFOSquareBeat);
        events.Add(OnLFOSquareBeat);

        Debug.Log("Initialised Events with" + events.Count + " Events ");
    }


    public float GetBeatDuration(Enums.BeatDurations durations)
    {
        float value = 1f;
        switch (durations)
        {
            case Enums.BeatDurations.QuarterBeat:
                value = 0.25f;
                break;
            case Enums.BeatDurations.HalfBeat:
                value = 0.5f;
                break;
            case Enums.BeatDurations.Beat:
                value = 1f;
                break;
            case Enums.BeatDurations.Bar:
                value = 4f;
                break;
            case Enums.BeatDurations.Bar4:
                value = 16f;
                break;
            case Enums.BeatDurations.Bar8:
                value = 32f;
                break;
            case Enums.BeatDurations.Bar16:
                value = 64f;
                break;
            case Enums.BeatDurations.Bar32:
                value = 128f;
                break;
            case Enums.BeatDurations.Bar64:
                value = 256f;
                break;
            default:
                value = 1f;
                break;
        }

        return FindFirstObjectByType<TapTempo>().GetBeatDurationMultiplied(value);
    }
    public void OnEnable()
    {
        if (events == null || events.Count < 17)
        {
            Debug.Log("INITIALISING");
            Initialise();
        }
        else
        {
            Debug.Log("NO NEED TO INIT");
        }
    }


}
