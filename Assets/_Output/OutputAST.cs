using UnityEngine;
using System.Collections.Generic;


public class OutputAST : SonicSystem
{
    public float changePoint = 1f;
    public float changeAt = 1f;
    bool hasFired;
    public Enums.BeatDurations animationDuration;
    Enums.BeatDurations animationDuration_old;
    public int maximumMoves = 24;
    public int moveCounter = 0;

    float duration;

    public void SetValue(float _value)
    {
        value = ConvertAndSetValue(_value); // calls convert in base class to ensure smoothing / thresholding are adhered to

        changeAt = value % changePoint;

        if (changeAt < 0.05f)
        {
            if (!hasFired)
            {
                if (moveCounter >= maximumMoves)
                {
                    TriggerAnimationOnAll("reset");
                    hasFired = true;
                    moveCounter = 0;

                }
                else
                {
                    moveCounter++;
                    TriggerRandomAnimationOnePerFrame();
                    hasFired = true;
                }
            }
        }
        else
        {
            hasFired = false;
        }

        if (animationDuration != animationDuration_old)
        {
            duration = player.ID.GetBeatDuration(animationDuration);
            animationDuration = animationDuration_old;
        }
    }


    private void OnEnable()
    {
        enumIndex = (int)sonicInputType;
        Subscribe(enumIndex);

        AddCardinalAnimatorsToChildren();
        InitializeManager(); // or in AWAKE?
    }

    private void OnDisable()
    {
        enumIndex = (int)sonicInputType;
        Unsubscribe(enumIndex);
    }

    void Subscribe(int index)
    {
        GetPlayer().ID.events[index] += SetValue;
    }

    void Unsubscribe(int index)
    {
        GetPlayer().ID.events[index] -= SetValue;
    }

    void OnValidate()
    {
        //        Debug.Log(enumIndex + " : " + (int)sonicInputType);
        // Check if (inspector dropdown enum) sonicInputType has changed
        if (enumIndex != (int)sonicInputType)
        {
            // Unsubscribe from current subscription
            Unsubscribe(enumIndex);

            //Update subscription index to match newly selected inspector dropdown enum
            enumIndex = (int)sonicInputType;
            // Subscribe to new subscription
            Subscribe(enumIndex);
        }
    }



    // public void OnEnable()
    // {
    //     Subscribe(this);
    //     base.DoValidation(this);
    //     AddCardinalAnimatorsToChildren();
    //     InitializeManager(); // or in AWAKE?
    // }

    // public void OnValidate()
    // {
    //     base.DoValidation(this);
    // }


    [Header("Bounds Settings")]
    [SerializeField] private GameObject _boundsReference;
    [SerializeField] private bool _useBoundsReference = false;



    [Header("Animation Settings")]
    [SerializeField] private float _cooldownTime = 0.2f;

    [Header("Animation Distribution")]
    [SerializeField] private float _rotationChance = 0.4f;
    [SerializeField] private float _movementChance = 0.3f;
    [SerializeField] private float _extrusionChance = 0.3f;

    [Header("Debug")]
    [SerializeField] private bool _showDebugInfo = false;
    [SerializeField] private bool _showBounds = true;

    // Private state
    private List<ASTAnimator> _animators = new List<ASTAnimator>();
    private Dictionary<ASTAnimator, float> _cooldowns = new Dictionary<ASTAnimator, float>();
    private bool _isInitialized = false;
    private Bounds _animationBounds;
    private Renderer _boundsRenderer;
    private float _lastTriggerTime = 0f; // Global cooldown to prevent rapid-fire triggers
    private int _lastTriggerFrame = -1; // Prevent multiple triggers in the same frame

    // // ScenePresetManager integration
    // private AudioConnection _currentAudioConnection;
    // private float _currentAudioValue = 0f;
    // private bool _isConnectedToAudioData = false;


    // void Start()
    // {
    //     InitializeManager();
    // }


    void Update()
    {
        UpdateCooldowns();
    }


    private void InitializeManager()
    {
        // Setup bounds
        SetupAnimationBounds();

        // Find all CardinalAnimators in the scene
        ASTAnimator[] foundAnimators = FindObjectsByType<ASTAnimator>(FindObjectsSortMode.None);
        _animators.AddRange(foundAnimators);

        if (_showDebugInfo)
            Debug.Log($"CardinalAnimatorManager: Found {_animators.Count} animators");




        // Subscribe to events
        //SubscribeToEvents();
        _isInitialized = true;

        if (_showDebugInfo)
            Debug.Log("CardinalAnimatorManager: Initialized successfully");
    }

