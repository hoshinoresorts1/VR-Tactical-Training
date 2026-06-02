using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace VRTacticalTraining.Gear
{
    /// <summary>
    /// URP-compatible planar mirror. Renders a reflection of the scene from a
    /// mirrored virtual camera into a RenderTexture and feeds it to this object's
    /// material. Works in the Universal Render Pipeline by hooking
    /// RenderPipelineManager.beginCameraRendering (OnWillRenderObject is not
    /// called under SRP).
    ///
    /// Attach to a Quad whose front face is the reflective surface. The mirror
    /// plane is defined by transform.position and the surface normal
    /// (transform.forward by default; toggle flipNormal if the reflection is
    /// rendered on the wrong side).
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(Renderer))]
    public class PlanarMirror : MonoBehaviour
    {
        [Header("Render Texture")]
        [Tooltip("Square size of the reflection RenderTexture.")]
        public int textureSize = 1024;

        [Tooltip("Layers the mirror camera renders. Set to PlayerCharacter only " +
                 "so the mirror shows just the user's avatar and gear.")]
        public LayerMask reflectLayers = ~0;

        [Header("Mirror Plane")]
        [Tooltip("Small offset to push the clip plane to avoid surface artifacts.")]
        public float clipPlaneOffset = 0.02f;

        [Tooltip("Flip the surface normal if the reflection appears on the back side.")]
        public bool flipNormal = false;

        [Header("Material Binding")]
        [Tooltip("Shader texture property to receive the reflection. URP Lit/Unlit use _BaseMap.")]
        public string texturePropertyName = "_BaseMap";

        private Camera _reflectionCamera;
        private RenderTexture _reflectionTexture;
        private Renderer _renderer;
        private MaterialPropertyBlock _mpb;
        private int _texturePropertyId;
        private bool _isRendering;

        private void OnEnable()
        {
            _renderer = GetComponent<Renderer>();
            _mpb = new MaterialPropertyBlock();
            _texturePropertyId = Shader.PropertyToID(texturePropertyName);
            RenderPipelineManager.beginCameraRendering += OnBeginCamera;
        }

        private void OnDisable()
        {
            RenderPipelineManager.beginCameraRendering -= OnBeginCamera;
            Cleanup();
        }

        private void Cleanup()
        {
            if (_reflectionCamera != null)
            {
                _reflectionCamera.targetTexture = null;
                if (Application.isPlaying)
                    Destroy(_reflectionCamera.gameObject);
                else
                    DestroyImmediate(_reflectionCamera.gameObject);
                _reflectionCamera = null;
            }

            if (_reflectionTexture != null)
            {
                if (Application.isPlaying)
                    Destroy(_reflectionTexture);
                else
                    DestroyImmediate(_reflectionTexture);
                _reflectionTexture = null;
            }
        }

        private void OnBeginCamera(ScriptableRenderContext context, Camera cam)
        {
            // Avoid recursion and skip non-render cameras.
            if (_isRendering) return;
            if (cam == _reflectionCamera) return;
            if (cam.cameraType == CameraType.Reflection || cam.cameraType == CameraType.Preview) return;
            if (_renderer == null || !_renderer.enabled || !_renderer.gameObject.activeInHierarchy) return;

            EnsureResources();

            // Mirror plane in world space.
            Vector3 pos = transform.position;
            Vector3 normal = flipNormal ? -transform.forward : transform.forward;

            // Reflect the source camera across the mirror plane.
            float d = -Vector3.Dot(normal, pos) - clipPlaneOffset;
            Vector4 plane = new Vector4(normal.x, normal.y, normal.z, d);
            Matrix4x4 reflection = CalculateReflectionMatrix(plane);

            // Copy relevant camera settings.
            _reflectionCamera.CopyFrom(cam);
            _reflectionCamera.cullingMask = reflectLayers;
            _reflectionCamera.targetTexture = _reflectionTexture;
            _reflectionCamera.enabled = false;
            _reflectionCamera.worldToCameraMatrix = cam.worldToCameraMatrix * reflection;

            // Oblique near clip plane so nothing behind the mirror is drawn.
            Vector4 clipPlane = CameraSpacePlane(_reflectionCamera, pos, normal, 1.0f);
            _reflectionCamera.projectionMatrix = cam.CalculateObliqueMatrix(clipPlane);

            // Reflection flips winding order.
            GL.invertCulling = true;
            _isRendering = true;
#pragma warning disable 0618
            UniversalRenderPipeline.RenderSingleCamera(context, _reflectionCamera);
#pragma warning restore 0618
            _isRendering = false;
            GL.invertCulling = false;

            // Bind the result to the mirror surface.
            _renderer.GetPropertyBlock(_mpb);
            _mpb.SetTexture(_texturePropertyId, _reflectionTexture);
            _renderer.SetPropertyBlock(_mpb);
        }

        private void EnsureResources()
        {
            if (_reflectionTexture == null ||
                _reflectionTexture.width != textureSize)
            {
                if (_reflectionTexture != null)
                {
                    _reflectionTexture.Release();
                    if (Application.isPlaying) Destroy(_reflectionTexture);
                    else DestroyImmediate(_reflectionTexture);
                }

                _reflectionTexture = new RenderTexture(textureSize, textureSize, 24)
                {
                    name = "PlanarMirrorRT_" + GetInstanceID(),
                    antiAliasing = 1
                };
                _reflectionTexture.Create();
            }

            if (_reflectionCamera == null)
            {
                GameObject go = new GameObject("PlanarMirror Camera (" + name + ")");
                go.hideFlags = HideFlags.HideAndDontSave;
                _reflectionCamera = go.AddComponent<Camera>();
                _reflectionCamera.enabled = false;

                // Mirror cameras should not add their own URP extras.
                var data = go.AddComponent<UniversalAdditionalCameraData>();
                data.renderShadows = true;
                data.requiresColorOption = CameraOverrideOption.Off;
                data.requiresDepthOption = CameraOverrideOption.Off;
            }
        }

        private static Matrix4x4 CalculateReflectionMatrix(Vector4 plane)
        {
            Matrix4x4 m = Matrix4x4.identity;
            m.m00 = 1f - 2f * plane.x * plane.x;
            m.m01 = -2f * plane.x * plane.y;
            m.m02 = -2f * plane.x * plane.z;
            m.m03 = -2f * plane.x * plane.w;

            m.m10 = -2f * plane.y * plane.x;
            m.m11 = 1f - 2f * plane.y * plane.y;
            m.m12 = -2f * plane.y * plane.z;
            m.m13 = -2f * plane.y * plane.w;

            m.m20 = -2f * plane.z * plane.x;
            m.m21 = -2f * plane.z * plane.y;
            m.m22 = 1f - 2f * plane.z * plane.z;
            m.m23 = -2f * plane.z * plane.w;

            m.m30 = 0f;
            m.m31 = 0f;
            m.m32 = 0f;
            m.m33 = 1f;
            return m;
        }

        private Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign)
        {
            Vector3 offsetPos = pos + normal * clipPlaneOffset;
            Matrix4x4 m = cam.worldToCameraMatrix;
            Vector3 cpos = m.MultiplyPoint(offsetPos);
            Vector3 cnormal = m.MultiplyVector(normal).normalized * sideSign;
            return new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal));
        }
    }
}
