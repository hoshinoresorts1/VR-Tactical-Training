using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Attach to the player or camera. Press `E` to raycast and start defusal on a Mine.
/// </summary>
public class DefusalStarter : MonoBehaviour
{
    public float maxDistance = 5f;
    public LayerMask interactMask = ~0;
#if ENABLE_INPUT_SYSTEM
    public Key defusalKey = Key.E;
#else
    public KeyCode defusalKey = KeyCode.E;
#endif

    void Update()
    {
        if (IsDefusalPressed())
        {
            TryStartDefusal();
        }
    }

    bool IsDefusalPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current[defusalKey].wasPressedThisFrame;
#else
        return Input.GetKeyDown(defusalKey);
#endif
    }

    void TryStartDefusal()
    {
        Camera cam = Camera.main;
        if (cam == null) cam = GetComponent<Camera>();
        Vector3 origin = cam != null ? cam.transform.position : transform.position;
        Vector3 dir = cam != null ? cam.transform.forward : transform.forward;

        if (Physics.Raycast(origin, dir, out RaycastHit hit, maxDistance, interactMask))
        {
            // Try find a Mine on the hit object or its parents
            var mine = hit.collider.GetComponent<Mine>() ?? hit.collider.GetComponentInParent<Mine>();
            if (mine != null)
            {
                mine.StartDefusal();
                return;
            }

            // Try mine object under MineCell
            var interaction = hit.collider.GetComponent<MineCellInteraction>() ?? hit.collider.GetComponentInParent<MineCellInteraction>();
            if (interaction != null)
            {
                var grid = interaction.GetType().GetField("gridReference", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(interaction) as MineGrid;
                // Not ideal to use reflection; prefer Mine on child
                var cell = interaction.GetType().GetField("cellData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(interaction) as MineCell;
                if (cell != null && cell.mineObject != null)
                {
                    var mineComp = cell.mineObject.GetComponent<Mine>();
                    if (mineComp != null)
                    {
                        mineComp.StartDefusal();
                        return;
                    }
                }
            }
        }
    }
}
