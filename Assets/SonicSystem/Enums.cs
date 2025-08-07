using System;
using UnityEngine;

public class Enums : MonoBehaviour
{
    public enum BandType { Bypass, LowPass, BandPass, HighPass }

    public enum SonicInput { None, Volume, Bass, Mids, His, SineBeat, SineBar, Sine4Bar, SawBeat, SawBar, Saw4Bar, TriBeat, TriBar, Tri4Bar, SquareBeat, SquareBar, Square4Bar };

    public enum InputConversion { Raw, Smoothed, ThresholdActual, ThresholdPassed }

    public enum OutputMode { Actual, Additive }

    public enum BeatDurations { QuarterBeat, HalfBeat, Beat, Bar, Bar4, Bar8, Bar16, Bar32, Bar64 }


}
