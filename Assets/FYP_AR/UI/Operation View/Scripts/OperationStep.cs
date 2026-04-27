using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class OperationStep
{
    public string stepTitle;
    [TextArea(3, 5)]
    public string instructionText;

    [Header("Associated Animation")]
    public List<TweenConfig> stepAnimationList = new List<TweenConfig>(); // List of tweens to play for this step

    [Header("Associated Highlight Objects")]
    public List<SelectableObject> highlightObjects = new List<SelectableObject>();
}