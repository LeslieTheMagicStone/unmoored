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
        float walkTimer;
        float takeoffTimer;
        int walkFrame;
        int takeoffFrame;
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

            Sprite result;
            if (controller.Form == CharacterForm.Anchor)
            {
                result = anchorSprite;
            }
            else if (!isGrounded && jumpSprites.Length >= 3)
            {
                if (controller.JustJumped && takeoffFrame < 2)
                {
                    takeoffTimer += Time.deltaTime;
                    if (takeoffTimer >= 1f / fps)
                    {
                        takeoffTimer -= 1f / fps;
                        takeoffFrame++;
                    }
                    result = jumpSprites[takeoffFrame];
                }
                else
                {
                    float vy = controller.VerticalVelocity;
                    result = vy >= 0f ? jumpSprites[1] : jumpSprites[2];
                }
            }
            else if (controller.MoveInput != 0f && walkSprites.Length > 0)
            {
                walkTimer += Time.deltaTime;
                if (walkTimer >= 1f / fps)
                {
                    walkTimer -= 1f / fps;
                    walkFrame = (walkFrame + 1) % walkSprites.Length;
                }
                result = walkSprites[walkFrame];
            }
            else
            {
                result = idleSprite;
                walkTimer = 0f;
                walkFrame = 0;
                takeoffFrame = 0;
                takeoffTimer = 0f;
            }

            spriteRenderer.sprite = result;
            if (controller.Form != CharacterForm.Anchor && controller.MoveInput != 0f)
                spriteRenderer.flipX = controller.MoveInput < 0f;

            wasGrounded = isGrounded;
        }
    }
}
