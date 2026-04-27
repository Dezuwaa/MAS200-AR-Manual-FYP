using UnityEngine;

public class SelectableObject : MonoBehaviour
{
    [SerializeField] Color selectedColor;
    private Outline myOutline;

    void Start()
    {
        myOutline = GetComponent<Outline>();
        myOutline.OutlineColor = selectedColor;
        myOutline.enabled = false;
    }

    public void Select()
    {
        myOutline.enabled = true;
    }

    public void Deselect()
    {
        myOutline.enabled = false;
    }
}
