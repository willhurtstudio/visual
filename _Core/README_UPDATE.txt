This update adds:
- _Core/Events: VoidEventChannelSO, FloatEventChannelSO
- _Core/Audio: MicrophoneManager (hot-plug), VolumeThreshold (RMS + rising edge)
- _Core/Display: ExternalDisplayManager (safe activation, removal handling)
- _Core/Tempo: TempoFollower (spectral-flux BPM tracking + confidence)
- _Core/Presets: VisualPresetSO, PresetController (apply & crossfade)
- _Core/Actions: RuntimeActionRouter + example actions (background, scale, spin)

How to try quickly:
1) Create ScriptableObjects for events (Assets/Create/Events/...).
2) Add an empty GameObject with:
   - AudioSource (mic input routed)
   - MicrophoneManager (optional), TempoFollower, VolumeThreshold
3) Create a Camera, add PresetController (bind Camera + a Transform to manipulate).
4) Add RuntimeActionRouter somewhere, hook its Trigger to VolumeThreshold.thresholdPassedEvent.
5) In Router, add action components (ChangeBackgroundColor, ScaleObject, SpinObject) and tick which to run.
6) Create a few VisualPresetSO assets and call PresetController.CrossfadeTo(...) to blend between them.
