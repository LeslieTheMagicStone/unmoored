using DG.Tweening;
using UnityEngine;

namespace CGJ2026
{
    /// <summary>Raises the water surface in steps; each activated <see cref="Checkpoint"/> sets the next height.</summary>
    public class RisingWater : MonoBehaviour
    {
        [SerializeField] Transform waterRoot;
        [SerializeField] float riseDuration = 1.5f;
        [SerializeField] Ease riseEase = Ease.OutCubic;

        Tween riseTween;

        public void RiseTo(float y)
        {
            riseTween?.Kill();
            riseTween = waterRoot.DOMoveY(y, riseDuration).SetEase(riseEase);
        }

        void Awake()
        {
            if (waterRoot == null)
                throw new System.Exception("RisingWater: waterRoot not assigned");
        }

        void OnDestroy()
        {
            riseTween?.Kill();
        }
    }
}