    private void SetupAnimationBounds()
    {
        // Check if we should use a bounds reference
        if (_boundsReference != null)
        {
            _animationBounds = GetBoundsFromGameObject(_boundsReference);

            Debug.Log($"CardinalAnimatorManager: Using bounds reference {_boundsReference.name}: {_animationBounds}");

        }
        else
        {
            Debug.LogError("Assign Bounds reference to Caridnal MAnager");
        }

        // Get or add a renderer to define bounds
        _boundsRenderer = GetComponent<Renderer>();
        if (_boundsRenderer == null)
        {
            // Add a BoxCollider to define bounds if no renderer exists
            BoxCollider boundsCollider = GetComponent<BoxCollider>();
            if (boundsCollider == null)
            {
                boundsCollider = gameObject.AddComponent<BoxCollider>();
                boundsCollider.size = new Vector3(10f, 10f, 10f); // Default size
                boundsCollider.isTrigger = true; // Make it a trigger so it doesn't interfere with physics
            }
            _animationBounds = boundsCollider.bounds;
        }
        else
        {
            _animationBounds = _boundsRenderer.bounds;
        }

        // Disable the renderer so it's not visible
        if (_boundsRenderer != null)
        {
            _boundsRenderer.enabled = false;
        }

        if (_showDebugInfo)
            Debug.Log($"CardinalAnimatorManager: Animation bounds set to {_animationBounds} (world space)");
    }

    void OnDrawGizmos()
    {
        if (_showBounds && _isInitialized)
        {
            // Draw manager bounds
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(_animationBounds.center, _animationBounds.size);

            // Draw bounds reference if using one
            if (_useBoundsReference && _boundsReference != null)
            {
                Gizmos.color = Color.cyan;
                Bounds refBounds = GetBoundsFromGameObject(_boundsReference);
                Gizmos.DrawWireCube(refBounds.center, refBounds.size);
            }
        }
    }

