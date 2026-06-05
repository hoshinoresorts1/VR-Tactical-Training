using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public static class XRHapticFeedback
{
    public static void PulseControllers(float amplitude, float duration)
    {
        List<InputDevice> controllers = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Controller, controllers);

        foreach (InputDevice controller in controllers)
        {
            HapticCapabilities capabilities;
            if (!controller.TryGetHapticCapabilities(out capabilities) || !capabilities.supportsImpulse)
                continue;

            controller.SendHapticImpulse(0u, Mathf.Clamp01(amplitude), Mathf.Max(0.01f, duration));
        }
    }
}
