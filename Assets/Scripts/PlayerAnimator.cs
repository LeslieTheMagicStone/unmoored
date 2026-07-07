using UnityEngine;

namespace CGJ2026
{
    public class PlayerAnimator : MonoBehaviour
    {
        [SerializeField] PlayerController controller;
        [SerializeField] Transform visualPivot;
        [SerializeField] Sprite idleSprite;
        [SerializeField] Sprite[] walkSprites;
        [SerializeField] Sprite[] jumpSprites;
        [SerializeField] Sprite anchorSprite;
        [SerializeField] Sprite anchorUpSprite;
        [SerializeField] Sprite[] anchorHangSprites;
        [SerializeField] float fps = 12f;

        SpriteRenderer spriteRenderer;
        float walkTimer;
        float takeoffTimer;
        int walkFrame;
        int takeoffFrame;
        float hangTimer;
        int hangStep;

        void Awake()
        {
            if (visualPivot == null)
                throw new System.Exception("PlayerAnimator: missing visualPivot");
            spriteRenderer = visualPivot.GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer == null)
                throw new System.Exception("PlayerAnimator: missing SpriteRenderer under visualPivot");
        }

        void LateUpdate()
        {
            bool isGrounded = controller.IsGrounded;

            Sprite result;
            bool flip = false;
            if (controller.Form == CharacterForm.Anchor)
            {
                Vector2 n = controller.SnapNormal;
                bool isHang = n.y < -0.5f || Mathf.Abs(n.x) > 0.5f;
                if (isHang && anchorHangSprites.Length >= 2)
                {
                    int nSprites = anchorHangSprites.Length;
                    int cycleLen = 2 * nSprites;
                    float hangFps = fps * 0.5f;
                    hangTimer += Time.deltaTime;
                    if (hangTimer >= 1f / hangFps)
                    {
                        hangTimer -= 1f / hangFps;
                        hangStep = (hangStep + 1) % cycleLen;
                    }
                    int frame = hangStep < nSprites ? hangStep : cycleLen - 1 - hangStep;
                    result = anchorHangSprites[frame];
                }
                else if (isHang)
                {
                    result = anchorUpSprite != null ? anchorUpSprite : anchorSprite;
                    hangStep = 0;
                    hangTimer = 0f;
                }
                else
                {
                    result = anchorSprite;
                    hangStep = 0;
                    hangTimer = 0f;
                }

                if (n.x > 0.5f)
                    flip = true;

                spriteRenderer.sprite = result;
                visualPivot.localScale = new Vector3(flip ? -1 : 1, 1, 1);
                return;
            }

            hangStep = 0;
            hangTimer = 0f;

            if (!isGrounded && jumpSprites.Length >= 3)
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
            if (controller.Form == CharacterForm.Anchor)
                visualPivot.localScale = new Vector3(flip ? -1 : 1, 1, 1);
            else if (controller.MoveInput != 0f)
                visualPivot.localScale = new Vector3(controller.MoveInput < 0f ? -1 : 1, 1, 1);
        }
    }
}
