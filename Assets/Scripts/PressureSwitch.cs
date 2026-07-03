using DG.Tweening;
using UnityEngine;

namespace CGJ2026
{
    [RequireComponent(typeof(Collider2D))]
    public class PressureSwitch : MonoBehaviour
    {
        [SerializeField] Gate targetGate;
        [SerializeField] float pressDepth = 0.3f;
        [SerializeField] float animDuration = 0.15f;

        Vector3 unpressedPos;
        Vector3 pressedPos;
        int bodiesOnSwitch;

        void Awake()
        {
            var col = GetComponent<Collider2D>();
            if (!col.isTrigger)
                throw new System.Exception("PressureSwitch collider must be set to IsTrigger");

            unpressedPos = transform.localPosition;
            pressedPos = unpressedPos + Vector3.down * pressDepth;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<PlayerController>() == null) return;
            bodiesOnSwitch++;
            if (bodiesOnSwitch == 1)
            {
                transform.DOKill();
                transform.DOLocalMove(pressedPos, animDuration).SetEase(Ease.OutQuad);
            }
            targetGate?.Open();
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (other.GetComponent<PlayerController>() == null) return;
            bodiesOnSwitch = Mathf.Max(0, bodiesOnSwitch - 1);
            if (bodiesOnSwitch == 0)
            {
                transform.DOKill();
                transform.DOLocalMove(unpressedPos, animDuration).SetEase(Ease.OutBack);
                targetGate?.Close();
            }
        }
    }
}
