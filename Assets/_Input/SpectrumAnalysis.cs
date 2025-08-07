using UnityEngine;
using System;


public class SpectrumAnalysis : SonicSystem
{
    // SonicEventsManager sonicEventsManager; // we only have one mega Scriptable Object now
    // SonicEventsObject sonicEventsObject;
    [Range(0f, 1f)]
    public float volumeHack;
    public float bassHack;
    public float midsHack;
    public float hisHack;
    AudioLevelTest audioLevelTest;
    public AudioSource audioSource;
    [SerializeField] private bool _autoGain = true;
    [SerializeField, Range(-10, 40)] private float _gain = 6;
    [SerializeField, Range(1, 40)] private float _dynamicRange = 12;

    [SerializeField]
    private BandInfo[] _bands = new BandInfo[]
        {
        new BandInfo(Enums.BandType.Bypass, 0.5f, 0.3f, 20f, 20000f),
        new BandInfo(Enums.BandType.LowPass, 0.5f, 0.3f, 20f, 800f),
        new BandInfo(Enums.BandType.BandPass, 0.5f, 0.3f, 800f, 4000f),
        new BandInfo(Enums.BandType.HighPass, 0.5f, 0.3f, 4000f, 20000f)
        };

    [Header("Filter Settings")]
    [SerializeField] private float _filterFc = 960f; // Crossover frequency in Hz
    [SerializeField] private float _filterQ = 0.15f; // Filter Q factor

    [Header("Spectrum Analysis")]
    [SerializeField] private int _spectrumSize = 256;
    [SerializeField] private FFTWindow _fftWindow = FFTWindow.BlackmanHarris;
    [SerializeField] private float _sampleRate = 44100f;

    [Header("Output Values")]
    [SerializeField] private float _normalizedLevel = 0f;
    [SerializeField] private float _inputLevel = 0f;
    [SerializeField] private float _currentGain = 0f;

    // Private members for LASP-style functionality
    private const float kSilence = -60f;
    private float _head = kSilence;
    private float _fall = 0f;
    private float[] _spectrum;


    // Time-domain filtering
    private BiquadFilter[] _filters;
    private float[] _filteredSums;
    private int _sampleCount = 0;
    private const int _bufferSize = 1024;
    private float[] _audioBuffer;

    // Public properties for external access
    public float normalizedLevel => _normalizedLevel;
    public float inputLevel => _inputLevel;
    public float currentGain => _currentGain;
    public float[] spectrum => _spectrum;
    public BandInfo[] bands => _bands;


    // // Events for external listeners (general)
    // public System.Action<float> OnLevelChanged;
    // public System.Action<float[]> OnSpectrumUpdated;

    // public enum FilterType { Bypass, LowPass, BandPass, HighPass }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioLevelTest = FindFirstObjectByType<AudioLevelTest>();
        // sonicEventsManager = FindFirstObjectByType<SonicEventsManager>();
        //sonicEventsObject = sonicEventsManager.GetCurrentSonicEventsObject();

        // Get or add AudioSource component
        //audioSource = GetComponent<AudioSource>();
        if (!audioSource)
        {
            Debug.LogError(this.name + " has no assigned audioSource, connect in inspector");
            return;
        }


        // Initialize spectrum array
        _spectrum = new float[_spectrumSize];

        // Initialize time-domain filters
        InitializeFilters();

        // Initialize audio buffer
        _audioBuffer = new float[_bufferSize];

        // Enums.BandType[] bandTypeEnumArray = (Enums.BandType[])System.Enum.GetValues(typeof(Enums.BandType));

        // for (int i 0; i < bandTypeEnumArray; i++)
        // {

        // }
        // {

