using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class VRMineHUD : MonoBehaviour
{
    public static VRMineHUD Instance { get; private set; }

    private Text statusText;
    private Text detectionText;
    private Text defusalText;
    private GameObject mainHallButton;
    private float statusUntil;
    private float detectionUntil;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureHUDOnSceneLoad()
    {
        if (SceneManager.GetActiveScene().name == "MineMap")
            GetOrCreate();
    }

    public static VRMineHUD GetOrCreate()
    {
        if (Instance != null)
            return Instance;

        GameObject hudObject = new GameObject("VRMineHUD");
        return hudObject.AddComponent<VRMineHUD>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        BuildHUD();
    }

    private void Update()
    {
        if (transform.parent == null && Camera.main != null)
        {
            transform.SetParent(Camera.main.transform, false);
            transform.localPosition = new Vector3(0f, -0.08f, 1.6f);
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one * 0.0015f;
        }

        if (statusText != null && Time.time > statusUntil)
            statusText.text = "";

        if (detectionText != null && Time.time > detectionUntil)
            detectionText.text = "";
    }

    public void SetLives(int lives)
    {
    }

    public void ShowHit(float duration)
    {
        ShowStatus("MINE HIT!", Color.yellow, duration);
    }

    public void ShowDetection(float duration)
    {
        if (detectionText == null)
            return;

        detectionText.text = "MINE DETECTED";
        detectionUntil = Time.time + duration;
    }

    public void SetDefusalInfo(string sequence, int seconds)
    {
        if (defusalText != null)
            defusalText.text = sequence + "\nTIME: " + seconds;
    }

    public void ClearDefusalInfo()
    {
        if (defusalText != null)
            defusalText.text = "";
    }

    private void ShowStatus(string message, Color color, float duration)
    {
        if (statusText == null)
            return;

        statusText.text = message;
        statusText.color = color;
        statusUntil = float.IsPositiveInfinity(duration) ? duration : Time.time + duration;
    }

    private void BuildHUD()
    {
        Transform cameraTransform = Camera.main != null ? Camera.main.transform : null;

        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;

        RectTransform rect = canvas.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(720f, 420f);

        transform.SetParent(cameraTransform, false);
        transform.localPosition = new Vector3(0f, -0.08f, 1.6f);
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one * 0.0015f;

        statusText = CreateText("Status", new Vector2(0f, 65f), new Vector2(660f, 90f), 54, TextAnchor.MiddleCenter);
        detectionText = CreateText("Detection", new Vector2(0f, 5f), new Vector2(660f, 60f), 34, TextAnchor.MiddleCenter);
        detectionText.color = Color.yellow;
        defusalText = CreateText("Defusal", new Vector2(0f, -85f), new Vector2(660f, 100f), 34, TextAnchor.MiddleCenter);

        CreateMainHallButton();
        SetMainHallVisible(false);
    }

    private Text CreateText(string objectName, Vector2 position, Vector2 size, int fontSize, TextAnchor alignment)
    {
        GameObject textObject = new GameObject(objectName);
        textObject.transform.SetParent(transform, false);

        RectTransform rect = textObject.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Text text = textObject.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = Color.white;
        text.raycastTarget = false;
        return text;
    }

    private void CreateMainHallButton()
    {
        mainHallButton = GameObject.CreatePrimitive(PrimitiveType.Cube);
        mainHallButton.name = "MainHallButton";
        mainHallButton.transform.SetParent(transform, false);
        mainHallButton.transform.localPosition = new Vector3(0f, -155f, 0f);
        mainHallButton.transform.localScale = new Vector3(260f, 62f, 8f);

        Renderer renderer = mainHallButton.GetComponent<Renderer>();
        if (renderer != null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
                shader = Shader.Find("Standard");

            if (shader != null)
                renderer.material = new Material(shader) { color = new Color(0.12f, 0.34f, 0.22f) };
        }

        XRSimpleInteractable interactable = mainHallButton.AddComponent<XRSimpleInteractable>();
        interactable.selectEntered.AddListener(OnMainHallSelected);

        VRMainHallButton clickHandler = mainHallButton.AddComponent<VRMainHallButton>();
        clickHandler.returnAction = ReturnToMainHall;

        Text label = CreateText("MainHallLabel", new Vector2(0f, -155f), new Vector2(260f, 62f), 28, TextAnchor.MiddleCenter);
        label.text = "MAIN HALL";
        label.transform.SetAsLastSibling();
    }

    private void OnMainHallSelected(SelectEnterEventArgs args)
    {
        ReturnToMainHall();
    }

    private void SetMainHallVisible(bool visible)
    {
        if (mainHallButton != null)
            mainHallButton.SetActive(visible);

        Transform label = transform.Find("MainHallLabel");
        if (label != null)
            label.gameObject.SetActive(visible);
    }

    private void ReturnToMainHall()
    {
        TrainingSceneReturn.GetOrCreate().ReturnToMainHall();
    }

}

public class VRMainHallButton : MonoBehaviour
{
    public Action returnAction;

    private void OnMouseDown()
    {
        returnAction?.Invoke();
    }
}
