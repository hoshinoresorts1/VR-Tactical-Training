using System.Collections;
using UnityEngine;
using UnityEngine.Events;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class WireDefusal : MonoBehaviour
{
    [Tooltip("Indices of wires that form the correct cut sequence (0-based)")]
    public int[] correctSequence = new int[] { 0, 1, 2 };

    [Tooltip("Randomize the wire cut order whenever defusal starts.")]
    public bool randomizeSequence = true;

    [Tooltip("Time allowed to cut each wire (seconds)")]
    public float timePerWire = 20f;

    public UnityEvent onDefusalSuccess;
    public UnityEvent onDefusalFailed;

    private static readonly Color[] WireColors = new Color[]
    {
        Color.red,
        Color.blue,
        Color.green
    };

    private static readonly string[] WireColorNames = new string[]
    {
        "RED",
        "BLUE",
        "GREEN"
    };

    private int currentStep = 0;
    private float timeRemaining;
    private bool completionQueued;

    private void OnEnable()
    {
        BeginDefusal(timePerWire);
    }

    public void BeginDefusal(float timeLimit)
    {
        timePerWire = timeLimit;

        if (randomizeSequence)
        {
            RandomizeSequence();
        }

        PrepareWires();
        ResetDefusal();
    }

    private void ResetDefusal()
    {
        currentStep = 0;
        timeRemaining = timePerWire;
        completionQueued = false;
    }

    private void Update()
    {
        if (currentStep >= correctSequence.Length) return;

        VRMineHUD.GetOrCreate().SetDefusalInfo(GetSequenceText(), Mathf.CeilToInt(timeRemaining));

        if (WasWireKeyPressed(0)) SelectWire(0);
        if (WasWireKeyPressed(1)) SelectWire(1);
        if (WasWireKeyPressed(2)) SelectWire(2);

        timeRemaining -= Time.deltaTime;
        if (timeRemaining <= 0f)
        {
            Fail();
        }
    }

    private void OnGUI()
    {
        if (!isActiveAndEnabled || currentStep >= correctSequence.Length)
            return;

        GUIStyle style = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 34,
            fontStyle = FontStyle.Bold
        };
        style.normal.textColor = Color.white;

        Rect rect = new Rect(0f, Screen.height * 0.35f, Screen.width, 100f);
        GUI.Label(rect, $"{GetSequenceText()}    TIME: {Mathf.CeilToInt(timeRemaining)}", style);
    }

    private bool WasWireKeyPressed(int wireIndex)
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current == null)
            return false;

        return wireIndex switch
        {
            0 => Keyboard.current.digit1Key.wasPressedThisFrame || Keyboard.current.numpad1Key.wasPressedThisFrame,
            1 => Keyboard.current.digit2Key.wasPressedThisFrame || Keyboard.current.numpad2Key.wasPressedThisFrame,
            2 => Keyboard.current.digit3Key.wasPressedThisFrame || Keyboard.current.numpad3Key.wasPressedThisFrame,
            _ => false
        };
#else
        switch (wireIndex)
        {
            case 0:
                return Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1);
            case 1:
                return Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2);
            case 2:
                return Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3);
            default:
                return false;
        }
#endif
    }

    public void SelectWire(int wireIndex)
    {
        if (currentStep >= correctSequence.Length) return;

        if (wireIndex == correctSequence[currentStep])
        {
            // correct
            XRHapticFeedback.PulseControllers(0.28f, 0.08f);
            ShowWireCut(wireIndex);
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
        if (completionQueued)
            return;

        completionQueued = true;
        VRMineHUD.GetOrCreate().ClearDefusalInfo();
        StartCoroutine(CompleteSuccessAfterDelay());
    }

    private IEnumerator CompleteSuccessAfterDelay()
    {
        yield return new WaitForSeconds(0.35f);
        onDefusalSuccess?.Invoke();
        gameObject.SetActive(false);
    }

    private void Fail()
    {
        VRMineHUD.GetOrCreate().ClearDefusalInfo();
        onDefusalFailed?.Invoke();
        // keep disabled to simulate explosion; scene logic can respawn
        gameObject.SetActive(false);
    }

    private void RandomizeSequence()
    {
        correctSequence = new int[] { 0, 1, 2 };

        for (int i = 0; i < correctSequence.Length; i++)
        {
            int randomIndex = Random.Range(i, correctSequence.Length);
            (correctSequence[i], correctSequence[randomIndex]) = (correctSequence[randomIndex], correctSequence[i]);
        }
    }

    private void PrepareWires()
    {
        WireInteractable[] wires = GetComponentsInChildren<WireInteractable>(true);
        foreach (WireInteractable wire in wires)
        {
            int colorIndex = Mathf.Clamp(wire.wireIndex, 0, WireColors.Length - 1);
            Renderer renderer = wire.GetComponent<Renderer>();
            if (renderer == null)
                continue;

            Shader shader = FindVisibleShader();
            if (shader == null)
                continue;

            Material material = new Material(shader);
            material.color = WireColors[colorIndex];
            renderer.material = material;

            wire.ConfigureSurfaceLayout();
        }
    }

    private void ShowWireCut(int wireIndex)
    {
        WireInteractable[] wires = GetComponentsInChildren<WireInteractable>(true);
        foreach (WireInteractable wire in wires)
        {
            if (wire.wireIndex == wireIndex)
            {
                wire.ShowCutVisual();
                return;
            }
        }
    }

    private Shader FindVisibleShader()
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader != null)
            return shader;

        shader = Shader.Find("Standard");
        if (shader != null)
            return shader;

        return Shader.Find("Sprites/Default");
    }

    private string GetSequenceText()
    {
        string[] parts = new string[correctSequence.Length];
        for (int i = 0; i < correctSequence.Length; i++)
        {
            int wireIndex = correctSequence[i];
            string colorName = WireColorNames[Mathf.Clamp(wireIndex, 0, WireColorNames.Length - 1)];
            parts[i] = colorName;
        }

        return string.Join(" > ", parts);
    }
}
