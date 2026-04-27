using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TweenController : MonoBehaviour
{
    private Sequence currentSequence;
    private List<Tween> activeInfiniteTweens = new List<Tween>(); // Tracks visual cues (infinite loops)

    // Data strcture to hold the original state
    private struct ObjectOriginData
    {
        public Vector3 localPosition;
        public Quaternion localRotation;
        public Vector3 localScale;
    }

    // Dictionary to store original states of objects before tweening, keyed by GameObject
    private Dictionary<GameObject, ObjectOriginData> originalStates = new Dictionary<GameObject, ObjectOriginData>();

    public void StopAllTweens()
    {
        // Kill the main sequence
        if (currentSequence != null && currentSequence.IsActive())
        {
            currentSequence.Kill();
        }

        // Kill any lingering infinite visual cues
        foreach (Tween t in activeInfiniteTweens)
        {
            if (t != null && t.IsActive()) t.Kill();
        }
        activeInfiniteTweens.Clear();

        DOTween.KillAll();
    }

    // Method to snap everything back to original state (used when exiting AR view or resetting)
    public void ResetAllToOrigin()
    {
        StopAllTweens(); // Always stop animations before forcing positions

        foreach (var kvp in originalStates)
        {
            if (kvp.Key != null) // Ensure the GameObject hasn't been destroyed
            {
                kvp.Key.transform.localPosition = kvp.Value.localPosition;
                kvp.Key.transform.localRotation = kvp.Value.localRotation;
                kvp.Key.transform.localScale = kvp.Value.localScale;
            }
        }
    }

    public Sequence PlayStepTweens(List<TweenConfig> stepTweens)
    {
        StopAllTweens();

        currentSequence = DOTween.Sequence();

        if (stepTweens == null || stepTweens.Count == 0)
            return currentSequence;
        
        foreach (TweenConfig tween in stepTweens)
        {
            Tween t = CreateTween(tween);
            
            if (t != null)
            {
                // If it's an infinite loop (visual cue), don't add to sequence
                if (tween.loop && tween.loopCount == -1)
                {
                    activeInfiniteTweens.Add(t);
                }
                else
                {
                    // Finite animation (actuators), add to sequence
                    currentSequence.Join(t);
                }
            }  
        }

        return currentSequence;
    }

    private Tween CreateTween(TweenConfig tweenConfig)
    {
        Tween currentTween = null;
        GameObject targetObject = tweenConfig.targetGameObject;

        // Cache the original state the first time we see this object
        if (!originalStates.ContainsKey(targetObject))
        {
            originalStates[targetObject] = new ObjectOriginData
            {
                localPosition = targetObject.transform.localPosition,
                localRotation = targetObject.transform.localRotation,
                localScale = targetObject.transform.localScale
            };
        }
        
        // 1. STATE FORCING: Snap to start position before tweening if requested
        if (tweenConfig.useStartValue)
        {
            switch (tweenConfig.propertyToTween)
            {
                case TweenProperty.Position:
                    targetObject.transform.position = tweenConfig.startValue;
                    break;
                case TweenProperty.LocalPosition:
                    targetObject.transform.localPosition = tweenConfig.startValue;
                    break;
                case TweenProperty.Rotation:
                    targetObject.transform.rotation = Quaternion.Euler(tweenConfig.startValue);
                    break;
                case TweenProperty.LocalRotation:
                    targetObject.transform.localRotation = Quaternion.Euler(tweenConfig.startValue);
                    break;
                case TweenProperty.Scale:
                    targetObject.transform.localScale = tweenConfig.startValue;
                    break;
            }
        }

        // 2. CREATE TWEEN
        switch (tweenConfig.propertyToTween)
        {
            case TweenProperty.Position:
                currentTween = targetObject.transform.DOMove(tweenConfig.targetValue, tweenConfig.duration).SetEase(tweenConfig.easeType);
                break;
            case TweenProperty.LocalPosition:
                currentTween = targetObject.transform.DOLocalMove(tweenConfig.targetValue, tweenConfig.duration).SetEase(tweenConfig.easeType);
                break;
            case TweenProperty.Rotation:
                currentTween = targetObject.transform.DORotate(tweenConfig.targetValue, tweenConfig.duration).SetEase(tweenConfig.easeType);
                break;
            case TweenProperty.LocalRotation:
                currentTween = targetObject.transform.DOLocalRotate(tweenConfig.targetValue, tweenConfig.duration).SetEase(tweenConfig.easeType);
                break;
            case TweenProperty.Scale:
                currentTween = targetObject.transform.DOScale(tweenConfig.targetValue, tweenConfig.duration).SetEase(tweenConfig.easeType);
                break;
        }

        // 3. APPLY SETTINGS
        currentTween?.SetSpeedBased(tweenConfig.isSpeedBased);
        
        // Apply Loop settings if enabled
        if (tweenConfig.loop)
        {
            currentTween?.SetLoops(tweenConfig.loopCount, tweenConfig.loopType);
        }
        
        return currentTween;
    }
}