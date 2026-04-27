using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public enum TweenProperty
{
    Position,
    LocalPosition,
    Rotation,
    LocalRotation,
    Scale,
}

[System.Serializable]
public class TweenConfig
{
    [Header("Target Object")]
    public GameObject targetGameObject;

    [Header("Start State (Optional)")]
    [Tooltip("If true, the object instantly snaps to the Start Value before animating.")]
    public bool useStartValue = false;
    public Vector3 startValue;

    [Header("Tween Settings")]
    public TweenProperty propertyToTween;
    public Vector3 targetValue;

    [Tooltip("Time it takes to complete the animation")]
    public float duration = 1f;
    [Tooltip("If true, duration acts as speed (units/degrees per second)")]
    public bool isSpeedBased = false;

    public Ease easeType = Ease.Linear;

    [Header("Loop Settings")]
    public bool loop = false;
    public LoopType loopType = LoopType.Restart;
    [Tooltip("Number of times to loop. Ignored if loop is false. Set to -1 for infinite loops.")]
    public int loopCount = -1; // -1 for infinite loops
}
