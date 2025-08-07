using UnityEngine;


public class OutputScale : SonicSystem
{

    public Vector3 axes = Vector3.one;

    [Range(0.1f, 360f)]
    public float multiplier = 1f;

    // FIX: Implement Smoothing
    public void SetValue(float _value)
    {
        value = ConvertAndSetValue(_value); // calls convert in base class to ensure smoothing / thresholding are adhered to


        Vector3 amount = axes * value * multiplier;

        if (outputMode == Enums.OutputMode.Additive)
        {
            Vector3 current = transform.localScale;
            current += amount * Time.deltaTime;
            transform.localScale = current;
        }
        if (outputMode == Enums.OutputMode.Actual)
        {
            transform.localScale = amount;
        }

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
     //   Debug.Log(enumIndex + " : " + (int)sonicInputType);
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
