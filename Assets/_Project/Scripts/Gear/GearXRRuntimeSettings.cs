using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

namespace VRTacticalTraining.Gear
{
    public class GearXRRuntimeSettings : MonoBehaviour
    {
        [SerializeField] private float runtimeCameraOffsetY = -0.2f;
        [SerializeField] private float moveSpeed = 1.2f;
        [SerializeField] private bool invertLeftMoveY = true;

        private Transform cameraOffset;
        private DynamicMoveProvider moveProvider;
        private InputAction leftMoveAction;
        private int leftMoveBindingIndex = -1;
        private InputBinding originalBindingOverride;

        private void Awake()
        {
            cameraOffset = transform.Find("Camera Offset");
            moveProvider = GetComponentInChildren<DynamicMoveProvider>(true);

            if (moveProvider == null)
                return;

            moveProvider.moveSpeed = moveSpeed;
            ConfigureLeftMoveInput();
        }

        private void LateUpdate()
        {
            if (cameraOffset == null)
                return;

            Vector3 position = cameraOffset.localPosition;
            position.y = runtimeCameraOffsetY;
            cameraOffset.localPosition = position;
        }

        private void OnDestroy()
        {
            if (leftMoveAction == null || leftMoveBindingIndex < 0)
                return;

            ApplyBindingOverride(leftMoveAction, leftMoveBindingIndex, originalBindingOverride);
        }

        private void ConfigureLeftMoveInput()
        {
            InputActionReference actionReference = moveProvider.leftHandMoveInput.inputActionReference;
            if (actionReference == null || actionReference.action == null)
                return;

            leftMoveAction = actionReference.action;
            for (int i = 0; i < leftMoveAction.bindings.Count; i++)
            {
                InputBinding binding = leftMoveAction.bindings[i];
                string path = binding.effectivePath;
                if (string.IsNullOrEmpty(path) ||
                    !path.Contains("{LeftHand}") ||
                    !path.Contains("Primary2DAxis"))
                    continue;

                leftMoveBindingIndex = i;
                originalBindingOverride = new InputBinding
                {
                    overridePath = binding.overridePath,
                    overrideInteractions = binding.overrideInteractions,
                    overrideProcessors = binding.overrideProcessors
                };

                if (invertLeftMoveY)
                {
                    string processors = string.IsNullOrWhiteSpace(binding.processors)
                        ? "InvertVector2(invertX=false,invertY=true)"
                        : binding.processors + ",InvertVector2(invertX=false,invertY=true)";

                    ApplyBindingOverride(leftMoveAction, leftMoveBindingIndex, new InputBinding
                    {
                        overrideProcessors = processors
                    });
                }

                break;
            }
        }

        private static void ApplyBindingOverride(InputAction action, int bindingIndex, InputBinding bindingOverride)
        {
            bool wasEnabled = action.enabled;
            if (wasEnabled)
                action.Disable();

            action.ApplyBindingOverride(bindingIndex, bindingOverride);

            if (wasEnabled)
                action.Enable();
        }
    }
}
