using UnityEngine;

public class MoveSpawnedObject : MonoBehaviour
{
    public Vector3 amount = Vector3.zero;

    [Range(0.001f, 0.999f)]
    public float falloff;
    bool isMoving = true;
    public float distanceMultipler = 5f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (isMoving)
        {
            amount *= falloff;
            transform.position += (amount * Time.deltaTime);

            if (amount.magnitude < 0.05f)
            {
                isMoving = false;
                Destroy(gameObject, 8f);
            }
        }
    }

    public void SetDirection(Vector3 direction)
    {
        amount = direction * distanceMultipler;
        isMoving = true;
    }
}
