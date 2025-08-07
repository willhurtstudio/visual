// using UnityEngine;

// public class SonicConversions : SonicSystem
// {


//     [Range(0, 1)]
//     public float smoothedAmount;
//     [Range(0, 1)]
//     public float threshold = 0.9f;
//     [Range(0, 1)]
//     public float smoothedFallSpeed = 0.3f;

//     float thresholdPassed;
//     float thresholdActual;
//     float fall;
//     float value_old;


//     public float GetConvertedValue(Enums.InputConversion _type, float _value)
//     {
//         float amount = _value;

//         fall += Mathf.Pow(10, 1 + smoothedFallSpeed * 2) * Time.deltaTime;
//         smoothedAmount -= fall * Time.deltaTime;

//         if (smoothedAmount < amount)
//         {
//             smoothedAmount = amount;
//             fall = 0;
//         }
//         smoothedAmount = Mathf.Clamp01(smoothedAmount);


//         if (amount > threshold)
//         {
//             thresholdActual = amount;
//             thresholdPassed = 1;
//         }
//         else
//         {
//             thresholdActual = 0;
//             thresholdPassed = 0;
//         }
//         value_old = amount;

//         switch (_type)
//         {
//             case Enums.InputConversion.Raw:
//                 value = _value;
//                 break;
//             case Enums.InputConversion.Smoothed:
//                 value = smoothedAmount;
//                 break;
//             case Enums.InputConversion.ThresholdPassed:
//                 value = thresholdPassed;
//                 break;
//             case Enums.InputConversion.ThresholdActual:
//                 value = thresholdActual;
//                 break;
//             default:
//                 value = 0; // never used
//                 break;
//         }
//         return value;
//     }
// }
