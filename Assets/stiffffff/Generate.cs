// using UnityEngine;
// using System.Collections.Generic;


// public class Generate : MonoBehaviour
// {
//     // Start is called once before the first execution of Update after the MonoBehaviour is created
//     public enum Type { Cardinal, Random, Uniform };
//     public static float[] cardinalRotations = new float[] { 0, 90, 180, 270 };

//     public static Vector3 Position()
//     {
//         return new Vector3(Random.value - 0.5f, Random.value - 0.5f, Random.value - 0.5f);
//     }

//     public static Vector3 Rotation(Type rotationType)
//     {
//         Vector3 value = Vector3.zero;

//         if (rotationType == Type.Cardinal)
//         {
//             value = new Vector3(cardinalRotations[Random.Range(0, cardinalRotations.Length)], cardinalRotations[Random.Range(0, cardinalRotations.Length)], cardinalRotations[Random.Range(0, cardinalRotations.Length)]);
//         }
//         else if (rotationType == Type.Random)
//         {
//             value = Random.rotation.eulerAngles;
//         }
//         else if (rotationType == Type.Uniform)
//         {
//             //
//         }
//         return value;
//     }


//     public static Vector3 Scale(Type scaleType)
//     {
//         Vector3 value = new Vector3(0, 0, 0);
//         if (scaleType == Type.Random)
//         {
//             value = new Vector3(Random.value, Random.value, Random.value);
//         }
//         else if (scaleType == Type.Uniform)
//         {
//             float v = Random.value;
//             value = new Vector3(v, v, v);
//         }
//         return value;
//     }
// }
