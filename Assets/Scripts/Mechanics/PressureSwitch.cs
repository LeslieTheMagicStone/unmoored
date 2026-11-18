using UnityEngine;

namespace CGJ2026
{
    /// <summary>Holds a gate and/or elevator open while at least one player stands on it.</summary>
    [RequireComponent(typeof(Collider2D), typeof(SpriteRenderer))]
    public class PressureSwitch : MonoBehaviour
    {
        [SerializeField] Gate targetGate;
        [SerializeField] Elevator targetElevator;
        [SerializeField] Sprite offSprite;
        [SerializeField] Sprite onSprite;

        SpriteRenderer sr;
        int playersOnSwitch;

        void Awake()
        {
            if (!GetComponent<Collider2D>().isTrigger)
                throw new System.Exception("PressureSwitch collider must be set to IsTrigger");
            if (targetGate == null && targetElevator == null)
                throw new System.Exception($"PressureSwitch [{name}]: assign a target gate or elevator");
            if (offSprite == null || onSprite == null)
                throw new System.Exception($"PressureSwitch [{name}]: assign both sprites");

            sr = GetComponent<SpriteRenderer>();
            sr.drawMode = SpriteDrawMode.Simple;
            sr.sprite = offSprite;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<PlayerController>() == null)
                return;

            playersOnSwitch++;
            if (playersOnSwitch == 1)
                SetPressed(true);
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (other.GetComponent<PlayerController>() == null)
                return;

            playersOnSwitch = Mathf.Max(0, playersOnSwitch - 1);
            if (playersOnSwitch == 0)
                SetPressed(false);
        }

        void SetPressed(bool pressed)
        {
            SwapSprite(pressed ? onSprite : offSprite);

            if (targetGate != null)
            {
                if (pressed) targetGate.Open();
                else targetGate.Close();
            }

            if (targetElevator != null)
            {
                if (pressed) targetElevator.Open();
                else targetElevator.Close();
            }
        }

        /// <summary>Swaps the sprite while keeping its bottom edge in place, since the two sprites differ in height.</summary>
        void SwapSprite(Sprite target)
        {
            float oldHeight = sr.sprite.bounds.size.y * transform.localScale.y;
            sr.sprite = target;
            float newHeight = target.bounds.size.y * transform.localScale.y;

            Vector3 p = transform.localPosition;
            p.y += (newHeight - oldHeight) * 0.5f;
            transform.localPosition = p;
        }
    }
}
