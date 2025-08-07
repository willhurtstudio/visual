using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

using UnityEngine.Splines;

public class SplineController : MonoBehaviour
{
    public SplineContainer splineContainer;
    public int numberOfPoints = 4;
    public Vector2 distanceFromCamera = new Vector2(4, 16);
    public List<Vector3> curvePoints = new List<Vector3>();
    public Vector2 distanceMinMax = new Vector2(10, 100);
    public Vector2 angleMinMax = new Vector2(5, 22);

    GameObject box;

    public void SetBoundingBox(GameObject _box)
    {
        box = _box;
    }
    //public Spline spline;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        splineContainer = GetComponent<SplineContainer>();
        curvePoints = GenerateCurvePoints(numberOfPoints);
        splineContainer.Spline = CreateSpline(curvePoints);
    }

    List<Vector3> GenerateCurvePoints(int numberOfPoints)
    {
        List<Vector3> points = new List<Vector3>();// startPoint, UnityEngine.Random.insideUnitCircle * initialLength }; //this is the initial points

        for (int i = 1; i < numberOfPoints; i++)
        {
            // Vector3 direction = points[points.Count - 1] - points[points.Count - 2];
            // direction.Normalize();

            Vector3 newPoint = RandomPointInGO(box);
            //newPoint.Normalize();
            // newPoint *= UnityEngine.Random.Range(distanceBounds.x, distanceBounds.y); // ??? 
            // newPoint += points[points.Count - 1];
            points.Add(newPoint);
        }

        // //Rotate into 3d space
        // for (int j = 1; j < numberOfPoints; j++)
        // {
        //     points[j] = Rotated(points[j], UnityEngine.Random.rotation, points[0]);
        // }

        return points;
    }
    // TODO this method doesnt work
    //  List<Vector3> GenerateCurvePoints(Vector3 startPoint, float initialLength, int numberOfPoints, Vector2 distanceBounds, Vector2 angleBounds)
    // {
    //     List<Vector3> points = new List<Vector3>() { startPoint, UnityEngine.Random.insideUnitCircle * initialLength }; //this is the initial points

    //     for (int i = 1; i < numberOfPoints; i++)
    //     {
    //         Vector3 direction = points[points.Count - 1] - points[points.Count - 2];
    //         direction.Normalize();

    //         Vector3 newPoint = RotateVector2(direction, UnityEngine.Random.Range(angleBounds.x, angleBounds.y));
    //         //newPoint.Normalize();
    //         newPoint *= UnityEngine.Random.Range(distanceBounds.x, distanceBounds.y); // ??? 
    //         newPoint += points[points.Count - 1];
    //         points.Add(newPoint);
    //     }

    //     //Rotate into 3d space
    //     for (int j = 1; j < numberOfPoints; j++)
    //     {
    //         points[j] = Rotated(points[j], UnityEngine.Random.rotation, points[0]);
    //     }

    //     return points;
    // }

    public Spline CreateSpline(List<Vector3> points)
    {
        Spline mySpline = new Spline();

        List<BezierKnot> bezierKnots = new List<BezierKnot>();
        for (int i = 0; i < points.Count; i++)
        {
            float3 pointf3 = points[i];
            BezierKnot bezierKnot = new BezierKnot(pointf3);
            mySpline.Add(bezierKnot, TangentMode.AutoSmooth);
        }
        return mySpline;
    }

    // public Vector3 RandomScreenPoint()
    // {
    //     Ray ray = cam.ScreenPointToRay(new Vector3(UnityEngine.Random.value * Screen.width, UnityEngine.Random.value * Screen.height, 0f));
    //     Vector3 point = ray.GetPoint(UnityEngine.Random.Range(distanceFromCamera.x, distanceFromCamera.y));
    //     Debug.Log("STARTPOINTS IS AT : " + point);
    //     return point;
    // }

    public Vector3 RandomPointInGO(GameObject go)
    {
        Vector3 position = go.transform.position;
        Vector3 size = go.transform.localScale;
        Vector3 random = new Vector3(UnityEngine.Random.Range(-0.5f, 0.5f), UnityEngine.Random.Range(-0.5f, 0.5f), UnityEngine.Random.Range(-0.5f, 0.5f));

        Vector3 result = UnityEngine.Vector3.Scale(size, random) + position;
        Debug.Log("STARTPOINTS IS AT : " + result);
        return result;
    }

    Vector2 RotateVector2(Vector2 vec, float angle)
    {
        float newAngle = Mathf.Atan2(vec.y, vec.x) + angle * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(newAngle), Mathf.Sin(newAngle) * Mathf.Rad2Deg);
    }

    public Vector3 Rotated(Vector3 vector, Quaternion rotation, Vector3 pivot = default(Vector3))
    {
        return rotation * (vector - pivot) + pivot;
    }

    public Vector3 Rotated(Vector3 vector, Vector3 rotation, Vector3 pivot = default(Vector3))
    {
        return Rotated(vector, Quaternion.Euler(rotation), pivot);
    }

    public Vector3 Rotated(Vector3 vector, float x, float y, float z, Vector3 pivot = default(Vector3))
    {
        return Rotated(vector, Quaternion.Euler(x, y, z), pivot);
    }
}
