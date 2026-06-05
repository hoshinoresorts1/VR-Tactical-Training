using System.Collections.Generic;
using UnityEngine;

// Shows a celebratory banner when all gear is equipped (training complete).
// Subscribes to GearEquipManager.EquippedStateChanged. The banner GameObject is
// hidden until completion, then pops in with a short scale animation. Text is
// ASCII only to avoid TMP missing-glyph warnings.
public class GearSuccessBanner : MonoBehaviour
{
    [SerializeField] private GearEquipManager manager;
    [SerializeField] private GameObject banner;     // the visual to show on completion
    [SerializeField] private float popScale = 1.2f; // peak scale during the pop
    [SerializeField] private float popSpeed = 6f;

    private bool justCompleted;
    private float t;
    private Vector3 baseScale = Vector3.one;

    private void OnEnable()
    {
        if (manager == null) manager = FindObjectOfType<GearEquipManager>();
        if (manager != null) manager.EquippedStateChanged += OnChanged;
        if (banner != null)
        {
            baseScale = banner.transform.localScale;
            banner.SetActive(false);
        }
    }

    private void OnDisable()
    {
        if (manager != null) manager.EquippedStateChanged -= OnChanged;
    }

    private void OnChanged(IReadOnlyDictionary<GearType, bool> state, bool ready)
    {
        if (banner == null) return;
        if (ready)
        {
            if (!banner.activeSelf)
            {
                banner.SetActive(true);
                justCompleted = true;
                t = 0f;
            }
        }
        else
        {
            banner.SetActive(false);
            justCompleted = false;
        }
    }

    private void Update()
    {
        if (banner == null || !banner.activeSelf || !justCompleted) return;
        t += Time.deltaTime * popSpeed;
        // single ease-out pop then settle
        float k = Mathf.Min(t, Mathf.PI);
        float s = 1f + (popScale - 1f) * Mathf.Sin(k);
        banner.transform.localScale = baseScale * s;
        if (t >= Mathf.PI)
        {
            banner.transform.localScale = baseScale;
            justCompleted = false;
        }
    }
}