        //     band.bandType
        //     //band.onBandValueChange = sonicEventsObject.sonicEvents;
        //     // band.sonicEventsObject = sonicEventsObject;
        // }

    }

    private void InitializeFilters()
    {
        _filters = new BiquadFilter[4]; // One for each band type
        _filteredSums = new float[4];

        for (int i = 0; i < 4; i++)
        {
            _filters[i] = new BiquadFilter();
            var filterType = (Enums.BandType)i;
            float normalizedFc = _filterFc / _sampleRate;
            _filters[i].SetParameters(normalizedFc, _filterQ, filterType);

        }
    }

    // Update is called once per frame
    void Update()
    {
        if (audioSource == null || !audioSource.isPlaying)
            return;

        // Get spectrum data for visualization
        audioSource.GetSpectrumData(_spectrum, 0, _fftWindow);

        // Process time-domain audio data
        ProcessTimeDomainAudio();

        // Calculate input level from spectrum (RMS of spectrum data)
        _inputLevel = CalculateInputLevel();

        var dt = Time.deltaTime;

        // Auto gain control (LASP-style)
        if (_autoGain)
        {
            // Slowly return to the noise floor
            const float kDecaySpeed = 0.6f;
            _head = Mathf.Max(_head - kDecaySpeed * dt, kSilence);

            // Pull up by input with a small headroom
            var room = _dynamicRange * 0.05f;
            _head = Mathf.Clamp(_inputLevel - room, _head, 0);
        }

        // Calculate current gain
        _currentGain = _autoGain ? -_head : _gain;

        // Normalize the input value
        var normalizedInput = Mathf.Clamp01((_inputLevel + _currentGain) / _dynamicRange + 1);

        // Hold and fall down animation (LASP-style)
        _fall += Mathf.Pow(10, 1 + 0.3f * 2) * dt;
        _normalizedLevel -= _fall * dt;

        // Pull up by input
        if (_normalizedLevel < normalizedInput)
        {
            _normalizedLevel = normalizedInput;
            _fall = 0;
        }

        // Update all bands using time-domain filtered data
        UpdateBands(dt);

        //******** CALCULATE VOLUME HACK HACK HACK AHGACK 
        bassHack = bands[1].level;
        midsHack = bands[2].level;
        hisHack = bands[3].level;

        volumeHack = 0.001f + (bassHack + midsHack + hisHack) / 3f;


        player.ID.events[1]?.Invoke(volumeHack);
        player.ID.events[2]?.Invoke(bassHack);
        player.ID.events[3]?.Invoke(midsHack);
        player.ID.events[4]?.Invoke(hisHack);

        audioLevelTest.SetLevels(volumeHack, bassHack, midsHack, hisHack);





        // Trigger general events
        //OnLevelChanged?.Invoke(_normalizedLevel);
        //OnSpectrumUpdated?.Invoke(_spectrum); ///************

    }

    private void ProcessTimeDomainAudio()
    {
        // Get audio data from AudioSource
        audioSource.GetOutputData(_audioBuffer, 0);

        // Reset sums
        for (int i = 0; i < 4; i++)
        {
            _filteredSums[i] = 0f;
        }

        // Process each sample through the filters
        for (int i = 0; i < _audioBuffer.Length; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                float filtered = _filters[j].FeedSample(_audioBuffer[i]);
                _filteredSums[j] += filtered * filtered;
            }
        }

        // Calculate RMS for each band
        float rmsSteps = 1f / _audioBuffer.Length;
        for (int i = 0; i < 4; i++)
        {
            _filteredSums[i] = Mathf.Sqrt(_filteredSums[i] * rmsSteps);
        }
    }

    private void UpdateBands(float deltaTime)
    {
        for (int i = 0; i < _bands.Length; i++)
        {
            var band = _bands[i];

            // Get the filtered RMS level for this band
            float filteredRms = _filteredSums[i];

            // Convert RMS to dB
            float filteredDb = filteredRms > 0 ? 20 * Mathf.Log10(filteredRms) : kSilence;

            band.UpdateLevel(player, i, filteredDb, deltaTime);

        }
    }

    private float CalculateInputLevel()
    {
        if (_spectrum == null || _spectrum.Length == 0)
            return kSilence;

        // Calculate RMS of spectrum data
        float sum = 0f;
        for (int i = 0; i < _spectrum.Length; i++)
        {
            sum += _spectrum[i] * _spectrum[i];
        }
        float rms = Mathf.Sqrt(sum / _spectrum.Length);

        // Convert to dBFS
        float db = rms > 0 ? 20 * Mathf.Log10(rms) : kSilence;

        return db;
    }

    public BandInfo GetBand(int index)
    {
        return index >= 0 && index < _bands.Length ? _bands[index] : null;
    }


    public BandInfo GetBand(Enums.BandType bandType)
    {
        for (int i = 0; i < _bands.Length; i++)
        {
            if (_bands[i].bandType == bandType)
                return _bands[i];
        }
        return null;
    }

    // Get frequency band levels (useful for visualization)
    public float GetFrequencyBand(int bandIndex, int bandCount = 8)
    {
        if (_spectrum == null || bandIndex < 0 || bandIndex >= bandCount)
            return 0f;

        int startIndex = bandIndex * _spectrum.Length / bandCount;
        int endIndex = (bandIndex + 1) * _spectrum.Length / bandCount;

        float sum = 0f;
        for (int i = startIndex; i < endIndex; i++)
        {
            sum += _spectrum[i];
        }

        return sum / (endIndex - startIndex);
    }


    // Convert frequency to spectrum index using logarithmic scaling
    private int FrequencyToIndex(float frequency)
    {
        if (frequency <= 0) return 0;

        // Use logarithmic scaling similar to LASP's approach
        // Map frequencies logarithmically from 20Hz to 20kHz
        float minFreq = 20f;
        float maxFreq = 20000f;

        // Calculate normalized position on log scale
        float logMin = Mathf.Log10(minFreq);
        float logMax = Mathf.Log10(maxFreq);
        float logFreq = Mathf.Log10(frequency);

        float normalizedPosition = (logFreq - logMin) / (logMax - logMin);

        // Convert to spectrum index - use only the first half of the spectrum (real frequencies)
        int maxIndex = _spectrumSize / 2;
        int index = Mathf.RoundToInt(normalizedPosition * (maxIndex - 1));

        return Mathf.Clamp(index, 0, maxIndex - 1);
    }

    // Convert spectrum index to frequency using logarithmic scaling
    private float IndexToFrequency(int index)
    {
        float minFreq = 20f;
        float maxFreq = 20000f;

        // Calculate normalized position - use only the first half of the spectrum
        int maxIndex = _spectrumSize / 2;
        float normalizedPosition = (float)index / (maxIndex - 1);

        // Convert to frequency using log scale
        float logMin = Mathf.Log10(minFreq);
        float logMax = Mathf.Log10(maxFreq);
        float logFreq = logMin + normalizedPosition * (logMax - logMin);

        return Mathf.Pow(10f, logFreq);
    }



}

