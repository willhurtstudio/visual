// using UnityEngine;
// using UnityEngine.Splines;
// using System.Collections.Generic;
// using PrimeTween;
// using Unity.Mathematics;

// public class SplineUser : MonoBehaviour
// {
//     public bool startTweenOnSetData = true;
//     //SplineContainer splineContainer;
//     MeshRenderer meshRenderer;
//     // DefaultChildData childData;

//     // GroupSpawner defaultParentSplineInitializer;
//     // Start is called once before the first execution of Update after the MonoBehaviour is created
//     void Start()
//     {
//         //splineContainer = GetComponent<SplineContainer>();
//         meshRenderer = GetComponent<MeshRenderer>();
//     }

//     // Update is called once per frame
//     void Update()
//     {

//     }

//     // public void SetData(DefaultChildData _childData)
//     // {
//     //    // childData = _childData;
//     //     //splineContainer = _childData.splineContainer;
//     //     meshRenderer = GetComponent<MeshRenderer>();
//     //     meshRenderer.enabled = false;
//     //    // defaultParentSplineInitializer = transform.parent.GetComponent<GroupSpawner>();
//     //     //splineContainer.Spline = childData.spline;


//     //     if (startTweenOnSetData)
//     //     {
//     //         StartTween();
//     //     }
//     // }

//     void StartTween()
//     {
//         Sequence sequence = Sequence.Create();
//         if (childData.startTimeA > 0f)
//         {
//             Debug.Log("Delay");
//             sequence.Group(Tween.Delay(transform, childData.startTimeA).OnComplete(Show));
//         }
//         else
//         {
//             Show();
//         }

//         if (childData.alignToSpline)
//         {
//             sequence.Chain(Tween.Custom(transform, childData.startLocationN, childData.stopLocationN, childData.durationA, (t, x) => AnimateFromSpline(x), childData.ease));
//             // sequence.Group(Tween.Custom(transform, childData.startLocationN, childData.stopLocationN, childData.durationA, (t, x) => AnimateScale(x), childData.ease));
//         }
//         else
//         {
//             sequence.Chain(Tween.Custom(transform, childData.startLocationN, childData.stopLocationN, childData.durationA, (t, x) => AnimateFromDataPoints(x), childData.ease));
//             // sequence.Group(Tween.Custom(transform, childData.startLocationN, childData.stopLocationN, childData.durationA, (t, x) => AnimateRotation(x), childData.ease));
//             // sequence.Group(Tween.Custom(transform, childData.startLocationN, childData.stopLocationN, childData.durationA, (t, x) => AnimateScale(x), childData.ease));
//         }
//         // if (childData.splineContainer.Splines[childData.siblingIndex].TryGetFloat4Data("Rotation", out SplineData<float4> rotationData))
//         // {
//         // //Debug.Log(childData.siblingIndex);
//         // float4 rotation = rotationData.Evaluate(childData.splineContainer.Splines[childData.siblingIndex], 0, InterpolatorUtility.LerpFloat4);
//         // float4 key = new float4(-1f, 1f, 0f, 0f);
//         // if (rotation.x == key.x && rotation.y == key.y && rotation.z == key.z && rotation.w == key.w)
//         // {
//         //     Debug.Log("Follow Generated TangentsLine");
//         //     //Follow Generated TangentsLine


//         // }
//         // else
//         // {
//         //Debug.Log("Follow User Rotation Scale Values");
//         // Follow User Rotation Values
//         // }

//         //}
//         //sequence.Group(Tween.Custom(transform.parent, 0f, 1f, childData.durationA, (s, y) => AnimateRotation(y), childData.ease));

//         sequence.ChainCallback(() => defaultParentSplineInitializer.RegisterCompletedAnimation());

//     }


//     void AnimateFromSpline(float progress)
//     {
//         // if (progress > float.MinValue)
//         // {
//         //     progress -= float.MinValue;
//         // }

//         //Set Position
//         var posOnSplineLocal = SplineUtility.EvaluatePosition(childData.splineContainer.Splines[childData.siblingIndex], progress);
//         transform.localPosition = posOnSplineLocal;
//         //Set Rotation, take a bit off the end to avoid errors at end of spline
//         var direction = SplineUtility.EvaluateTangent(childData.splineContainer.Splines[childData.siblingIndex], Mathf.Clamp(progress, 0f, 1f - 0.05f));
//         var upSplineDirection = SplineUtility.EvaluateUpVector(childData.splineContainer.Splines[childData.siblingIndex], Mathf.Clamp(progress, 0f, 1f - 0.05f));
//         var rot = Quaternion.LookRotation(direction, upSplineDirection);
//         transform.localRotation = rot;

//         // // Set Scale
//         // if (childData.splineContainer.Splines[childData.siblingIndex].TryGetFloat4Data("Scale", out SplineData<float4> scaleData))
//         // {
//         //     float index = childData.splineContainer.Splines[childData.siblingIndex].ConvertIndexUnit(progress, PathIndexUnit.Normalized, PathIndexUnit.Knot);
//         //     float4 scale = scaleData.Evaluate(childData.splineContainer.Splines[childData.siblingIndex], index, PathIndexUnit.Knot, InterpolatorUtility.LerpFloat4);
//         //     transform.localScale = new Vector3(scale.x, scale.y, scale.z);
//         // }
//     }

//     void AnimateFromDataPoints(float progress)
//     {
//         // Set Position
//         float3 position = childData.splineContainer.Splines[childData.siblingIndex].EvaluatePosition(progress);
//         transform.localPosition = position;

//         // Set Rotation
//         if (childData.splineContainer.Splines[childData.siblingIndex].TryGetFloat4Data("Rotation", out SplineData<float4> rotationData))
//         {
//             float index = childData.splineContainer.Splines[childData.siblingIndex].ConvertIndexUnit(progress, PathIndexUnit.Normalized, PathIndexUnit.Knot); //Knot is saved but we normalize it
//             float4 rotation = rotationData.Evaluate(childData.splineContainer.Splines[childData.siblingIndex], index, PathIndexUnit.Knot, InterpolatorUtility.LerpFloat4);
//             transform.localRotation = new Quaternion(rotation.x, rotation.y, rotation.z, rotation.w);
//         }

//         // Set Scale
//         if (childData.splineContainer.Splines[childData.siblingIndex].TryGetFloat4Data("Scale", out SplineData<float4> scaleData))
//         {
//             float index = childData.splineContainer.Splines[childData.siblingIndex].ConvertIndexUnit(progress, PathIndexUnit.Normalized, PathIndexUnit.Knot);
//             float4 scale = scaleData.Evaluate(childData.splineContainer.Splines[childData.siblingIndex], index, PathIndexUnit.Knot, InterpolatorUtility.LerpFloat4);
//             transform.localScale = new Vector3(scale.x, scale.y, scale.z);
//         }

//     }

//     void Show()
//     {
//         meshRenderer.enabled = true;

//     }
//     void Hide()
//     {
//         meshRenderer.enabled = false;
//     }
// }