    /// <summary>
    /// Get bounds from a GameObject, considering all renderers and colliders
    /// </summary>
    private Bounds GetBoundsFromGameObject(GameObject obj)
    {
        if (obj == null) return new Bounds();

        // Try to get bounds from renderers first
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            Bounds combinedBounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                combinedBounds.Encapsulate(renderers[i].bounds);
            }
            return combinedBounds;
        }
        else
        {
            Debug.LogError("CardinalAnimatorManager: No renderers found for " + obj.name + " creating one, this is not good");
            return new Bounds(obj.transform.position, obj.transform.lossyScale);
        }

        // // Fallback to colliders
        // Collider[] colliders = obj.GetComponentsInChildren<Collider>();
        // if (colliders.Length > 0)
        // {
        //     Bounds combinedBounds = colliders[0].bounds;
        //     for (int i = 1; i < colliders.Length; i++)
        //     {
        //         combinedBounds.Encapsulate(colliders[i].bounds);
        //     }
        //     return combinedBounds;
        // }

        // // Last resort: use transform bounds
        // return new Bounds(obj.transform.position, obj.transform.lossyScale);
    }

    // private void SubscribeToEvents()
    // {
    // if (_spectrumAnalysis == null) return;

    // // Subscribe to band events
    // if (_subscribeToBypassBand)
    // {
    //     var bypassBand = _spectrumAnalysis.GetBand(Enums.BandType.Bypass);
    //     if (bypassBand != null)
    //     {
    //         bypassBand.onBandValueChange += OnBandValueChange;
    //     }
    // }

    // if (_subscribeToLowPassBand)
    // {
    //     var lowPassBand = _spectrumAnalysis.GetBand(Enums.BandType.LowPass);
    //     if (lowPassBand != null)
    //     {
    //         lowPassBand.OnThresholdPassed += () => OnBandThresholdPassed("LowPass");
    //     }
    // }

    // if (_subscribeToBandPassBand)
    // {
    //     var bandPassBand = _spectrumAnalysis.GetBand(Enums.BandType.BandPass);
    //     if (bandPassBand != null)
    //     {
    //         bandPassBand.OnThresholdPassed += () => OnBandThresholdPassed("BandPass");
    //     }
    // }

    // if (_subscribeToHighPassBand)
    // {
    //     var highPassBand = _spectrumAnalysis.GetBand(Enums.BandType.HighPass);
    //     if (highPassBand != null)
    //     {
    //         highPassBand.OnThresholdPassed += () => OnBandThresholdPassed("HighPass");
    //     }
    // }
    // }

    // private void UnsubscribeFromEvents()
    // {
    //     if (_spectrumAnalysis == null) return;

    //     // Note: Lambda expressions cannot be directly unsubscribed
    //     // The events will be cleaned up when the objects are destroyed
    //     // This is a limitation of using lambda expressions for event subscription
    // }

    private void TriggerRandomAnimationOnePerFrame()
    {
        // Prevent multiple triggers in the same frame
        if (Time.frameCount == _lastTriggerFrame)
        {

            Debug.Log($"CardinalAnimatorManager: Skipping threshold - already triggered this frame");
            return;

        }



        //        Debug.Log($"CardinalAnimatorManager: HighPass threshold passed at time {Time.time:F2}");


        _lastTriggerFrame = Time.frameCount;
        TriggerRandomAnimation();
    }

    private void TriggerRandomAnimation()
    {
        if (!_isInitialized || _animators.Count == 0) return;

        // Global cooldown to prevent rapid-fire triggers
        if (Time.time - _lastTriggerTime < _cooldownTime)
        {
            if (_showDebugInfo)
                Debug.Log($"CardinalAnimatorManager: Skipping trigger from  due to global cooldown");
            return;
        }

        // Get available animators (not on cooldown)
        List<ASTAnimator> availableAnimators = GetAvailableAnimators();

        if (availableAnimators.Count == 0)
        {
            if (_showDebugInfo)
                Debug.Log($"CardinalAnimatorManager: No available animators for");
            return;
        }

        if (_showDebugInfo)
            Debug.Log($"CardinalAnimatorManager:  threshold passed. Available animators: {availableAnimators.Count}");

        // Select random animator
        ASTAnimator selectedAnimator = availableAnimators[Random.Range(0, availableAnimators.Count)];

        selectedAnimator.SetAnimationDuration(0.5f);
        // Trigger random animation
        bool animationTriggered = TriggerRandomAnimationType(selectedAnimator);

        if (animationTriggered)
        {
            // Set cooldown for this specific animator
            _cooldowns[selectedAnimator] = Time.time + _cooldownTime;

            // Set global cooldown
            _lastTriggerTime = Time.time;

            // Trigger event


            if (_showDebugInfo)
                Debug.Log($"CardinalAnimatorManager: Triggered animation on {selectedAnimator.name} from ");
        }
        else
        {
            if (_showDebugInfo)
                Debug.Log($"CardinalAnimatorManager: Failed to trigger animation on {selectedAnimator.name} from");
        }
    }

    private bool TriggerRandomAnimationType(ASTAnimator animator)
    {
        float random = Random.Range(0f, 1f);
        float cumulative = 0f;

        cumulative += _rotationChance;
        if (random <= cumulative)
            return animator.RotateToCardinal();

        cumulative += _movementChance;
        if (random <= cumulative)
            return animator.MoveAlongCardinal();

        cumulative += _extrusionChance;
        if (random <= cumulative)
            return animator.ExtrudeAlongCardinal();

        // Fallback to rotation
        return animator.RotateToCardinal();
    }

    private List<ASTAnimator> GetAvailableAnimators()
    {
        List<ASTAnimator> available = new List<ASTAnimator>();
        float currentTime = Time.time;

        foreach (var animator in _animators)
        {
            if (animator == null) continue;

            if (!_cooldowns.ContainsKey(animator) || _cooldowns[animator] <= currentTime)
            {
                if (!animator.isAnimating)
                {
                    available.Add(animator);
                }
            }
        }

        return available;
    }

    private void UpdateCooldowns()
    {
        float currentTime = Time.time;
        List<ASTAnimator> toRemove = new List<ASTAnimator>();

        foreach (var kvp in _cooldowns)
        {
            if (kvp.Value <= currentTime)
            {
                toRemove.Add(kvp.Key);
            }
        }

        foreach (var animator in toRemove)
        {
            _cooldowns.Remove(animator);
        }
    }

    // Public methods for runtime control
    public void RegisterAnimator(ASTAnimator animator)
    {
        if (animator != null && !_animators.Contains(animator))
        {
            _animators.Add(animator);

            // Only set bounds if the animator isn't using a custom bounds reference
            if (!animator.IsUsingBoundsReference())
            {
                animator.SetAnimationBounds(_animationBounds);
            }

            if (_showDebugInfo)
                Debug.Log($"CardinalAnimatorManager: Registered {animator.name}");
        }
    }

    public void UnregisterAnimator(ASTAnimator animator)
    {
        if (_animators.Contains(animator))
        {
            _animators.Remove(animator);
            _cooldowns.Remove(animator);
            if (_showDebugInfo)
                Debug.Log($"CardinalAnimatorManager: Unregistered {animator.name}");
        }
    }

    public void TriggerAnimationOnAll(string animationType)
    {
        foreach (var animator in _animators)
        {
            if (animator == null || animator.isAnimating) continue;

            switch (animationType.ToLower())
            {
                case "rotate":
                    animator.RotateToCardinal();
                    break;
                case "move":
                    animator.MoveAlongCardinal();
                    break;
                case "extrude":
                    animator.ExtrudeAlongCardinal();
                    break;
                case "reset":
                    Debug.Log("RESETTTTTTTING");
                    animator.ResetToOriginal();
                    break;
            }
        }
    }

    // Getters for current state
    public int GetAnimatorCount() => _animators.Count;
    public int GetAvailableAnimatorCount() => GetAvailableAnimators().Count;
    public bool IsInitialized() => _isInitialized;
    //  public SpectrumAnalysis GetAudioTracker() => _spectrumAnalysis;
    public Bounds GetAnimationBounds() => _animationBounds;
    public GameObject GetBoundsReference() => _boundsReference;
    public bool IsUsingBoundsReference() => _useBoundsReference;

    // Public methods for bounds control
    public void SetBoundsReference(GameObject boundsObject)
    {
        _boundsReference = boundsObject;
        _useBoundsReference = boundsObject != null;

        if (_useBoundsReference && _boundsReference != null)
        {
            _animationBounds = GetBoundsFromGameObject(_boundsReference);
        }
        else
        {
            // Fall back to component-based bounds
            SetupAnimationBounds();
        }

        // Update all registered animators (only those not using custom bounds reference)
        foreach (var animator in _animators)
        {
            if (animator != null && !animator.IsUsingBoundsReference())
            {
                animator.SetAnimationBounds(_animationBounds);
            }
        }

        if (_showDebugInfo)
            Debug.Log($"CardinalAnimatorManager: Bounds reference set to {boundsObject?.name ?? "null"}");
    }

    public void UpdateBoundsFromReference()
    {
        if (_useBoundsReference && _boundsReference != null)
        {
            _animationBounds = GetBoundsFromGameObject(_boundsReference);

            // Update all registered animators (only those not using custom bounds reference)
            foreach (var animator in _animators)
            {
                if (animator != null && !animator.IsUsingBoundsReference())
                {
                    animator.SetAnimationBounds(_animationBounds);
                }
            }

            if (_showDebugInfo)
                Debug.Log($"CardinalAnimatorManager: Updated bounds from reference {_boundsReference.name}: {_animationBounds}");
        }
    }

    public void SetBoundsSize(Vector3 size)
    {
        // Only apply if not using bounds reference
        if (_useBoundsReference)
        {
            Debug.LogWarning("CardinalAnimatorManager: Cannot set bounds size when using bounds reference. Use SetBoundsReference(null) to disable.");
            return;
        }

        BoxCollider boundsCollider = GetComponent<BoxCollider>();
        if (boundsCollider != null)
        {
            boundsCollider.size = size;
            _animationBounds = boundsCollider.bounds;

            // Update all registered animators (only those not using custom bounds reference)
            foreach (var animator in _animators)
            {
                if (animator != null && !animator.IsUsingBoundsReference())
                {
                    animator.SetAnimationBounds(_animationBounds);
                }
            }
        }
    }

    public void SetBoundsCenter(Vector3 center)
    {
        // Only apply if not using bounds reference
        if (_useBoundsReference)
        {
            Debug.LogWarning("CardinalAnimatorManager: Cannot set bounds center when using bounds reference. Use SetBoundsReference(null) to disable.");
            return;
        }

        transform.position = center;
        if (_boundsRenderer != null)
        {
            _animationBounds = _boundsRenderer.bounds;
        }
        else
        {
            BoxCollider boundsCollider = GetComponent<BoxCollider>();
            if (boundsCollider != null)
            {
                _animationBounds = boundsCollider.bounds;
            }
        }

        // Update all registered animators (only those not using custom bounds reference)
        foreach (var animator in _animators)
        {
            if (animator != null && !animator.IsUsingBoundsReference())
            {
                animator.SetAnimationBounds(_animationBounds);
            }
        }
    }

    private void AddCardinalAnimatorsToChildren()
    {
        // Get all child GameObjects
        Transform[] childTransforms = GetComponentsInChildren<Transform>();

        int addedCount = 0;
        foreach (Transform child in childTransforms)
        {
            // Skip the manager's own transform
            if (child == transform) continue;

            // Check if the child already has a ASTAnimator
            ASTAnimator existingAnimator = child.GetComponent<ASTAnimator>();
            if (existingAnimator == null)
            {
                // Add ASTAnimator component
                ASTAnimator newAnimator = child.gameObject.AddComponent<ASTAnimator>();
                addedCount++;

                if (_showDebugInfo)
                    Debug.Log($"CardinalAnimatorManager: Added ASTAnimator to {child.name}");
            }
        }

        if (_showDebugInfo && addedCount > 0)
            Debug.Log($"CardinalAnimatorManager: Added {addedCount} ASTAnimator components to children");
    }
}
