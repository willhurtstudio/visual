// using UnityEngine;

// public class MicrophoneTester : SonicSystem
// {

//     int deviceCount = -10000; // Initialize to a value that is unlikely to match the actual device count

//     void Start()
//     {
//         foreach (var device in Microphone.devices)
//         {
//             Debug.Log("Name: " + device);
//         }
//     }

//     void Update()
//     {
//         if (deviceCount != Microphone.devices.Length)
//         {
//             deviceCount = Microphone.devices.Length;
//             Debug.Log("Device count changed: " + deviceCount);
//             player.ID.events.OnMicrophoneDeviceHasChanged?.Invoke(deviceCount);
//         }}

// }
