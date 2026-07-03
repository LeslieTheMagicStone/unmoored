using UnityEngine;

namespace CGJ2026
{
    public enum CharacterForm { Human, Anchor }

    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
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
        [SerializeField] LayerMask groundLayer;

        CharacterForm form = CharacterForm.Human;
        Rigidbody2D rb;
        BoxCollider2D col;
        float moveInput;
        bool jumpPressed;
        bool anchorTogglePressed;
        bool isGrounded;
        float coyoteTimer;
        bool justJumped;
        RopeSystem ropeSystem;

        public CharacterForm Form => form;
        public bool IsAnchored => form == CharacterForm.Anchor;
        public bool IsGrounded => isGrounded;
        public bool JustJumped => justJumped;
        public float VerticalVelocity => rb != null ? rb.velocity.y : 0f;
        public int PlayerIndex => playerIndex;
        public PlayerConfig Config => config;
        public Rigidbody2D Body => rb;
        public float MoveInput => moveInput;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<BoxCollider2D>();
            if (config == null)
                throw new System.Exception($"PlayerController [{playerIndex}]: config not assigned");

            rb.gravityScale = config.GravityScale;
            rb.drag = config.HumanDrag;
            ropeSystem = FindObjectOfType<RopeSystem>();
        }

        void Update()
        {
            ReadInput();
        }


        void FixedUpdate()
        {
            float halfW = 0.35f;
            float footY = col.bounds.min.y + 0.02f;
            Vector2 center = new(transform.position.x, footY);
            Vector2 left = center + Vector2.left * halfW;
            Vector2 right = center + Vector2.right * halfW;
            isGrounded = Physics2D.Raycast(center, Vector2.down, 0.08f, groundLayer)
                      || Physics2D.Raycast(left, Vector2.down, 0.08f, groundLayer)
                      || Physics2D.Raycast(right, Vector2.down, 0.08f, groundLayer);

            if (anchorTogglePressed)
            {
                anchorTogglePressed = false;
                ToggleAnchor();
            }

            if (form == CharacterForm.Anchor)
            {
                jumpPressed = false;

                if (isGrounded)
                {
                    if (rb.bodyType != RigidbodyType2D.Kinematic)
                    {
                        rb.bodyType = RigidbodyType2D.Kinematic;
                        rb.velocity = Vector2.zero;
                    }
                }
                else
                {
                    rb.bodyType = RigidbodyType2D.Dynamic;
                    rb.gravityScale = config.GravityScale;
                }
                return;
            }

            if (isGrounded)
            {
                coyoteTimer = config.CoyoteTime;
                justJumped = false;
            }
            else
                coyoteTimer -= Time.fixedDeltaTime;

            bool hasGroundControl = isGrounded || coyoteTimer > 0f;

            if (hasGroundControl)
            {
                float vx = moveInput * config.MoveSpeed;
                rb.velocity = new Vector2(vx, rb.velocity.y);
            }
            else if (moveInput != 0f)
            {
                bool canSwing = ropeSystem != null && ropeSystem.TryApplySwingInput(this, moveInput);
                if (!canSwing)
                    ApplyAirControl();
            }
            if (jumpPressed && coyoteTimer > 0f)
            {
                jumpPressed = false;
                coyoteTimer = 0f;
                justJumped = true;
                rb.AddForce(Vector2.up * config.JumpForce, ForceMode2D.Impulse);
            }
            else if (jumpPressed)
            {
                jumpPressed = false; // consume stale input, don't jump on land
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

        void ToggleAnchor()
        {
            if (form == CharacterForm.Anchor)
            {
                ExitAnchorForm(true);
                return;
            }

            EnterAnchorForm();
        }

        public void ResetToHuman()
        {
            bool wasAnchored = form == CharacterForm.Anchor;
            coyoteTimer = 0f;
            jumpPressed = false;
            RestoreHumanState(wasAnchored);
        }

        void EnterAnchorForm()
        {
            form = CharacterForm.Anchor;
            jumpPressed = false;
            coyoteTimer = 0f;
            rb.angularVelocity = 0f;
            rb.velocity *= 1.15f;
            rb.mass = config.AnchorMass;
            rb.drag = config.AnchorDrag;
            EventBus.Emit(E.PlayerAnchored, this);
        }

        void ExitAnchorForm(bool emitEvent)
        {
            if (form == CharacterForm.Human) return;
            RestoreHumanState(emitEvent);
        }

        void RestoreHumanState(bool wasAnchored)
        {
            form = CharacterForm.Human;
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.mass = config.HumanMass;
            rb.drag = config.HumanDrag;
            rb.gravityScale = config.GravityScale;
            rb.angularVelocity = 0f;
            if (wasAnchored)
                EventBus.Emit(E.PlayerReleased, this);
        }

        void ApplyAirControl()
        {
            rb.AddForce(config.SwingForce * 0.4f * Mathf.Sign(moveInput) * Vector2.right, ForceMode2D.Force);
        }
    }
}
