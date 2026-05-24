using UnityEngine;
using UnityEngine.Events;

public class WireDefusal : MonoBehaviour
{
    [Tooltip("Indices of wires that form the correct cut sequence (0-based)")]
    public int[] correctSequence = new int[] { 0, 1, 2 };

    [Tooltip("Time allowed to cut each wire (seconds)")]
    public float timePerWire = 20f;

    public UnityEvent onDefusalSuccess;
    public UnityEvent onDefusalFailed;

    private int currentStep = 0;
    private float timeRemaining;

    private void OnEnable()
    {
        ResetDefusal();
    }

    private void ResetDefusal()
    {
        currentStep = 0;
        timeRemaining = timePerWire;
    }

    private void Update()
    {
        if (currentStep >= correctSequence.Length) return;
        timeRemaining -= Time.deltaTime;
        if (timeRemaining <= 0f)
        {
            Fail();
        }
    }

    public void SelectWire(int wireIndex)
    {
        if (currentStep >= correctSequence.Length) return;

        if (wireIndex == correctSequence[currentStep])
        {
            // correct
            currentStep++;
            timeRemaining = timePerWire;
            if (currentStep >= correctSequence.Length)
            {
                Success();
            }
        }
        else
        {
            Fail();
        }
    }

    private void Success()
    {
        onDefusalSuccess?.Invoke();
        gameObject.SetActive(false);
    }

    private void Fail()
    {
        onDefusalFailed?.Invoke();
        // keep disabled to simulate explosion; scene logic can respawn
        gameObject.SetActive(false);
    }
}
