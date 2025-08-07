

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ASTAnimator : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float _animationDuration = 0.2f;
    [SerializeField] private AnimationCurve _animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Randomization Settings")]
    [SerializeField] private float _minMovementDistance = 0.5f;
    [SerializeField] private float _maxMovementDistance = 2f;
    [SerializeField] private float _minExtrusionScale = 0.5f; // Values below 1.0 shrink, above 1.0 grow
    [SerializeField] private float _maxExtrusionScale = 2f;
    [SerializeField] private bool _excludeCurrentRotation = true;

    [Header("Bounds Settings")]
    [SerializeField] private GameObject _boundsReference;
    [SerializeField] private bool _useBoundsReference = true;

    [Header("Debug")]
    [SerializeField] private bool _showDebugInfo = false;

    // Private state
    private bool _isAnimating = false;
    private Vector3 _originalPosition;
    private Vector3 _originalScale;
    private Quaternion _originalRotation;
    private CardinalDirection _currentRotation = CardinalDirection.North;
    private Bounds _animationBounds;
    private bool _hasBounds = false;

    // Public properties
    public bool isAnimating => _isAnimating;
    public float animationDuration => _animationDuration;


    // Cardinal directions
    public enum CardinalDirection
    {
        North,  // +Z
        South,  // -Z
        East,   // +X
        West,   // -X
        Up,     // +Y
        Down    // -Y
    }

    void Start()
    {
        // Store original transform values
        _originalPosition = transform.localPosition;
        _originalScale = transform.localScale;
        _originalRotation = transform.localRotation;

        // Determine current rotation
        _currentRotation = DetermineCurrentRotation();

        if (_showDebugInfo)
            Debug.Log($"{gameObject.name}: Initial rotation determined as {_currentRotation} (exclude current: {_excludeCurrentRotation})");

        // Update bounds from reference if set
        if (_useBoundsReference)
        {
            UpdateBoundsFromReference();
        }

        // Register with manager
        RegisterWithManager();
    }

    void OnDestroy()
    {
        // Unregister from manager
        UnregisterFromManager();
    }

    private void RegisterWithManager()
    {
        OutputAST manager = FindFirstObjectByType<OutputAST>();
        if (manager != null)
        {
            manager.RegisterAnimator(this);
        }
    }

    private void UnregisterFromManager()
    {
        OutputAST manager = FindFirstObjectByType<OutputAST>();
        if (manager != null)
        {
            manager.UnregisterAnimator(this);
        }
    }

    // Public method to set animation bounds
    public void SetAnimationBounds(Bounds bounds)
    {
        _animationBounds = bounds;
        _hasBounds = true;

        if (_showDebugInfo)
            Debug.Log($"{gameObject.name}: Animation bounds set to {bounds}");
    }

    /// <summary>
    /// Set a GameObject whose bounds will be used for animation constraints
    /// </summary>
    /// <param name="boundsObject">The GameObject to use for bounds reference</param>
    public void SetBoundsReference(GameObject boundsObject)
    {
        _boundsReference = boundsObject;
        _useBoundsReference = boundsObject != null;
        UpdateBoundsFromReference();

        if (_showDebugInfo)
            Debug.Log($"{gameObject.name}: Bounds reference set to {boundsObject?.name ?? "null"}");
    }

    /// <summary>
    /// Update bounds from the reference GameObject
    /// </summary>
    public void UpdateBoundsFromReference()
    {
        if (!_useBoundsReference || _boundsReference == null) return;

        // Get bounds from the reference object
        Bounds referenceBounds = GetBoundsFromGameObject(_boundsReference);

        if (referenceBounds.size.magnitude > 0)
        {
            _animationBounds = referenceBounds;
            _hasBounds = true;

            if (_showDebugInfo)
                Debug.Log($"{gameObject.name}: Updated bounds from reference {_boundsReference.name}: {referenceBounds}");
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: Reference object {_boundsReference.name} has no valid bounds!");
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

        // Fallback to colliders
        Collider[] colliders = obj.GetComponentsInChildren<Collider>();
        if (colliders.Length > 0)
        {
            Bounds combinedBounds = colliders[0].bounds;
            for (int i = 1; i < colliders.Length; i++)
            {
                combinedBounds.Encapsulate(colliders[i].bounds);
            }
            return combinedBounds;
        }

        // Last resort: use transform bounds
        return new Bounds(obj.transform.position, obj.transform.lossyScale);
    }

    /// <summary>
    /// Rotate the object to a random cardinal direction
    /// </summary>
    /// <returns>True if animation started, false if already animating</returns>
    public bool RotateToCardinal()
    {
        if (_isAnimating)
        {
            if (_showDebugInfo)
                Debug.Log($"{gameObject.name}: Already animating, cannot rotate");
            return false;
        }

        CardinalDirection randomDirection = GetRandomCardinalDirection();
        Vector3 targetRotation = GetCardinalRotation(randomDirection);
        _currentRotation = randomDirection;

        if (_showDebugInfo)
            Debug.Log($"{gameObject.name}: Rotating to {randomDirection} (target rotation: {targetRotation})");

        StartCoroutine(AnimateRotation(targetRotation));
        return true;
    }

    /// <summary>
    /// Rotate the object to a specific cardinal direction
    /// </summary>
    /// <param name="direction">The cardinal direction to rotate towards</param>
    /// <returns>True if animation started, false if already animating</returns>
    public bool RotateToCardinal(CardinalDirection direction)
    {
        if (_isAnimating)
        {
            if (_showDebugInfo)
                Debug.Log($"{gameObject.name}: Already animating, cannot rotate to {direction}");
            return false;
        }

        Vector3 targetRotation = GetCardinalRotation(direction);
        _currentRotation = direction;
        StartCoroutine(AnimateRotation(targetRotation));
        return true;
    }

    /// <summary>
    /// Move the object along a random cardinal direction with random distance
    /// </summary>
    /// <returns>True if animation started, false if already animating</returns>
    public bool MoveAlongCardinal()
    {
        if (_isAnimating)
        {
            if (_showDebugInfo)
                Debug.Log($"{gameObject.name}: Already animating, cannot move");
            return false;
        }

        CardinalDirection randomDirection = GetRandomCardinalDirection();
        float randomDistance = Random.Range(_minMovementDistance, _maxMovementDistance);
        Vector3 targetPosition = _originalPosition + GetCardinalVector(randomDirection) * randomDistance;

        // Clamp to bounds if bounds are set
        if (_hasBounds)
        {
            targetPosition = ClampPositionToBounds(targetPosition);
        }

        StartCoroutine(AnimatePosition(targetPosition));
        return true;
    }

    /// <summary>
    /// Move the object along a specific cardinal direction
    /// </summary>
    /// <param name="direction">The cardinal direction to move in</param>
    /// <returns>True if animation started, false if already animating</returns>
    public bool MoveAlongCardinal(CardinalDirection direction)
    {
        if (_isAnimating)
        {
            if (_showDebugInfo)
                Debug.Log($"{gameObject.name}: Already animating, cannot move to {direction}");
            return false;
        }

        Vector3 targetPosition = _originalPosition + GetCardinalVector(direction) * _maxMovementDistance;

        // Clamp to bounds if bounds are set
        if (_hasBounds)
        {
            targetPosition = ClampPositionToBounds(targetPosition);
        }

        StartCoroutine(AnimatePosition(targetPosition));
        return true;
    }

    /// <summary>
    /// Extrude the object along a random cardinal direction with random scale
    /// </summary>
    /// <returns>True if animation started, false if already animating</returns>
    public bool ExtrudeAlongCardinal()
    {
        if (_isAnimating)
        {
            if (_showDebugInfo)
                Debug.Log($"{gameObject.name}: Already animating, cannot extrude");
            return false;
        }

        CardinalDirection randomDirection = GetRandomCardinalDirection();
        float randomScale = Random.Range(_minExtrusionScale, _maxExtrusionScale);
        float randomDistance = Random.Range(_minMovementDistance * 0.5f, _maxMovementDistance * 0.5f);

        Vector3 cardinalVector = GetCardinalVector(randomDirection);

        // Move by half the extrusion distance in the direction
        Vector3 targetPosition = _originalPosition + cardinalVector * randomDistance;

        // Scale by the extrusion amount in the direction
        Vector3 targetScale = _originalScale;
        if (cardinalVector.x != 0) targetScale.x = _originalScale.x * randomScale;
        if (cardinalVector.y != 0) targetScale.y = _originalScale.y * randomScale;
        if (cardinalVector.z != 0) targetScale.z = _originalScale.z * randomScale;

        // Clamp to bounds if bounds are set
        if (_hasBounds)
        {
            targetPosition = ClampPositionToBounds(targetPosition);
            targetScale = ClampScaleToBounds(targetScale);
        }

        StartCoroutine(AnimateExtrusion(targetPosition, targetScale));
        return true;
    }

    /// <summary>
    /// Extrude the object along a specific cardinal direction
    /// </summary>
    /// <param name="direction">The cardinal direction to extrude along</param>
    /// <returns>True if animation started, false if already animating</returns>
    public bool ExtrudeAlongCardinal(CardinalDirection direction)
    {
        if (_isAnimating)
        {
            if (_showDebugInfo)
                Debug.Log($"{gameObject.name}: Already animating, cannot extrude to {direction}");
            return false;
        }

        Vector3 cardinalVector = GetCardinalVector(direction);

        // Move by half the extrusion distance in the direction
        Vector3 targetPosition = _originalPosition + cardinalVector * (_maxMovementDistance * 0.5f);

        // Scale by the extrusion amount in the direction
        Vector3 targetScale = _originalScale;
        if (cardinalVector.x != 0) targetScale.x = _originalScale.x * _maxExtrusionScale;
        if (cardinalVector.y != 0) targetScale.y = _originalScale.y * _maxExtrusionScale;
        if (cardinalVector.z != 0) targetScale.z = _originalScale.z * _maxExtrusionScale;

        // Clamp to bounds if bounds are set
        if (_hasBounds)
        {
            targetPosition = ClampPositionToBounds(targetPosition);
            targetScale = ClampScaleToBounds(targetScale);
        }

        StartCoroutine(AnimateExtrusion(targetPosition, targetScale));
        return true;
    }

    /// <summary>
    /// Reset the object to its original transform values
    /// </summary>
    /// <returns>True if animation started, false if already animating</returns>
    public bool ResetToOriginal()
    {
        if (_isAnimating)
        {
            if (_showDebugInfo)
                Debug.Log($"{gameObject.name}: Already animating, cannot reset");
            return false;
        }

        StartCoroutine(AnimateReset());
        return true;
    }

    // Helper methods
    private CardinalDirection GetRandomCardinalDirection()
    {
        CardinalDirection[] directions = System.Enum.GetValues(typeof(CardinalDirection)) as CardinalDirection[];

        if (_excludeCurrentRotation && directions.Length > 1)
        {
            // Create a list excluding the current rotation
            List<CardinalDirection> availableDirections = new List<CardinalDirection>();
            foreach (var direction in directions)
            {
                if (direction != _currentRotation)
                {
                    availableDirections.Add(direction);
                }
            }

            if (availableDirections.Count > 0)
            {
                CardinalDirection selected = availableDirections[Random.Range(0, availableDirections.Count)];
                if (_showDebugInfo)
                    Debug.Log($"{gameObject.name}: Current rotation: {_currentRotation}, Available: [{string.Join(", ", availableDirections)}], Selected: {selected}");
                return selected;
            }
        }

        CardinalDirection fallback = directions[Random.Range(0, directions.Length)];
        if (_showDebugInfo)
            Debug.Log($"{gameObject.name}: Using fallback selection. Current: {_currentRotation}, Selected: {fallback}");
        return fallback;
    }

    private CardinalDirection DetermineCurrentRotation()
    {
        Vector3 euler = transform.localRotation.eulerAngles;

        // Normalize angles to 0-360
        float y = euler.y;
        while (y < 0) y += 360;
        while (y >= 360) y -= 360;

        // Determine cardinal direction based on Y rotation
        if (y >= 315 || y < 45) return CardinalDirection.North;
        if (y >= 45 && y < 135) return CardinalDirection.East;
        if (y >= 135 && y < 225) return CardinalDirection.South;
        if (y >= 225 && y < 315) return CardinalDirection.West;

        return CardinalDirection.North; // Default
    }

    private Vector3 GetCardinalVector(CardinalDirection direction)
    {
        switch (direction)
        {
            case CardinalDirection.North: return Vector3.forward;
            case CardinalDirection.South: return Vector3.back;
            case CardinalDirection.East: return Vector3.right;
            case CardinalDirection.West: return Vector3.left;
            case CardinalDirection.Up: return Vector3.up;
            case CardinalDirection.Down: return Vector3.down;
            default: return Vector3.zero;
        }
    }

    private Vector3 GetCardinalRotation(CardinalDirection direction)
    {
        switch (direction)
        {
            case CardinalDirection.North: return new Vector3(0, 0, 0);
            case CardinalDirection.South: return new Vector3(0, 180, 0);
            case CardinalDirection.East: return new Vector3(0, 90, 0);
            case CardinalDirection.West: return new Vector3(0, 270, 0);
            case CardinalDirection.Up: return new Vector3(270, 0, 0);
            case CardinalDirection.Down: return new Vector3(90, 0, 0);
            default: return Vector3.zero;
        }
    }

    // Animation coroutines
    private IEnumerator AnimateRotation(Vector3 targetRotation)
    {
        _isAnimating = true;
        Quaternion startRotation = transform.localRotation;
        Quaternion endRotation = Quaternion.Euler(targetRotation);
        float elapsed = 0f;

        if (_showDebugInfo)
            Debug.Log($"{gameObject.name}: Starting rotation animation");

        while (elapsed < _animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / _animationDuration;
            float curveValue = _animationCurve.Evaluate(t);

            transform.localRotation = Quaternion.Lerp(startRotation, endRotation, curveValue);
            yield return null;
        }

        transform.localRotation = endRotation;
        _isAnimating = false;

        if (_showDebugInfo)
            Debug.Log($"{gameObject.name}: Rotation animation complete");
    }

    private IEnumerator AnimatePosition(Vector3 targetPosition)
    {
        _isAnimating = true;
        Vector3 startPosition = transform.localPosition;
        float elapsed = 0f;

        if (_showDebugInfo)
            Debug.Log($"{gameObject.name}: Starting position animation");

        while (elapsed < _animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / _animationDuration;
            float curveValue = _animationCurve.Evaluate(t);

            transform.localPosition = Vector3.Lerp(startPosition, targetPosition, curveValue);
            yield return null;
        }

        transform.localPosition = targetPosition;
        _isAnimating = false;

        if (_showDebugInfo)
            Debug.Log($"{gameObject.name}: Position animation complete");
    }

    private IEnumerator AnimateExtrusion(Vector3 targetPosition, Vector3 targetScale)
    {
        _isAnimating = true;
        Vector3 startPosition = transform.localPosition;
        Vector3 startScale = transform.localScale;
        float elapsed = 0f;

        if (_showDebugInfo)
            Debug.Log($"{gameObject.name}: Starting extrusion animation");

        while (elapsed < _animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / _animationDuration;
            float curveValue = _animationCurve.Evaluate(t);

            transform.localPosition = Vector3.Lerp(startPosition, targetPosition, curveValue);
            transform.localScale = Vector3.Lerp(startScale, targetScale, curveValue);
            yield return null;
        }

        transform.localPosition = targetPosition;
        transform.localScale = targetScale;
        _isAnimating = false;

        if (_showDebugInfo)
            Debug.Log($"{gameObject.name}: Extrusion animation complete");
    }

    private IEnumerator AnimateReset()
    {
        _isAnimating = true;
        Vector3 startPosition = transform.localPosition;
        Vector3 startScale = transform.localScale;
        Quaternion startRotation = transform.localRotation;
        float elapsed = 0f;

        if (_showDebugInfo)
            Debug.Log($"{gameObject.name}: Starting reset animation");

        while (elapsed < _animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / _animationDuration;
            float curveValue = _animationCurve.Evaluate(t);

            transform.localPosition = Vector3.Lerp(startPosition, _originalPosition, curveValue);
            transform.localScale = Vector3.Lerp(startScale, _originalScale, curveValue);
            transform.localRotation = Quaternion.Lerp(startRotation, _originalRotation, curveValue);
            yield return null;
        }

        transform.localPosition = _originalPosition;
        transform.localScale = _originalScale;
        transform.localRotation = _originalRotation;
        _isAnimating = false;

        if (_showDebugInfo)
            Debug.Log($"{gameObject.name}: Reset animation complete");
    }

    // Bounds checking methods
    private Vector3 ClampPositionToBounds(Vector3 position)
    {
        if (!_hasBounds) return position;

        // Convert local position to world position for bounds checking
        Vector3 worldPosition = transform.parent != null ?
            transform.parent.TransformPoint(position) : position;

        // Get the object's bounds at the target world position
        Bounds objectBounds = GetObjectBoundsAtWorldPosition(worldPosition);

        // Clamp the object bounds to stay within animation bounds
        Vector3 clampedCenter = _animationBounds.ClosestPoint(objectBounds.center);

        // Calculate the offset from the object's center to its world position
        Vector3 centerToPosition = worldPosition - objectBounds.center;

        // Apply the offset to the clamped center
        Vector3 clampedWorldPosition = clampedCenter + centerToPosition;

        // Convert back to local position
        Vector3 clampedLocalPosition = transform.parent != null ?
            transform.parent.InverseTransformPoint(clampedWorldPosition) : clampedWorldPosition;

        if (_showDebugInfo && Vector3.Distance(position, clampedLocalPosition) > 0.01f)
        {
            Debug.Log($"{gameObject.name}: Position clamped from {position} to {clampedLocalPosition}");
        }

        return clampedLocalPosition;
    }

    private Vector3 ClampScaleToBounds(Vector3 scale)
    {
        // Enforce minimum scale of 0.1x0.1x0.1 units (allow shrinking)
        Vector3 minScale = Vector3.one * 0.1f;
        Vector3 clampedScale = new Vector3(
            Mathf.Max(scale.x, minScale.x),
            Mathf.Max(scale.y, minScale.y),
            Mathf.Max(scale.z, minScale.z)
        );

        if (!_hasBounds) return clampedScale;

        // Get the object's bounds with the target scale at current world position
        Vector3 worldPosition = transform.parent != null ?
            transform.parent.TransformPoint(transform.localPosition) : transform.position;
        Bounds objectBounds = GetObjectBoundsAtWorldPositionAndScale(worldPosition, clampedScale);

        // Check if the scaled object would fit within bounds
        if (_animationBounds.Contains(objectBounds.min) && _animationBounds.Contains(objectBounds.max))
        {
            return clampedScale; // Scale is fine
        }

        // Calculate maximum allowed scale that fits within bounds
        Vector3 maxScale = _originalScale;

        // Calculate scale ratio component by component
        Vector3 scaleRatio = new Vector3(
            _animationBounds.size.x / objectBounds.size.x,
            _animationBounds.size.y / objectBounds.size.y,
            _animationBounds.size.z / objectBounds.size.z
        );

        // Use the smallest scale ratio to ensure it fits in all dimensions
        float minRatio = Mathf.Min(scaleRatio.x, scaleRatio.y, scaleRatio.z);
        maxScale = _originalScale * minRatio;

        // Clamp the target scale to the maximum allowed scale, but respect minimum scale
        Vector3 finalScale = new Vector3(
            Mathf.Clamp(clampedScale.x, minScale.x, maxScale.x),
            Mathf.Clamp(clampedScale.y, minScale.y, maxScale.y),
            Mathf.Clamp(clampedScale.z, minScale.z, maxScale.z)
        );

        if (_showDebugInfo && Vector3.Distance(scale, finalScale) > 0.01f)
        {
            Debug.Log($"{gameObject.name}: Scale clamped from {scale} to {finalScale}");
        }

        return finalScale;
    }

    private Bounds GetObjectBoundsAtWorldPosition(Vector3 worldPosition)
    {
        // Get the object's renderer bounds
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            Bounds bounds = renderer.bounds;
            bounds.center = worldPosition;
            return bounds;
        }

        // Fallback: use a default bounds based on scale
        Vector3 worldScale = transform.parent != null ?
            transform.parent.TransformVector(_originalScale) : _originalScale;
        return new Bounds(worldPosition, worldScale);
    }

    private Bounds GetObjectBoundsAtWorldPositionAndScale(Vector3 worldPosition, Vector3 localScale)
    {
        // Get the object's renderer bounds
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            Bounds bounds = renderer.bounds;
            bounds.center = worldPosition;

            // Convert local scale to world scale
            Vector3 transformedScale = transform.parent != null ?
                transform.parent.TransformVector(localScale) : localScale;
            bounds.size = transformedScale;
            return bounds;
        }

        // Fallback: use a default bounds based on scale
        Vector3 fallbackScale = transform.parent != null ?
            transform.parent.TransformVector(localScale) : localScale;
        return new Bounds(worldPosition, fallbackScale);
    }

    // Public setters for runtime configuration
    public void SetAnimationDuration(float duration)
    {
        _animationDuration = Mathf.Max(0.1f, duration);
    }

    public void SetAnimationCurve(AnimationCurve curve)
    {
        _animationCurve = curve;
    }

    public void SetMovementDistanceRange(float min, float max)
    {
        _minMovementDistance = Mathf.Max(0f, min);
        _maxMovementDistance = Mathf.Max(_minMovementDistance, max);
    }

    public void SetExtrusionScaleRange(float min, float max)
    {
        _minExtrusionScale = Mathf.Max(0.1f, min); // Allow shrinking down to 0.1x (10% of original size)
        _maxExtrusionScale = Mathf.Max(_minExtrusionScale, max);
    }

    public void SetExcludeCurrentRotation(bool exclude)
    {
        _excludeCurrentRotation = exclude;
    }

    // Getters for current state
    public Vector3 GetOriginalPosition() => _originalPosition;
    public Vector3 GetOriginalScale() => _originalScale;
    public Quaternion GetOriginalRotation() => _originalRotation;
    public CardinalDirection GetCurrentRotation() => _currentRotation;
    public Bounds GetAnimationBounds() => _animationBounds;
    public bool HasBounds() => _hasBounds;
    public GameObject GetBoundsReference() => _boundsReference;
    public bool IsUsingBoundsReference() => _useBoundsReference;

    // Debug method to visualize bounds
    void OnDrawGizmos()
    {
        if (_showDebugInfo)
        {
            // Draw animation bounds
            if (_hasBounds)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(_animationBounds.center, _animationBounds.size);
            }

            // Draw bounds reference object
            if (_useBoundsReference && _boundsReference != null)
            {
                Gizmos.color = Color.blue;
                Bounds refBounds = GetBoundsFromGameObject(_boundsReference);
                Gizmos.DrawWireCube(refBounds.center, refBounds.size);
            }

            // Draw current object bounds
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(renderer.bounds.center, renderer.bounds.size);
            }
        }
    }
}