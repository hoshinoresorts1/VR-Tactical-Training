using UnityEngine;

public class Mine : MonoBehaviour
{
    [Tooltip("Prefab of the defusal UI/object (WireDefusal) to spawn when starting defusal")]
    public GameObject defusalPrefab;

    [Tooltip("Local spawn offset for the defusal UI")]
    public Vector3 defusalOffset = new Vector3(0f, 0.3f, 0f);

    private void Awake()
    {
        gameObject.tag = "Mine";
    }

    public void StartDefusal()
    {
        if (defusalPrefab == null)
        {
            Debug.LogWarning("Defusal prefab not assigned on Mine.");
            return;
        }

        Vector3 spawnPos = transform.position + defusalOffset;
        Instantiate(defusalPrefab, spawnPos, Quaternion.identity);
    }
}
