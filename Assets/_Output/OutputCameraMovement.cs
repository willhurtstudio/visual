using System.Collections.Generic;
using UnityEngine;


public class OutputCameraMovement : SonicSystem
{
    public List<CameraTransform> cameraTransforms = new List<CameraTransform>();
    public Camera cam;

    public float chance = 0.1f;
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

    public int counter = 0;
    public float changePoint;
    public float changeAt = 1f;
    bool hasFired;

    public void SetValue(float _value)
    {
        value = ConvertAndSetValue(_value); // calls convert in base class to ensure smoothing / thresholding are adhered to

        if (outputMode == Enums.OutputMode.Additive) // Goto NExt on 0
        {
            if (value < 0.01f && hasFired == false)
            {
                if (Random.value < chance)
                {
                    cameraTransforms[Random.Range(0, cameraTransforms.Count)].SetCamera(cam);
                    // counter++;
                    // counter = counter >= cameraTransforms.Count ? 0 : counter;
                    // cameraTransforms[counter].SetCamera(cam);
                    hasFired = true;
                }

            }
            else if (value > 0.05)
            {
                hasFired = false;
            }
            else
            {
                //hasFired = false;
            }
        }
        else
        {
            changePoint = 1f / cameraTransforms.Count; // Goto next throuhghout
            changeAt = value % changePoint;

            if (changeAt < 0.01f && Random.value < chance)
            {
                if (!hasFired)
                {
                    counter++;
                    counter = counter >= cameraTransforms.Count ? 0 : counter;
                    cameraTransforms[counter].SetCamera(cam);
                    hasFired = true;
                }
            }
            else
            {
                hasFired = false;
            }
        }
    }
}


[System.Serializable]
public class CameraTransform
{
    public Vector3 position;
    public Vector3 rotation;
    public float size;
    public bool isOrthographic;

    public void SetCamera(Camera cam)
    {
        cam.transform.position = this.position;
        cam.transform.rotation = Quaternion.Euler(this.rotation);
        cam.orthographic = this.isOrthographic;
        cam.orthographicSize = this.size;
        cam.fieldOfView = this.size;
    }
}
