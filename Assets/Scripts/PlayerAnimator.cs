using UnityEngine;

namespace CGJ2026
{
    public class PlayerAnimator : MonoBehaviour
    {
        [SerializeField] PlayerController controller;
        [SerializeField] Sprite idleSprite;
        [SerializeField] Sprite[] walkSprites;
        [SerializeField] float fps = 12f;

        SpriteRenderer spriteRenderer;
        float timer;
        int frame;

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
                throw new System.Exception("PlayerAnimator: missing SpriteRenderer");
        }

        void LateUpdate()
        {
            if (controller.MoveInput != 0f && walkSprites.Length > 0)
            {
                timer += Time.deltaTime;
                if (timer >= 1f / fps)
                {
                    timer -= 1f / fps;
                    frame = (frame + 1) % walkSprites.Length;
                }
                spriteRenderer.sprite = walkSprites[frame];
                spriteRenderer.flipX = controller.MoveInput < 0f;
            }
            else
            {
                spriteRenderer.sprite = idleSprite;
                timer = 0f;
                frame = 0;
                // keep last flip direction
            }
        }
    }
}
