using UnityEngine;

public class WireInteractable : MonoBehaviour
{
    public int wireIndex = 0;
    public WireDefusal wireDefusal;

    // Simple editor/test interaction
    private void OnMouseDown()
    {
        if (wireDefusal != null)
            wireDefusal.SelectWire(wireIndex);
    }
}
