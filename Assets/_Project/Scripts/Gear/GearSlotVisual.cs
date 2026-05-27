using System.Collections;
using UnityEngine;

// Controls the slot's visual guide and equip-success flash feedback.
// Attach to a GearSlot GameObject; assign a child Renderer (e.g. a translucent box / outline mesh)
// that represents the slot guide.
public class GearSlotVisual : MonoBehaviour
{
    [SerializeField] private Renderer guideRenderer;
    [SerializeField] private Color idleColor = new Color(0f, 0.6f, 1f, 0.35f);
    [SerializeField] private Color equippedFlashColor = new Color(0f, 1f, 0.3f, 0.85f);
    [SerializeField] private float flashDuration = 0.5f;

    private static readonly int BaseColorProperty = Shader.PropertyToID("_BaseColor");
    private static readonly int LegacyColorProperty = Shader.PropertyToID("_Color");

    private MaterialPropertyBlock propertyBlock;
    private Coroutine flashRoutine;

    private void Awake()
    {
        if (guideRenderer == null)
        {
            guideRenderer = GetComponentInChildren<Renderer>(true);
        }

        propertyBlock = new MaterialPropertyBlock();
    }

    public void ShowIdle()
    {
        if (guideRenderer == null)
        {
            return;
        }

        StopFlash();
        guideRenderer.enabled = true;
        ApplyColor(idleColor);
    }

    public void Hide()
    {
        if (guideRenderer == null)
        {
            return;
        }

        StopFlash();
        guideRenderer.enabled = false;
    }

    public void FlashEquipped()
    {
        if (guideRenderer == null)
        {
            return;
        }

        StopFlash();
        flashRoutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        guideRenderer.enabled = true;
        ApplyColor(equippedFlashColor);
        yield return new WaitForSeconds(flashDuration);
        guideRenderer.enabled = false;
        flashRoutine = null;
    }

    private void StopFlash()
    {
        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
            flashRoutine = null;
        }
    }

    private void ApplyColor(Color color)
    {
        if (guideRenderer == null)
        {
            return;
        }

        guideRenderer.GetPropertyBlock(propertyBlock);
        // Cover both URP Lit (_BaseColor) and Built-in/legacy (_Color) shaders so the user
        // can pick whichever material fits their pipeline.
        propertyBlock.SetColor(BaseColorProperty, color);
        propertyBlock.SetColor(LegacyColorProperty, color);
        guideRenderer.SetPropertyBlock(propertyBlock);
    }
}
