using System.Collections;
using UnityEngine;

// Positions this object in front of the player's camera when the scene starts,
// so a floating menu/panel is always visible and reachable regardless of where
// the player spawns. Faces the player so labels are readable.
public class PlaceInFrontOfPlayerOnStart : MonoBehaviour
{
    public float distance = 2.0f;     // meters in front of the camera
    public float height = 1.1f;       // absolute world height
    public float sideOffset = 0.0f;   // + = to the player's right

    private IEnumerator Start()
    {
        // Wait one frame so the XR rig / camera is positioned.
        yield return null;
        Place();
    }

    public void Place()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 fwd = cam.transform.forward;
        fwd.y = 0f;
        if (fwd.sqrMagnitude < 0.0001f) fwd = Vector3.forward;
        fwd.Normalize();
        Vector3 right = Vector3.Cross(Vector3.up, fwd);

        Vector3 pos = cam.transform.position + fwd * distance + right * sideOffset;
        pos.y = height;
        transform.position = pos;

        Vector3 toCam = cam.transform.position - transform.position;
        toCam.y = 0f;
        if (toCam.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(toCam, Vector3.up);
    }
}
