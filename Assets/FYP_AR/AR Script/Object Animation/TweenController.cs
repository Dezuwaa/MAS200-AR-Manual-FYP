using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TweenController : MonoBehaviour
{
    private Sequence currentSequence;
    private List<Tween> activeInfiniteTweens = new List<Tween>();

    private struct ObjectOriginData
    {
        public Vector3 localPosition;
        public Quaternion localRotation;
        public Vector3 localScale;
        public bool activeSelf; // store original visibility
    }

    private Dictionary<GameObject, ObjectOriginData> originalStates 
        = new Dictionary<GameObject, ObjectOriginData>();

    // ================================================================
    //  PUBLIC API
    // ================================================================

    public void StopAllTweens()
    {
        if (currentSequence != null && currentSequence.IsActive())
            currentSequence.Kill();

        foreach (Tween t in activeInfiniteTweens)
            if (t != null && t.IsActive()) t.Kill();

        activeInfiniteTweens.Clear();
        DOTween.KillAll();
    }

    public void ResetAllToOrigin()
    {
        StopAllTweens();

        foreach (var kvp in originalStates)
        {
            if (kvp.Key == null) continue;

            kvp.Key.transform.localPosition = kvp.Value.localPosition;
            kvp.Key.transform.localRotation = kvp.Value.localRotation;
            kvp.Key.transform.localScale    = kvp.Value.localScale;
            kvp.Key.SetActive(kvp.Value.activeSelf); // restore original visibility
        }
    }

    public Sequence PlayStepTweens(List<TweenConfig> stepTweens)
    {
        StopAllTweens();
        currentSequence = DOTween.Sequence();

        if (stepTweens == null || stepTweens.Count == 0)
            return currentSequence;

        foreach (TweenConfig tweenConfig in stepTweens)
        {
            Tween t = CreateTween(tweenConfig);
            if (t == null) continue;

            if (tweenConfig.loop && tweenConfig.loopCount == -1)
            {
                // Infinite loops tracked separately, never added to sequence
                activeInfiniteTweens.Add(t);
            }
            else
            {
                switch (tweenConfig.playMode)
                {
                    case TweenPlayMode.Append:
                        currentSequence.Append(t);
                        break;
                    case TweenPlayMode.Join:
                    default:
                        currentSequence.Join(t);
                        break;
                }
            }
        }

        return currentSequence;
    }

    // ================================================================
    //  INTERNAL
    // ================================================================

    private Tween CreateTween(TweenConfig tweenConfig)
    {
        GameObject target = tweenConfig.targetGameObject;
        if (target == null) return null;

        // Cache original state the first time we see this object
        if (!originalStates.ContainsKey(target))
        {
            originalStates[target] = new ObjectOriginData
            {
                localPosition = target.transform.localPosition,
                localRotation = target.transform.localRotation,
                localScale    = target.transform.localScale,
                activeSelf    = target.activeSelf
            };
        }

        // Visibility
        if (tweenConfig.setActiveOnPlay)
            target.SetActive(true);
        
        if (tweenConfig.setInactiveOnPlay)
            target.SetActive(false);
        // Snap to start value if requested
        if (tweenConfig.useStartValue)
        {
            switch (tweenConfig.propertyToTween)
            {
                case TweenProperty.Position:
                    target.transform.position = tweenConfig.startValue;
                    break;
                case TweenProperty.LocalPosition:
                    target.transform.localPosition = tweenConfig.startValue;
                    break;
                case TweenProperty.Rotation:
                    target.transform.rotation = Quaternion.Euler(tweenConfig.startValue);
                    break;
                case TweenProperty.LocalRotation:
                    target.transform.localRotation = Quaternion.Euler(tweenConfig.startValue);
                    break;
                case TweenProperty.Scale:
                    target.transform.localScale = tweenConfig.startValue;
                    break;
            }
        }

        // Create tween
        Tween t = null;
        switch (tweenConfig.propertyToTween)
        {
            case TweenProperty.Position:
                t = target.transform.DOMove(tweenConfig.targetValue, tweenConfig.duration)
                    .SetEase(tweenConfig.easeType);
                break;
            case TweenProperty.LocalPosition:
                t = target.transform.DOLocalMove(tweenConfig.targetValue, tweenConfig.duration)
                    .SetEase(tweenConfig.easeType);
                break;
            case TweenProperty.Rotation:
                t = target.transform.DORotate(tweenConfig.targetValue, tweenConfig.duration)
                    .SetEase(tweenConfig.easeType);
                break;
            case TweenProperty.LocalRotation:
                t = target.transform.DOLocalRotate(tweenConfig.targetValue, tweenConfig.duration)
                    .SetEase(tweenConfig.easeType);
                break;
            case TweenProperty.Scale:
                t = target.transform.DOScale(tweenConfig.targetValue, tweenConfig.duration)
                    .SetEase(tweenConfig.easeType);
                break;
        }

        t?.SetSpeedBased(tweenConfig.isSpeedBased);

        if (tweenConfig.loop)
            t?.SetLoops(tweenConfig.loopCount, tweenConfig.loopType);

        return t;
    }
}