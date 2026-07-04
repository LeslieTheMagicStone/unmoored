using UnityEngine;

namespace CGJ2026
{
    [RequireComponent(typeof(Collider2D), typeof(SpriteRenderer))]
    public class PressureSwitch : MonoBehaviour
    {
        [SerializeField] Gate targetGate;
        [SerializeField] Elevator targetElevator;
        [SerializeField] Sprite offSprite;
        [SerializeField] Sprite onSprite;

        SpriteRenderer sr;
        int bodiesOnSwitch;

        void Awake()
        {
            var col = GetComponent<Collider2D>();
            if (!col.isTrigger)
                throw new System.Exception("PressureSwitch collider must be set to IsTrigger");

            sr = GetComponent<SpriteRenderer>();
            sr.drawMode = SpriteDrawMode.Simple;
            sr.sprite = offSprite;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<PlayerController>() == null) return;
            bodiesOnSwitch++;
            if (bodiesOnSwitch == 1)
                SwapSprite(onSprite);
            targetGate?.Open();
            targetElevator?.Open();
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (other.GetComponent<PlayerController>() == null) return;
            bodiesOnSwitch = Mathf.Max(0, bodiesOnSwitch - 1);
            if (bodiesOnSwitch == 0)
            {
                SwapSprite(offSprite);
                targetGate?.Close();
                targetElevator?.Close();
            }
        }

        void SwapSprite(Sprite target)
        {
            if (sr.sprite == target) return;

            float oldHeight = sr.sprite != null ? sr.sprite.bounds.size.y * transform.localScale.y : 0f;
            sr.sprite = target;
            float newHeight = target.bounds.size.y * transform.localScale.y;

            Vector3 p = transform.localPosition;
            p.y += (newHeight - oldHeight) * 0.5f;
            transform.localPosition = p;
        }
    }
}
