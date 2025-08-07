// using System;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;

// // Gets all Input from Ipad and Passes it on to Part Manager
// public class LeafGrabber : MonoBehaviour
// {
//     Transform instance;

//     public List<Material> materials = new List<Material>();
//     public List<Transform> leaves = new List<Transform>();

//     public Vector2 scaleBounds;

//     void DoLeaf()
//     {
//         if (instance != null)
//         {
//             Destroy(instance.gameObject);
//         }

//         instance = Instantiate(leaves[UnityEngine.Random.Range(0, leaves.Count)], this.transform.position, Quaternion.identity);
//         instance.GetComponent<Renderer>().sharedMaterial = materials[UnityEngine.Random.Range(0, materials.Count)];
//         instance.rotation = UnityEngine.Random.rotation;
//         float amount = UnityEngine.Random.Range(scaleBounds.x, scaleBounds.y);
//         Vector3 scale = new Vector3(amount, amount, amount);
//         instance.localScale = scale;


//     }

//     // Update is called once per frame
//     void Update()
//     {
//         DoLeaf();

//     }




//     // public void FakeARandomisedInput(int noOfFakeInputs)
//     // {        
//     //     partManager.NewUndoGroup();

//     //     for(int n = 0; n < noOfFakeInputs; n++)
//     //     {
//     //         deltaPosition = new Vector2(Random.value * 360f, Random.value * 360f);
//     //         Vector3 position = Camera.main.ViewportToWorldPoint(new Vector3(Random.value, Random.value, Camera.main.nearClipPlane + Random.Range(depthMinMax.x, depthMinMax.y)));
//     //         partManager.ReceiveInput(position, deltaPosition, Random.value * stylusPressureMultiplier);
//     //     }
//     //     partManager.UpdatePrefabs();

//     // } 

//     // public void FlatIsoCameraToggle(){
//     //     cameraIsIso = ! cameraIsIso;

//     //     if(cameraIsIso)
//     //     {
//     //         Camera.main.transform.localRotation = Quaternion.Euler(45f,35,0f);
//     //     }
//     //     else
//     //     {
//     //         Camera.main.transform.localRotation = Quaternion.identity;
//     //     }
//     // }   

//     // public void EdgeDetectionAllOn(){
//     //     edgesAreAllOn = !edgesAreAllOn;
//     //     if(edgesAreAllOn)
//     //     {
//     //         Camera.main.GetComponent<EdgeDetection>().edgesOnly = 1f;
//     //     }
//     //     else{
//     //         Camera.main.GetComponent<EdgeDetection>().edgesOnly = 0f;
//     //     }
//     // } 

//     // // Foregropund MiddleGround Background
//     // public void FMBSelector(int i){

//     //     float thirdOfClipPlaneDistance = (Camera.main.farClipPlane - Camera.main.nearClipPlane) / 3f;
//     //     if (i == 0){
//     //         depthMinMax = new Vector2(0.3f, 0.6f);
//     //     }
//     //     if (i == 1){
//     //         depthMinMax = new Vector2(0.6f, 1.2f);
//     //     }
//     //     if (i == 2){
//     //         depthMinMax = new Vector2(1.2f, 2f);
//     //     }

//     //     /*
//     //      float thirdOfClipPlaneDistance = (Camera.main.farClipPlane - Camera.main.nearClipPlane) / 3f;
//     //     if (i == 0){
//     //         depthMinMax = new Vector2(Camera.main.nearClipPlane + 2f, thirdOfClipPlaneDistance);
//     //     }
//     //     if (i == 1){
//     //         depthMinMax = new Vector2(thirdOfClipPlaneDistance, 2 * thirdOfClipPlaneDistance);
//     //     }
//     //     if (i == 2){
//     //         depthMinMax = new Vector2(2 * thirdOfClipPlaneDistance, Camera.main.farClipPlane);
//     //     }
//     //     */
//     // }
// }
