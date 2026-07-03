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
        [SerializeField] FootSensor footSensor;
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
        RopeSystem ropeSystem;

        public CharacterForm Form => form;
        public int PlayerIndex => playerIndex;
        public PlayerConfig Config => config;

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
            if (footSensor == null)
                throw new System.Exception($"PlayerController [{playerIndex}]: footSensor not assigned");

            footSensor.Init(groundLayer);
            ropeSystem = FindObjectOfType<RopeSystem>();
        }

        void Update() => ReadInput();

        void FixedUpdate()
        {
            isGrounded = footSensor.IsGrounded;

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

                bool hasGroundControl = isGrounded || coyoteTimer > 0f;

                if (hasGroundControl)
                {
                    bool wallBlocked = moveInput != 0f && IsAgainstWall(Mathf.Sign(moveInput));
                    float vx = wallBlocked ? 0f : moveInput * config.MoveSpeed;
                    rb.velocity = new Vector2(vx, rb.velocity.y);
                }
                else if (moveInput != 0f)
                {
                    if (ropeSystem != null && IsRopeTaut())
                        ApplySwingForce();
                    else
                        ApplyAirControl();
                }

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
                rb.mass = config.AnchorMass;
                rb.drag = config.AnchorDrag;
                spriteRenderer.color = anchorColor;
                transform.localScale = anchorScale;
                EventBus.Emit(E.PlayerAnchored, this);
            }
            else
            {
                form = CharacterForm.Human;
                rb.mass = config.HumanMass;
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
            rb.mass = config.HumanMass;
            rb.drag = config.HumanDrag;
            rb.velocity = Vector2.zero;
            spriteRenderer.color = humanColor;
            transform.localScale = humanScale;
        }

        bool IsRopeTaut()
        {
            Transform other = GetOtherPlayer();
            if (other == null) return false;
            return Vector2.Distance(transform.position, other.position) >= ropeSystem.MaxLength - 0.3f;
        }

        void ApplySwingForce()
        {
            Transform other = GetOtherPlayer();
            Vector2 ropeVec = (Vector2)transform.position - (Vector2)other.position;
            Vector2 tangent = new(-ropeVec.y, ropeVec.x);
            tangent.Normalize();

            if (Vector2.Dot(tangent, Vector2.right) * moveInput < 0f)
                tangent = -tangent;

            rb.AddForce(config.SwingForce * Mathf.Abs(moveInput) * tangent, ForceMode2D.Force);
        }

        void ApplyAirControl()
        {
            rb.AddForce(config.SwingForce * 0.4f * Mathf.Sign(moveInput) * Vector2.right, ForceMode2D.Force);
        }

        Transform GetOtherPlayer()
        {
            if (ropeSystem == null) return null;
            return ropeSystem.PlayerA == this ? ropeSystem.PlayerB.transform : ropeSystem.PlayerA.transform;
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
