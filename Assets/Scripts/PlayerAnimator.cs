using UnityEngine;

namespace CGJ2026
{
    public class PlayerAnimator : MonoBehaviour
    {
        [SerializeField] PlayerController controller;
        [SerializeField] Sprite idleSprite;
        [SerializeField] Sprite[] walkSprites;
        [SerializeField] Sprite[] jumpSprites;
        [SerializeField] Sprite anchorSprite;
        [SerializeField] float fps = 12f;

        SpriteRenderer spriteRenderer;
        float timer;
        int frame;
        bool wasGrounded;

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
                throw new System.Exception("PlayerAnimator: missing SpriteRenderer");
        }

        void LateUpdate()
        {
            bool isGrounded = controller.IsGrounded;
            if (!isGrounded && wasGrounded)
                frame = 0;

            Sprite result;
            if (controller.Form == CharacterForm.Anchor)
            {
                result = anchorSprite;
            }
            else if (!isGrounded && jumpSprites.Length > 0)
            {
                result = jumpSprites[Mathf.Min(frame, jumpSprites.Length - 1)];
            }
            else if (controller.MoveInput != 0f && walkSprites.Length > 0)
            {
                timer += Time.deltaTime;
                if (timer >= 1f / fps)
                {
                    timer -= 1f / fps;
                    frame = (frame + 1) % walkSprites.Length;
                }
                result = walkSprites[frame];
            }
            else
            {
                result = idleSprite;
                timer = 0f;
                frame = 0;
            }

            spriteRenderer.sprite = result;
            if (controller.MoveInput != 0f)
                spriteRenderer.flipX = controller.MoveInput < 0f;

            wasGrounded = isGrounded;
        }
    }
}
