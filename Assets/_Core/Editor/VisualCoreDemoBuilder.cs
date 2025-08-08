#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class VisualCoreDemoBuilder {
    [MenuItem("Tools/Visual Core/Build Sample Scene")]
    public static void BuildSampleScene() {
        // Create/ensure folders
        string demoFolder = "Assets/_Core/DemoAssets";
        if (!AssetDatabase.IsValidFolder("Assets/_Core")) AssetDatabase.CreateFolder("Assets", "_Core");
        if (!AssetDatabase.IsValidFolder(demoFolder)) AssetDatabase.CreateFolder("Assets/_Core", "DemoAssets");

        // Create SO events
        var thresholdEvent = ScriptableObject.CreateInstance<VoidEventChannelSO>();
        var levelEvent = ScriptableObject.CreateInstance<FloatEventChannelSO>();
        var bpmEvent = ScriptableObject.CreateInstance<FloatEventChannelSO>();
        var confEvent = ScriptableObject.CreateInstance<FloatEventChannelSO>();
        AssetDatabase.CreateAsset(thresholdEvent, demoFolder + "/ThresholdPassed.asset");
        AssetDatabase.CreateAsset(levelEvent, demoFolder + "/LevelEvent.asset");
        AssetDatabase.CreateAsset(bpmEvent, demoFolder + "/BPMEvent.asset");
        AssetDatabase.CreateAsset(confEvent, demoFolder + "/ConfidenceEvent.asset");

        // Create Presets
        var presetRed = ScriptableObject.CreateInstance<VisualPresetSO>();
        presetRed.affectsBackground = true; presetRed.backgroundColor = Color.red;
        presetRed.affectsTransform = true; presetRed.targetScale = new Vector3(2,2,2); presetRed.spinSpeed = 120;
        AssetDatabase.CreateAsset(presetRed, demoFolder + "/Preset_Red.asset");

        var presetBlue = ScriptableObject.CreateInstance<VisualPresetSO>();
        presetBlue.affectsBackground = true; presetBlue.backgroundColor = Color.blue;
        presetBlue.affectsTransform = true; presetBlue.targetScale = new Vector3(1,1,1); presetBlue.spinSpeed = 0;
        AssetDatabase.CreateAsset(presetBlue, demoFolder + "/Preset_Blue.asset");

        AssetDatabase.SaveAssets();

        // New scene
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Camera setup
        Camera cam = Object.FindFirstObjectByType<Camera>();
        if (cam == null) cam = new GameObject("Main Camera").AddComponent<Camera>();
        cam.transform.position = new Vector3(0, 1.5f, -6f);
        cam.transform.LookAt(Vector3.zero);

        // Add PresetController on Camera
        var presetCtrl = cam.gameObject.AddComponent<PresetController>();
        presetCtrl.targetCamera = cam;

        // Audio host with AudioSource + Tempo/Threshold
        GameObject audioHost = new GameObject("AudioHost");
        var src = audioHost.AddComponent<AudioSource>();
        src.playOnAwake = true; src.loop = true; // Mic will stream into this by MicrophoneManager if you want
        var mic = audioHost.AddComponent<MicrophoneManager>();
        mic.inputLevelEvent = levelEvent;

        var tempo = audioHost.AddComponent<TempoFollower>();
        tempo.bpmEvent = bpmEvent;
        tempo.confidenceEvent = confEvent;

        var thresh = audioHost.AddComponent<VolumeThreshold>();
        thresh.inputSignal = levelEvent;
        thresh.onThresholdPassed = thresholdEvent;
        thresh.threshold = 0.2f;

        // A target cube to scale/spin
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = "Target";
        cube.transform.position = Vector3.zero;
        presetCtrl.targetTransform = cube.transform;

        // Action router with actions on same GO
        GameObject routerGO = new GameObject("RuntimeActionRouter");
        var router = routerGO.AddComponent<RuntimeActionRouter>();
        router.triggerEvent = thresholdEvent;

        var actBg = routerGO.AddComponent<ChangeBackgroundColor>();
        actBg.targetCamera = cam;
        actBg.color = new Color(0.9f, 0.2f, 0.2f);
        var actScale = routerGO.AddComponent<ScaleObject>();
        actScale.target = cube.transform;
        actScale.scaleTarget = new Vector3(2f, 2f, 2f);
        var actSpin = routerGO.AddComponent<SpinObject>();
        actSpin.target = cube.transform;
        actSpin.spinSpeed = 180f;
        router.actionScripts.Add(actBg);
        router.actionScripts.Add(actScale);
        router.actionScripts.Add(actSpin);
        // Default to background + scale on trigger
        router.selected.Add(UserActionType.ChangeBackgroundColor);
        router.selected.Add(UserActionType.ScaleObject);

        // Simple demo controller to switch presets on keys
        var helper = new GameObject("DemoControls").AddComponent<DemoInputHelper>();
        helper.presetController = presetCtrl;
        helper.presetA = presetRed;
        helper.presetB = presetBlue;

        // Save scene
        string scenePath = "Assets/_Core/DemoAssets/VisualCore_Sample.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        EditorUtility.DisplayDialog("Visual Core", "Sample scene created at:\n" + scenePath + "\n\nPlay it and speak into the mic or feed line audio.\nPress '1' to apply Red preset, '2' for Blue, 'F' to crossfade.", "OK");
        AssetDatabase.Refresh();
    }
}

// Helper MonoBehaviour to wire simple demo input
public class DemoInputHelper : MonoBehaviour {
    public PresetController presetController;
    public VisualPresetSO presetA;
    public VisualPresetSO presetB;
    public float fadeSeconds = 1.0f;

    void Update() {
        if (presetController == null) return;
        if (Input.GetKeyDown(KeyCode.Alpha1)) presetController.Apply(presetA);
        if (Input.GetKeyDown(KeyCode.Alpha2)) presetController.Apply(presetB);
        if (Input.GetKeyDown(KeyCode.F)) presetController.CrossfadeTo(presetB, fadeSeconds);
    }
}
#endif
