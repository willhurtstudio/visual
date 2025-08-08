using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Linq;
using UnityEditor;

[RequireComponent(typeof(AudioSource))]

public class AudioInput : MonoBehaviour
{
    MicTest micTest;
    //[MicrophoneDevice]
    [SerializeField] private int selectedDeviceIndex = 0;

    private string[] deviceNames;
    public AudioSource _audioSrc;
    [Header("Microphone Settings")]
    [Tooltip("Select which microphone device to use")]

    // Property to get the selected device name
    public string SelectedDeviceName
    {
        get
        {
            if (deviceNames != null && selectedDeviceIndex >= 0 && selectedDeviceIndex < deviceNames.Length)
            {
                return deviceNames[selectedDeviceIndex];
            }
            return "No Device Selected";
        }
    }

    // Property to get all available device names
    public string[] AvailableDevices
    {
        get { return deviceNames; }
    }

    void Start()
    {
        micTest = FindFirstObjectByType<MicTest>();

        // Get all microphone devices
        deviceNames = Microphone.devices;

        // Log all available devices
        for (int i = 0; i < deviceNames.Length; i++)
        {
            Debug.Log($"Device {i}: {deviceNames[i]}");
            micTest.RecieveMicDetails($"Device {i}: {deviceNames[i]}");
        }

        if (_audioSrc == null)
            _audioSrc = GetComponent<AudioSource>();
        if (_audioSrc == null)
            _audioSrc = gameObject.AddComponent<AudioSource>();

        // Use the selected device
        if (selectedDeviceIndex >= 0 && selectedDeviceIndex < deviceNames.Length)
        {
            _audioSrc.clip = Microphone.Start(deviceNames[selectedDeviceIndex], true, 1, 44100);
            while (!(Microphone.GetPosition(null) > 0)) { } // this line removes lag.
            _audioSrc.loop = true;
            _audioSrc.Play();

            Debug.Log($"Using microphone device: {deviceNames[selectedDeviceIndex]}");
        }
        else
        {
            Debug.LogError($"Selected device index {selectedDeviceIndex} is out of range. Available devices: {deviceNames.Length}");
        }
    }


    // Method to get the current device name
    public string GetSelectedDeviceName()
    {
        return SelectedDeviceName;
    }

    // Method to get all available device names
    public string[] GetAllDeviceNames()
    {
        return deviceNames;
    }

    // Method to change device at runtime
    public void SetDevice(int deviceIndex)
    {
        if (deviceIndex >= 0 && deviceIndex < deviceNames.Length)
        {
            selectedDeviceIndex = deviceIndex;

            // Stop current recording if any
            if (Microphone.IsRecording(null))
            {
                Microphone.End(null);
            }

            // Start with new device
            if (_audioSrc != null)
            {
                _audioSrc.clip = Microphone.Start(deviceNames[selectedDeviceIndex], true, 1, 44100);
                while (!(Microphone.GetPosition(null) > 0)) { } // this line removes lag.
                _audioSrc.loop = true;
                _audioSrc.Play();
                Debug.Log($"Switched to microphone device: {deviceNames[selectedDeviceIndex]}");
                micTest.RecieveMicDetails($"Switched to microphone device: {deviceNames[selectedDeviceIndex]}");
            }
        }
        else
        {
            Debug.LogError($"Device index {deviceIndex} is out of range. Available devices: {deviceNames.Length}");
            micTest.RecieveMicDetails($"Device index {deviceIndex} is out of range. Available devices: {deviceNames.Length}");

        }
    }

    int currentIndex = 0;
    public void SetNextDevice()
    {
        currentIndex++;
        if (currentIndex < deviceNames.Length)
        {
            SetDevice(currentIndex);
        }
        else
        {
            currentIndex = 0;
            SetDevice(currentIndex);
        }
    }

    void Update()
    {
        // _audioSrc.GetSpectrumData(_spectrum, 0, FFTWindow.BlackmanHarris);
        // // printing _spectrum here gives the same decimal values each time.
        // Debug.Log(_spectrum[256]);
    }
}