using UnityEngine;
/// <summary>
/// Computes a simple RMS level from an AudioSource and raises a Void event when threshold is crossed upward.
/// Use this as the trigger for user-selected actions.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class VolumeThreshold : MonoBehaviour {
    public FloatEventChannelSO levelEvent;
    public VoidEventChannelSO thresholdPassedEvent;
    [Range(0f, 1f)] public float threshold = 0.2f;
    public int window = 256;

    private AudioSource _src;
    private float _prevLevel;
    void Awake(){ _src = GetComponent<AudioSource>(); }
    void Update(){
        if (_src == null || !_src.isPlaying) return;
        float[] buf = new float[Mathf.Max(32, window)];
        _src.GetOutputData(buf, 0);
        float sum = 0f; for(int i=0;i<buf.Length;i++) sum += buf[i]*buf[i];
        float rms = Mathf.Sqrt(sum / buf.Length);
        levelEvent?.Raise(rms);
        // rising edge detection
        if (_prevLevel < threshold && rms >= threshold) thresholdPassedEvent?.Raise();
        _prevLevel = rms;
    }
}
