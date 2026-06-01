using UnityEngine;

// Keeps the player character GameObject locked to the user's actual head position
// (i.e. the main camera, which tracks the HMD in VR). This makes "the user IS the
// character": when the user walks (room-scale, continuous move, or teleport), the
// character body follows. When the user turns their body, the character turns too.
//
// Notes:
// - Only X/Z are copied from the head target. Y stays at groundY so the character's
//   feet remain on the floor regardless of the user's actual height.
// - Only yaw (rotation around Y) is copied — pitch/roll on the head don't rotate
//   the whole body. Head pitch is the user's own head movement; the character body
//   should stay upright.
// - Runs in LateUpdate so it overrides any animation root motion that frame.
public class PlayerCharacterFollower : MonoBehaviour
{
    [SerializeField] private Transform headTarget;
    [SerializeField] private float groundY = 0f;
    [SerializeField] private bool followYaw = true;

    private void LateUpdate()
    {
        if (headTarget == null)
        {
            return;
        }

        Vector3 headPos = headTarget.position;
        transform.position = new Vector3(headPos.x, groundY, headPos.z);

        if (!followYaw)
        {
            return;
        }

        Vector3 forward = headTarget.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude > 0.0001f)
        {
            transform.rotation = Quaternion.LookRotation(forward.normalized, Vector3.up);
        }
    }

    public void SetTarget(Transform target)
    {
        headTarget = target;
    }
}
