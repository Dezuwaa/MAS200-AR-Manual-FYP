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

public enum TweenPlayMode
{
    Join,   // Plays simultaneously with other tweens in the sequence
    Append, // Plays after the previous tween finishes
}

[System.Serializable]
public class TweenConfig
{
    [Header("Target Object")]
    public GameObject targetGameObject;

    [Header("Visibility")]
    [Tooltip("Makes the object visible before the tween plays.")]
    public bool setActiveOnPlay = false;

    [Tooltip("Makes the object invisible before the tween plays.")]
    public bool setInactiveOnPlay = false;


    [Header("Start State (Optional)")]
    [Tooltip("If true, the object instantly snaps to the Start Value before animating.")]
    public bool useStartValue = false;
    public Vector3 startValue;

    [Header("Tween Settings")]
    public TweenPlayMode playMode = TweenPlayMode.Join;
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
    [Tooltip("Number of times to loop. Set to -1 for infinite.")]
    public int loopCount = -1;
}