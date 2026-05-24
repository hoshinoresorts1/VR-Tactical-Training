using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class MineDetector : MonoBehaviour
{
    [SerializeField] private Transform rayOrigin;
    [SerializeField] private float maxDistance = 10f;
    [SerializeField] private LayerMask cellLayer = ~0;
    [SerializeField] private KeyCode detectKey = KeyCode.Mouse0;

    private void Awake()
    {
        if (rayOrigin == null)
            rayOrigin = transform;
    }

    private void Update()
    {
        if (IsDetectPressed())
        {
            Detect();
        }
    }

    private bool IsDetectPressed()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            return Mouse.current.leftButton.wasPressedThisFrame;
        }
#endif
        try
        {
            return Input.GetKeyDown(detectKey);
        }
        catch (System.InvalidOperationException)
        {
            return false;
        }
    }

    private bool TryGetMousePosition(out Vector2 position)
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            position = Mouse.current.position.ReadValue();
            return true;
        }
#endif
        try
        {
            position = Input.mousePosition;
            return true;
        }
        catch (System.InvalidOperationException)
        {
            position = default;
            return false;
        }
    }

    public void Detect()
    {
        if (rayOrigin == null)
            return;

        Ray ray;
        if (TryGetMousePosition(out Vector2 screenPosition) && Camera.main != null)
        {
            ray = Camera.main.ScreenPointToRay(screenPosition);
        }
        else
        {
            ray = new Ray(rayOrigin.position, rayOrigin.forward);
        }

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, cellLayer))
        {
            MineCellInteraction interaction = hit.collider.GetComponent<MineCellInteraction>() ?? hit.collider.GetComponentInParent<MineCellInteraction>();
            interaction?.SelectCell();
        }

        Debug.DrawRay(ray.origin, ray.direction * maxDistance, Color.yellow, 1f);
    }
}
