using DG.Tweening;
using UnityEngine;

namespace CGJ2026
{
    public class CameraSetup : MonoBehaviour
    {
        [SerializeField] Transform playerA;
        [SerializeField] Transform playerB;
        [SerializeField] float smoothTime = 0.3f;
        [SerializeField] float orthoSize = 6f;

        static Camera cam;
        static Vector3 smoothPos;
        static Vector3 shakeOffset;
        Vector3 velocity;
        float defaultSize;

        public static Vector2? FocusOverride { get; set; }
        public static float? SizeOverride { get; set; }

        public static void Shake(float intensity, float duration)
        {
            DOTween.Kill("cam_shake");
            DOTween.To(() => intensity, v =>
            {
                shakeOffset = (Vector3)(Random.insideUnitCircle * v);
            }, 0f, duration)
                .SetId("cam_shake")
                .SetUpdate(true)
                .OnComplete(() => shakeOffset = Vector3.zero);
        }

        void Awake()
        {
            cam = Camera.main;
            if (cam != null)
            {
                defaultSize = cam.orthographicSize;
                cam.orthographicSize = orthoSize;
                smoothPos = cam.transform.position;
            }
        }

        void LateUpdate()
        {
            if (cam == null) return;

            cam.orthographicSize = SizeOverride ?? orthoSize;

            Vector3 target;
            if (FocusOverride.HasValue)
                target = (Vector3)FocusOverride.Value + Vector3.back * 10f;
            else if (playerA != null && playerB != null)
                target = Vector3.Lerp(playerA.position, playerB.position, 0.5f) + Vector3.back * 10f;
            else
                return;

            smoothPos = Vector3.SmoothDamp(smoothPos, target, ref velocity, smoothTime);
            cam.transform.position = smoothPos + shakeOffset;
        }

        void OnDestroy()
        {
            DOTween.Kill("cam_shake");
        }
    }
}
