using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class TrainingScenePortal : MonoBehaviour
{
    [Tooltip("Scene name to load when the player enters or selects this portal.")]
    public string targetSceneName;

    [Tooltip("Allow trigger enter with the Player tag to load the scene.")]
    public bool loadOnPlayerEnter = true;

    [Tooltip("Allow XR Ray selection to load the scene.")]
    public bool loadOnXRSelect = true;

    private XRSimpleInteractable xrInteractable;

    private void Awake()
    {
        EnsureSetup();
    }

    private void OnEnable()
    {
        EnsureSetup();

        if (xrInteractable != null)
            xrInteractable.selectEntered.AddListener(OnXRSelectEntered);
    }

    private void OnDisable()
    {
        if (xrInteractable != null)
            xrInteractable.selectEntered.RemoveListener(OnXRSelectEntered);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!loadOnPlayerEnter || !other.CompareTag("Player"))
            return;

        LoadTargetScene();
    }

    public void LoadTargetScene()
    {
        if (string.IsNullOrWhiteSpace(targetSceneName))
        {
            Debug.LogWarning("TrainingScenePortal targetSceneName is empty.", this);
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(targetSceneName))
        {
            Debug.LogWarning("Scene is not in Build Settings or does not exist: " + targetSceneName, this);
            return;
        }

        SceneManager.LoadScene(targetSceneName);
    }

    private void OnXRSelectEntered(SelectEnterEventArgs args)
    {
        if (loadOnXRSelect)
            LoadTargetScene();
    }

    private void EnsureSetup()
    {
        Collider collider = GetComponent<Collider>();
        if (collider == null)
        {
            BoxCollider box = gameObject.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = Vector3.one * 1.5f;
        }
        else
        {
            collider.isTrigger = true;
        }

        xrInteractable = GetComponent<XRSimpleInteractable>();
        if (xrInteractable == null)
            xrInteractable = gameObject.AddComponent<XRSimpleInteractable>();
    }
}