[System.Serializable]
public class BandInfo
{
    public Enums.BandType bandType;

    [Header("Threshold Settings")]
    [Range(0, 1)]
    public float threshold = 0.5f;

    [Range(0, 1)]
    public float fallSpeed = 0.3f;

    [Header("Frequency Range")]
    [Range(20, 20000)]
    public float frequencyStart;

    [Range(20, 20000)]
    public float frequencyEnd;

    [Header("Auto-Gain Settings")]
    public bool autoGain = true;
    [Range(-10, 40)]
    public float gain = 6f;
    [Range(1, 40)]
    public float dynamicRange = 12f;

    [Header("Output Values")]
    [Range(0, 1)]
    public float level = 0f;


    private float fall = 0f;
    private float head = -60f; // Auto-gain head level
    private const float kSilence = -60f;
    // public bool thresholdPassed = false;

    //public AudioFloatEventNEW onBandValueChange;

    //public Enums.MegaE sonicEventKind;
    //[SerializeReference]
    //public SonicEvent onBandValueChange;
    //public SonicEventsObject sonicEventsObject;


    public BandInfo(Enums.BandType bandType, float threshold = 0.5f, float fallSpeed = 0.3f, float frequencyStart = 20f, float frequencyEnd = 20000f)
    {
        this.bandType = bandType;
        this.threshold = threshold;
        this.fallSpeed = fallSpeed;
        this.frequencyStart = frequencyStart;
        this.frequencyEnd = frequencyEnd;
        this.autoGain = true;
        this.gain = 6f;
        this.dynamicRange = 12f;
        this.level = 0f;
        //this.smoothLevel = 0f;
        this.fall = 0f;
        this.head = kSilence;
        // this.thresholdPassed = false;
        // sonicEventsObject = _sonicEventsObject;
        if (bandType == Enums.BandType.Bypass)
        {
            this.autoGain = false;
            this.gain = 0f;
            this.dynamicRange = 20f;
        }
    }

