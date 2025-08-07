using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public sealed class Monitor : MonoBehaviour
{
    TapTempo tempoTapDetector;
    AudioInput audioInput;

    private Button resetButton;
    private Button tapButton;

    private Button audioButton;
    void Start()
    {
        tempoTapDetector = FindFirstObjectByType<TapTempo>();
        audioInput = FindFirstObjectByType<AudioInput>();
        var root = GetComponent<UIDocument>().rootVisualElement;

        root.Q("audio-one").dataSource = FindFirstObjectByType<MicTest>();
        root.Q("audio-two").dataSource = FindFirstObjectByType<MicTestExternal>();

        root.Q("display-text").dataSource = FindFirstObjectByType<MultiDisplayTestDebug>();
        //root.Q("display-text-external").dataSource = FindFirstObjectByType<MultiDisplayTestExternal>();

        root.Q("tempo-text").dataSource = FindFirstObjectByType<TempoTest>();
        root.Q("tempo-instruction-text").dataSource = FindFirstObjectByType<TempoInstructionTest>();
        root.Q("audiolevels-text").dataSource = FindFirstObjectByType<AudioLevelTest>();

        //root.RegisterCallback<PointerDownEvent>((ev) => PointerDown(ev));
        tapButton = root.Q<Button>("tap-button");
        tapButton.RegisterCallback<ClickEvent>(TapClicked);

        resetButton = root.Q<Button>("reset-button");
        resetButton.RegisterCallback<ClickEvent>(ResetClicked);

        audioButton = root.Q<Button>("audio-button");
        audioButton.RegisterCallback<ClickEvent>(AudioClicked);

        // // Get a reference to the Button from UXML and assign it its action.
        // var uxmlButton = container.Q<Button>("the-uxml-button");
        // uxmlButton.RegisterCallback<MouseUpEvent>((evt) => action());
    }

    private void TapClicked(ClickEvent e)
    {
        tempoTapDetector.RegisterTap();
    }

    void ResetClicked(ClickEvent e)
    {
        SceneManager.LoadScene(0);
    }

    void AudioClicked(ClickEvent e)
    {
        audioInput.SetNextDevice();
    }
}
