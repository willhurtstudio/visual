// using UnityEngine;
// using UnityEditor;

// #if UNITYEDITOR
// [CustomPropertyDrawer(typeof(MicrophoneDeviceAttribute))]
// public class MicrophoneDeviceDrawer : PropertyDrawer
// {
//     public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//     {
//         EditorGUI.BeginProperty(position, label, property);

//         // Get available microphone devices
//         string[] devices = Microphone.devices;

//         if (devices.Length == 0)
//         {
//             EditorGUI.LabelField(position, label, new GUIContent("No microphone devices found"));
//             EditorGUI.EndProperty();
//             return;
//         }

//         // Create options array for the dropdown
//         string[] options = new string[devices.Length];
//         for (int i = 0; i < devices.Length; i++)
//         {
//             options[i] = $"{i}: {devices[i]}";
//         }

//         // Get current selection
//         int currentIndex = property.intValue;
//         if (currentIndex >= devices.Length)
//         {
//             currentIndex = 0;
//             property.intValue = 0;
//         }

//         // Draw the dropdown
//         int newIndex = EditorGUI.Popup(position, label.text, currentIndex, options);

//         // Update the property if selection changed
//         if (newIndex != currentIndex)
//         {
//             property.intValue = newIndex;
//             property.serializedObject.ApplyModifiedProperties();
//         }

//         EditorGUI.EndProperty();
//     }

//     public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
//     {
//         return EditorGUI.GetPropertyHeight(property, label, true);
//     }
// }

// // Custom attribute to mark fields that should use the microphone device drawer
// public class MicrophoneDeviceAttribute : PropertyAttribute
// {
// }

// #endif