    public void UpdateLevel(Player player, int i, float inputDb, float deltaTime)
    {
        // Apply auto-gain control (LASP-style)
        if (autoGain)
        {
            // Slowly return to the noise floor
            const float kDecaySpeed = 0.6f;
            head = Mathf.Max(head - kDecaySpeed * deltaTime, kSilence);

            // Pull up by input with a small headroom
            var room = dynamicRange * 0.05f;
            head = Mathf.Clamp(inputDb - room, head, 0);
        }

        // Calculate current gain
        float currentGain = autoGain ? -head : gain;

        // Normalize the input value to 0-1 range
        float normalizedInput = Mathf.Clamp01((inputDb + currentGain) / dynamicRange + 1);

        // Ensure normalized input is in 0-1 range
        normalizedInput = Mathf.Clamp01(normalizedInput);

        level = normalizedInput;

        //player.ID.events[i + 1]?.Invoke(level); // ******* i+1 is here as the events list is based of an ENUM of which 0 is None 5. (1,2,3,4) entries are volume, bass, mid, his


        //* Have moved smoothing  and threshold passed to the event listener class

        // // Update smooth level with fall
        // fall += Mathf.Pow(10, 1 + fallSpeed * 2) * deltaTime;
        // smoothLevel -= fall * deltaTime;

        // if (smoothLevel < level)
        // {
        //     smoothLevel = level;
        //     fall = 0;
        // }

        // // Ensure smoothLevel stays normalized
        // smoothLevel = Mathf.Clamp01(smoothLevel);

        // onBandSmoothValueChange?.TriggerEvent(smoothLevel);

        // // Check threshold with normalized values - only fire when crossing above threshold
        // bool currentlyPassed = smoothLevel >= threshold;
        // if (currentlyPassed && !thresholdPassed)
        // {
        //     // Only fire when crossing above threshold (getting louder)
        //     thresholdPassed = true;
        //     OnThresholdPassed?.Invoke();
        // }
        // else if (!currentlyPassed && thresholdPassed)
        // {
        //     // Reset when dropping below threshold (getting quieter)
        //     thresholdPassed = false;
        // }
    }
}


// Biquad IIR filter implementation similar to LASP
[System.Serializable]
public class BiquadFilter
{
    private float _a0, _a1, _a2;
    private float _b1, _b2;
    private float _z1, _z2;


    public void SetParameters(float Fc, float Q, Enums.BandType filterType)
    {
        var K = Mathf.Tan(Mathf.PI * Fc);
        var norm = 1f / (1f + K / Q + K * K);

        switch (filterType)
        {
            case Enums.BandType.Bypass:
                _a0 = 1f;
                _a1 = 0f;
                _a2 = 0f;
                break;
            case Enums.BandType.LowPass:
                _a0 = K * K * norm;
                _a1 = 2f * _a0;
                _a2 = _a0;
                break;
            case Enums.BandType.BandPass:
                _a0 = K / Q * norm;
                _a1 = 0f;
                _a2 = -_a0;
                break;
            case Enums.BandType.HighPass:
                _a0 = norm;
                _a1 = -2f * _a0;
                _a2 = _a0;
                break;
        }

        _b1 = 2f * (K * K - 1f) * norm;
        _b2 = (1f - K / Q + K * K) * norm;
    }

    public float FeedSample(float input)
    {
        var output = _a0 * input + _z1;
        _z1 = _a1 * input + _z2 - output * _b1;
        _z2 = _a2 * input - output * _b2;
        return output;
    }

    public void Reset()
    {
        _z1 = 0f;
        _z2 = 0f;
    }
}
