using DG.Tweening;
using UnityEngine;

namespace CGJ2026
{
    public class RisingWater : MonoBehaviour
    {
        [SerializeField] Transform waterRoot;
        [SerializeField] float riseDuration = 1.5f;
        [SerializeField] Ease riseEase = Ease.OutCubic;

        void Awake()
        {
            if (waterRoot == null)
                throw new System.Exception("RisingWater: waterRoot not assigned");
        }

        public void RiseTo(float y)
        {
            waterRoot.DOMoveY(y, riseDuration).SetEase(riseEase);
        }
    }
}
