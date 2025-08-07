using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
public abstract class SonicSystem : MonoBehaviour
{
    protected Player player;
    //? I think I cna keep the subscription stuff all this abse calss, will check #
    public Enums.SonicInput sonicInputType = Enums.SonicInput.None;
    [HideInInspector]
    public int enumIndex = 0;
    public Enums.InputConversion conversionType;

    [Range(0f, 1f)]
    public float value;
    public Enums.OutputMode outputMode;


    protected virtual void Awake()
    {
        // ??? could this be finding the preset 
        player = transform.root.GetComponent<Player>();
        if (player == null)
        {
            Debug.Log("could not gather player");
        }
        else
        {
            Debug.Log(player.name + " gathered by " + this.name.ToString());
        }
        //player.ID.events.Initialise();

    }

    public Player GetPlayer()
    {
        if (player == null)
        {
            player = transform.root.GetComponent<Player>();
            string debugText = player == null ? "Could not Access Player in Root" : player.name + "Saved";
            return player;
        }
        else
        {
            return player;
        }
    }


    [Range(0, 1)]
    public float smoothedValue;
    [Range(0, 1)]
    public float threshold = 0.9f;
    [Range(0, 1)]
    public float smoothedFallSpeed = 0.3f;

    float thresholdPassed;
    float thresholdActual;
    float fall;
    float rawValue_old;



    public float ConvertAndSetValue(float _rawValue)
    {
        fall += Mathf.Pow(10, 1 + smoothedFallSpeed * 2) * Time.deltaTime;
        smoothedValue -= fall * Time.deltaTime;

        if (smoothedValue < _rawValue)
        {
            smoothedValue = _rawValue;
            fall = 0;
        }
        smoothedValue = Mathf.Clamp01(smoothedValue);


        if (_rawValue > threshold)
        {
            thresholdActual = _rawValue;
            thresholdPassed = 1;
        }
        else
        {
            thresholdActual = 0;
            thresholdPassed = 0;
        }
        rawValue_old = _rawValue;

        switch (conversionType)
        {
            case Enums.InputConversion.Raw:
                value = _rawValue;
                break;
            case Enums.InputConversion.Smoothed:
                value = smoothedValue;
                break;
            case Enums.InputConversion.ThresholdPassed:
                value = thresholdPassed;
                break;
            case Enums.InputConversion.ThresholdActual:
                value = thresholdActual;
                break;
            default:
                value = 0; // never used
                break;
        }
        return value;
    }
}

