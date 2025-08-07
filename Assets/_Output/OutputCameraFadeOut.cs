
using UnityEngine;

public class OutputCameraFadeOut : SonicSystem
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public Canvas canvas;

    void SetValue(float _value)
    {
        value = ConvertAndSetValue(_value); // calls convert in base class to ensure smoothing / thresholding are adhered to
        transform.Rotate(0f, value, 0f);
    }

    private void OnEnable()
    {

        enumIndex = (int)sonicInputType;
        Debug.Log("subscribeToIndex: " + enumIndex);
        Debug.Log(this.name + " wants to subscribe to : index " + enumIndex + " called " + player.ID.events[enumIndex]);
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
        //player.ID.events[index] -= SetValue;
        GetPlayer().ID.events[index] -= SetValue;
    }




    void OnValidate()
    {
        Debug.Log(enumIndex + " : " + (int)sonicInputType);
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


