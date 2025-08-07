using System.Collections.Generic;
using UnityEngine;

public class OutputColor : SonicSystem
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public ColorGroupsSO colorGroupSO;
    ColorGroup colorGroupZero;
    ColorGroup colorGroupOne;
    public ColorGroup colorGroupActual;
    public Camera cam;

    [Range(0, 6)]
    public int backgroundChange;

    ColorGroup SetNextColorGroup(ColorGroup current)
    {
        return colorGroupSO.GetRandomColorGroup(current);
    }


    void Start()
    {
        colorGroupActual = new ColorGroup(new List<Color>() { Color.black, Color.white, Color.red, Color.green, Color.blue });
        colorGroupZero = new ColorGroup(new List<Color>() { Color.black, Color.black, Color.black, Color.black, Color.black });
        colorGroupOne = new ColorGroup(new List<Color>() { Color.black, Color.black, Color.black, Color.black, Color.black });
    }


    bool hasFired;

    void SetValue(float _value)
    {
        value = ConvertAndSetValue(_value); // calls convert in base class to ensure smoothing / thresholding are adhered to

        if (value < 0.01f || value > 0.99f)
        {
            value = Mathf.Round(value);

            if (value < 0.01f && !hasFired)
            {
                colorGroupOne.colors = (SetNextColorGroup(colorGroupZero).GetColors());

                hasFired = true;
            }

            else if (value > 0.99f && !hasFired)
            {
                colorGroupZero.colors = (SetNextColorGroup(colorGroupOne).GetColors());
                hasFired = true;
            }
        }
        else
        {
            hasFired = false;
        }

        LerpBetweenColorgroups(colorGroupZero, colorGroupOne, value);
    }

    void LerpBetweenColorgroups(ColorGroup a, ColorGroup b, float lerp)
    {
        for (int i = 0; i < colorGroupSO.matList.Count; i++)
        {
            Color c = Color.Lerp(a.colors[i], b.colors[i], lerp);
            c.a = 1f;
            colorGroupActual.colors[i] = c;
            colorGroupSO.matList[i].color = c;
        }

        if (backgroundChange != 0)
        {
            cam.backgroundColor = colorGroupActual.colors[backgroundChange - 1];
        }
    }




    private void OnEnable()
    {
        enumIndex = (int)sonicInputType;
        Subscribe(enumIndex);
    }

    private void OnDisable()
    {
        enumIndex = (int)sonicInputType;
        Unsubscribe(enumIndex);
    }

    void Subscribe(int index)
    {
        GetPlayer().ID.events[index] += SetValue;
    }

    void Unsubscribe(int index)
    {
        GetPlayer().ID.events[index] -= SetValue;
    }

    void OnValidate()
    {
        //    Debug.Log(enumIndex + " : " + (int)sonicInputType);
        // Check if (inspector dropdown enum) sonicInputType has changed
        if (enumIndex != (int)sonicInputType)
        {
            // Unsubscribe from current subscription
            Unsubscribe(enumIndex);

            //Update subscription index to match newly selected inspector dropdown enum
            enumIndex = (int)sonicInputType;
            // Subscribe to new subscription
            Subscribe(enumIndex);
        }
    }

}






