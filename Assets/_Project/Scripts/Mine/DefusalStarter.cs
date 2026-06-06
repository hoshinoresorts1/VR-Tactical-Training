using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Attach to the player or camera. Aim the left controller ray at a mine and hold the left trigger to defuse it.
/// </summary>
public class DefusalStarter : MonoBehaviour
{
    public float maxDistance = 5f;
    public float holdSeconds = 3f;
    public LayerMask interactMask = ~0;
    private Mine heldMine;
    private float holdTimer;
#if ENABLE_INPUT_SYSTEM
    private InputAction startDefusalAction;
#endif

#if ENABLE_INPUT_SYSTEM
    private void OnEnable()
    {
        SetupStartDefusalAction();
    }

    private void OnDisable()
    {
        startDefusalAction?.Dispose();
        startDefusalAction = null;
    }
#endif

    void Update()
    {
        Mine aimedMine = GetAimedMine();
        if (aimedMine != null && IsDefusalHeld())
        {
            if (heldMine != aimedMine)
            {
                heldMine = aimedMine;
                holdTimer = 0f;
            }

            holdTimer += Time.deltaTime;
            VRMineHUD.GetOrCreate().SetDefusalInfo("DEFUSING", Mathf.CeilToInt(Mathf.Max(0f, holdSeconds - holdTimer)));

            if (holdTimer >= holdSeconds)
            {
                heldMine.StartDefusal();
                ResetHold();
            }

            return;
        }

        ResetHold();
    }

    bool IsDefusalHeld()
    {
#if ENABLE_INPUT_SYSTEM
        return startDefusalAction != null && startDefusalAction.IsPressed();
#else
        return false;
#endif
    }

#if ENABLE_INPUT_SYSTEM
    private void SetupStartDefusalAction()
    {
        if (startDefusalAction != null)
            return;

        startDefusalAction = new InputAction("StartMineDefusalFromRay", InputActionType.Button);
        startDefusalAction.AddBinding("<XRController>{LeftHand}/triggerButton");
        startDefusalAction.AddBinding("<XRController>{LeftHand}/trigger");
        startDefusalAction.Enable();
    }
#endif

    private Mine GetAimedMine()
    {
        Transform leftController = FindLeftControllerTransform();
        if (leftController == null)
            return null;

        if (Physics.Raycast(leftController.position, leftController.forward, out RaycastHit hit, maxDistance, interactMask))
        {
            var mine = hit.collider.GetComponent<Mine>() ?? hit.collider.GetComponentInParent<Mine>();
            if (mine != null)
                return mine;

            var interaction = hit.collider.GetComponent<MineCellInteraction>() ?? hit.collider.GetComponentInParent<MineCellInteraction>();
            if (interaction != null)
            {
                var cell = interaction.GetType().GetField("cellData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(interaction) as MineCell;
                if (cell != null && cell.mineObject != null)
                    return cell.mineObject.GetComponent<Mine>();
            }
        }

        return null;
    }

    private void ResetHold()
    {
        if (heldMine != null)
            VRMineHUD.GetOrCreate().ClearDefusalInfo();

        heldMine = null;
        holdTimer = 0f;
    }

    private Transform FindLeftControllerTransform()
    {
        string[] names =
        {
            "Left Controller",
            "LeftHand Controller",
            "LeftHand Controller Stabilized",
            "Left Controller Stabilized"
        };

        foreach (string controllerName in names)
        {
            GameObject controller = GameObject.Find(controllerName);
            if (controller != null)
                return controller.transform;
        }

        var rayInteractors = FindObjectsOfType<UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor>(true);
        foreach (var rayInteractor in rayInteractors)
        {
            if (rayInteractor != null && rayInteractor.name.ToLowerInvariant().Contains("left"))
                return rayInteractor.transform;
        }

        return null;
    }
}
