using UnityEngine;

namespace CGJ2026
{
    public enum CharacterForm { Human, Anchor }

    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Identity")]
        [SerializeField] int playerIndex = 0;

        [Header("Config")]
        [SerializeField] PlayerConfig config;

        [Header("Key Bindings")]
        [SerializeField] KeyCode moveLeftKey = KeyCode.A;
        [SerializeField] KeyCode moveRightKey = KeyCode.D;
        [SerializeField] KeyCode jumpKey = KeyCode.W;
        [SerializeField] KeyCode anchorKey = KeyCode.E;

        [Header("Physics")]
        [SerializeField] Transform groundCheck;
        [SerializeField] LayerMask groundLayer;
        [SerializeField] float wallCheckDistance = 0.05f;

        CharacterForm form = CharacterForm.Human;
        Rigidbody2D rb;
        BoxCollider2D col;
        SpriteRenderer spriteRenderer;
        Vector3 humanScale = Vector3.one;
        Vector3 anchorScale = new(1.3f, 0.5f, 1f);
        Color anchorColor;
        Color humanColor;
        float moveInput;
        bool jumpPressed;
        bool anchorTogglePressed;
        bool isGrounded;
        float coyoteTimer;

        public CharacterForm Form => form;
        public int PlayerIndex => playerIndex;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<BoxCollider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
                throw new System.Exception($"PlayerController [{playerIndex}]: missing SpriteRenderer");

            humanColor = spriteRenderer.color;
            anchorColor = Color.Lerp(humanColor, Color.black, 0.5f);

            if (config == null)
                throw new System.Exception($"PlayerController [{playerIndex}]: config not assigned");

            rb.gravityScale = config.GravityScale;
            rb.drag = config.HumanDrag;
            if (groundCheck == null)
                throw new System.Exception($"PlayerController [{playerIndex}]: groundCheck not assigned");
        }

        void Update() => ReadInput();

        void FixedUpdate()
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, config.GroundCheckRadius, groundLayer);

            if (anchorTogglePressed)
            {
                anchorTogglePressed = false;
                ToggleForm();
            }

            if (form == CharacterForm.Human)
            {
                if (isGrounded)
                    coyoteTimer = config.CoyoteTime;
                else
                    coyoteTimer -= Time.fixedDeltaTime;

                bool wallBlocked = moveInput != 0f && IsAgainstWall(Mathf.Sign(moveInput));
                float vx = wallBlocked ? 0f : moveInput * config.MoveSpeed;
                rb.velocity = new Vector2(vx, rb.velocity.y);

                if (jumpPressed && coyoteTimer > 0f)
                {
                    jumpPressed = false;
                    coyoteTimer = 0f;
                    rb.AddForce(Vector2.up * config.JumpForce, ForceMode2D.Impulse);
                }
                else if (jumpPressed)
                {
                    jumpPressed = false; // consume stale input, don't jump on land
                }
            }
        }

        void ReadInput()
        {
            float h = 0f;
            if (Input.GetKey(moveLeftKey)) h -= 1f;
            if (Input.GetKey(moveRightKey)) h += 1f;
            moveInput = h;

            if (Input.GetKeyDown(jumpKey)) jumpPressed = true;
            if (Input.GetKeyDown(anchorKey)) anchorTogglePressed = true;
        }

        void ToggleForm()
        {
            if (form == CharacterForm.Human)
            {
                form = CharacterForm.Anchor;
                rb.drag = config.AnchorDrag;
                spriteRenderer.color = anchorColor;
                transform.localScale = anchorScale;
                EventBus.Emit(E.PlayerAnchored, this);
            }
            else
            {
                form = CharacterForm.Human;
                rb.drag = config.HumanDrag;
                spriteRenderer.color = humanColor;
                transform.localScale = humanScale;
                EventBus.Emit(E.PlayerReleased, this);
            }
        }

        public void ResetToHuman()
        {
            coyoteTimer = 0f;
            if (form == CharacterForm.Human) return;
            form = CharacterForm.Human;
            rb.drag = config.HumanDrag;
            rb.velocity = Vector2.zero;
            spriteRenderer.color = humanColor;
            transform.localScale = humanScale;
        }

        bool IsAgainstWall(float direction)
        {
            Vector2 size = col.size * 0.9f;
            Vector2 origin = (Vector2)transform.position + col.offset;
            RaycastHit2D hit = Physics2D.BoxCast(origin, size, 0f, new Vector2(direction, 0), wallCheckDistance, groundLayer);
            return hit.collider != null;
        }
    }
}
