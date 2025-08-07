
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Splines;
public class OutputSpawnLeaf : SonicSystem
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public SplineController splineController;

    public SplineContainer splineContainer;

    public int counter = 0;
    public float spawnPoint;
    // public float changeAt = 1f;
    bool hasFired;
    public bool stopSpawning = false;
    public float spawnSizeMultipler = 6f;
    public List<float> spawnScales = new List<float>() { 0.2f, 0.2f, 1f, 1f, 1f, 2f, 4f };
    public float splineLength;
    public int numLeavesToSpawn;
    public float spawnEveryLengthOfSpline = 1f;

    public Vector2 durationMinMax;

    void Start()
    {
        splineContainer = GetComponent<SplineContainer>();
        splineLength = splineContainer.Splines[0].GetLength();
        numLeavesToSpawn = (int)(splineLength / spawnEveryLengthOfSpline);
        spawnPoint = 1f / (float)numLeavesToSpawn;
    }

    public void SetParent(Transform p, Transform c)
    {
        c.parent = p;
    }
    float currentTime = 0;

    public void SetValue(float _value)
    {
        float duration = Random.Range(durationMinMax.x, durationMinMax.y);
        //value = ConvertAndSetValue(_value); // calls convert in base class to ensure smoothing / thresholding are adhered to
        currentTime += Time.deltaTime;
        Vector3 point = splineContainer.Splines[0].EvaluatePosition(currentTime);
        Vector3 point_old = splineContainer.Splines[0].EvaluatePosition(currentTime - Time.deltaTime);
        Vector3 distance = point - point_old;

        float spawnAtModuloed = currentTime % spawnPoint;


        // if (value < 0.9f)
        // {
        //     Vector3 point = splineContainer.Splines[0].EvaluatePosition(value + 0.09f);
        //     Vector3 point_old = splineContainer.Splines[0].EvaluatePosition(value);
        //     Vector3 distance = point - point_old;

        //     float spawnAtModuloed = value % spawnPoint;

        if (spawnAtModuloed < 0.01f)
        {
            if (!hasFired)
            {
                counter++;
                if (counter < numLeavesToSpawn)
                {
                    Spawn(point, distance);
                    hasFired = true;
                }
                else
                {
                    stopSpawning = true;
                }
            }
        }
        else
        {
            hasFired = false;
        }


        if (stopSpawning)
        {
            if (GetComponentsInChildren<Transform>().Length == 1)
            {
                Destroy(gameObject);
            }
        }
    }



    public List<Transform> leafTransforms = new List<Transform>();
    public List<Material> materials = new List<Material>();


    public void Spawn(Vector3 position, Vector3 direction)
    {
        Transform instance = Instantiate(leafTransforms[Random.Range(0, leafTransforms.Count)], position, Random.rotation);
        instance.GetComponent<MeshRenderer>().material = materials[Random.Range(0, materials.Count)];
        float randomScale = spawnScales[Random.Range(0, spawnScales.Count)] * spawnSizeMultipler;
        instance.localScale = new Vector3(randomScale, randomScale, randomScale);
        instance.parent = this.transform;

        instance.GetComponent<MoveSpawnedObject>().SetDirection(direction);
    }

    public void OnNowNowIsParented()
    {
        enumIndex = (int)sonicInputType;
        Debug.Log("subscribeToIndex: " + enumIndex);
        //        Debug.Log(this.name + " wants to subscribe to : index " + enumIndex + " called " + player.ID.events[enumIndex]);
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

    // void OnValidate()
    // {
    //     Debug.Log(enumIndex + " : " + (int)sonicInputType);
    //     // Check if (inspector dropdown enum) sonicInputType has changed
    //     if (enumIndex != (int)sonicInputType)
    //     {
    //         // Unsubscribe from current subscription
    //         Unsubscribe(enumIndex);

    //         //Update subscription index to match newly selected inspector dropdown enum
    //         enumIndex = (int)sonicInputType;
    //         // Subscribe to new subscription
    //         Subscribe(enumIndex);
    //     }
    // }
}


