using DG.Tweening;
using UnityEngine;

namespace CGJ2026
{
    /// <summary>
    /// Smoothly follows the midpoint of both players. A <see cref="CameraFocusZone"/> can
    /// temporarily override the focus point and zoom, and <see cref="Shake"/> adds screen shake.
    /// </summary>
    public class CameraSetup : MonoBehaviour
    {
        const string ShakeTweenId = "cam_shake";
        static readonly Vector3 CameraDepth = Vector3.back * 10f;

        [SerializeField] Transform playerA;
        [SerializeField] Transform playerB;
        [SerializeField] float smoothTime = 0.3f;
        [SerializeField] float orthoSize = 6f;

        static Vector3 shakeOffset;
        static Object focusOwner;
        static Vector2 focusPoint;
        static float? focusSize;

        Camera cam;
        Vector3 smoothPos;
        Vector3 velocity;
        float currentSize;
        float sizeVelocity;

        public static void Shake(float intensity, float duration)
        {
            DOTween.Kill(ShakeTweenId);
            DOTween.To(() => intensity, v => shakeOffset = (Vector3)(Random.insideUnitCircle * v), 0f, duration)
                .SetId(ShakeTweenId)
                .SetUpdate(true)
                .OnComplete(() => shakeOffset = Vector3.zero);
        }

        /// <summary>Points the camera at <paramref name="point"/> until <paramref name="owner"/> releases it.</summary>
        public static void SetFocus(Object owner, Vector2 point, float? size)
        {
            focusOwner = owner != null ? owner : throw new System.ArgumentNullException(nameof(owner));
            focusPoint = point;
            focusSize = size;
        }

        /// <summary>Clears the focus override, but only if <paramref name="owner"/> still holds it.</summary>
        public static void ClearFocus(Object owner)
        {
            if (focusOwner != owner)
                return;
            focusOwner = null;
            focusSize = null;
        }

        void Awake()
        {
            if (playerA == null || playerB == null)
                throw new System.Exception("CameraSetup: both players must be assigned");

            cam = GetComponent<Camera>();
            if (cam == null)
                cam = Camera.main;
            if (cam == null)
                throw new System.Exception("CameraSetup: no camera on this object and no MainCamera in the scene");

            // Static state outlives scenes; never inherit a focus or shake from the previous one.
            focusOwner = null;
            focusSize = null;
            shakeOffset = Vector3.zero;

            currentSize = orthoSize;
            cam.orthographicSize = orthoSize;
            smoothPos = cam.transform.position;
        }

        void LateUpdate()
        {
            float targetSize = focusSize ?? orthoSize;
            currentSize = Mathf.SmoothDamp(currentSize, targetSize, ref sizeVelocity, smoothTime);
            cam.orthographicSize = currentSize;

            Vector3 target = focusOwner != null
                ? (Vector3)focusPoint
                : Vector3.Lerp(playerA.position, playerB.position, 0.5f);

            smoothPos = Vector3.SmoothDamp(smoothPos, target + CameraDepth, ref velocity, smoothTime);
            cam.transform.position = smoothPos + shakeOffset;
        }

        void OnDestroy()
        {
            DOTween.Kill(ShakeTweenId);
        }
    }
}
