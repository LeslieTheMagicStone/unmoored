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
        [SerializeField] FootSensor footSensor;
        [SerializeField] LayerMask groundLayer;

        CharacterForm form = CharacterForm.Human;
        Rigidbody2D rb;
        BoxCollider2D col;
        SpriteRenderer spriteRenderer;
        Vector3 baseScale;
        float maxSwingTilt = 35f;
        float tiltSpeed = 8f;
        Color anchorColor;
        Color humanColor;
        float moveInput;
        bool jumpPressed;
        bool anchorTogglePressed;
        bool isGrounded;
        float coyoteTimer;
        bool isSwinging;
        float swingTilt;
        RopeSystem ropeSystem;
        readonly Collider2D[] anchorHits = new Collider2D[8];

        public CharacterForm Form => form;
        public bool IsAnchored => form == CharacterForm.Anchor;
        public int PlayerIndex => playerIndex;
        public PlayerConfig Config => config;
        public Rigidbody2D Body => rb;
        public float MoveInput => moveInput;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<BoxCollider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
                throw new System.Exception($"PlayerController [{playerIndex}]: missing SpriteRenderer");

            humanColor = spriteRenderer.color;
            anchorColor = Color.Lerp(humanColor, Color.black, 0.5f);
            baseScale = transform.localScale;

            if (config == null)
                throw new System.Exception($"PlayerController [{playerIndex}]: config not assigned");

            rb.gravityScale = config.GravityScale;
            rb.drag = config.HumanDrag;
            if (footSensor == null)
                throw new System.Exception($"PlayerController [{playerIndex}]: footSensor not assigned");

            footSensor.Init(groundLayer);
            ropeSystem = FindObjectOfType<RopeSystem>();
        }

        void Update()
        {
            ReadInput();
        }

        void LateUpdate()
        {
            float targetTilt = isSwinging ? maxSwingTilt * Mathf.Sign(-moveInput) : 0f;
            swingTilt = Mathf.Lerp(swingTilt, targetTilt, tiltSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0f, 0f, swingTilt);
        }

        void FixedUpdate()
        {
            isGrounded = footSensor.IsGrounded;

            if (anchorTogglePressed)
            {
                anchorTogglePressed = false;
                ToggleAnchor();
            }

            if (form == CharacterForm.Anchor)
            {
                jumpPressed = false;
                isSwinging = false;
                rb.velocity = Vector2.zero;
                return;
            }

            if (isGrounded)
                coyoteTimer = config.CoyoteTime;
            else
                coyoteTimer -= Time.fixedDeltaTime;

            bool hasGroundControl = isGrounded || coyoteTimer > 0f;

            if (hasGroundControl)
            {
                isSwinging = false;
                float vx = moveInput * config.MoveSpeed;
                rb.velocity = new Vector2(vx, rb.velocity.y);
            }
            else if (moveInput != 0f)
            {
                bool canSwing = ropeSystem != null && ropeSystem.TryApplySwingInput(this, moveInput);
                isSwinging = canSwing;
                if (!canSwing)
                    ApplyAirControl();
            }
            else
            {
                isSwinging = false;
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

            if (TryFindAnchorSurface(out _))
                EnterAnchorForm();
        }

        public void ResetToHuman()
        {
            bool wasAnchored = form == CharacterForm.Anchor;
            coyoteTimer = 0f;
            jumpPressed = false;
            isSwinging = false;
            RestoreHumanState();
            if (wasAnchored)
                EventBus.Emit(E.PlayerReleased, this);
        }

        void EnterAnchorForm()
        {
            form = CharacterForm.Anchor;
            isSwinging = false;
            jumpPressed = false;
            coyoteTimer = 0f;
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.mass = config.AnchorMass;
            rb.drag = config.AnchorDrag;
            spriteRenderer.color = anchorColor;
            transform.localScale = new Vector3(baseScale.x * 1.5f, baseScale.y * 0.3f, 1f);
            EventBus.Emit(E.PlayerAnchored, this);
        }

        void ExitAnchorForm(bool emitEvent)
        {
            if (form == CharacterForm.Human) return;

            RestoreHumanState();
            if (emitEvent)
                EventBus.Emit(E.PlayerReleased, this);
        }

        void RestoreHumanState()
        {
            form = CharacterForm.Human;
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.mass = config.HumanMass;
            rb.drag = config.HumanDrag;
            rb.gravityScale = config.GravityScale;
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            spriteRenderer.color = humanColor;
            transform.localScale = baseScale;
        }

        void ApplyAirControl()
        {
            rb.AddForce(config.SwingForce * 0.4f * Mathf.Sign(moveInput) * Vector2.right, ForceMode2D.Force);
        }

        bool TryFindAnchorSurface(out Vector2 anchorSurfacePoint)
        {
            Bounds playerBounds = col.bounds;
            int hitCount = Physics2D.OverlapBoxNonAlloc(
                playerBounds.center,
                config.AnchorProbeSize,
                0f,
                anchorHits,
                groundLayer);

            for (int i = 0; i < hitCount; i++)
            {
                Collider2D hit = anchorHits[i];
                if (hit == null || hit == col) continue;
                if (TryGetAnchorPointOnBounds(hit.bounds, playerBounds, out anchorSurfacePoint))
                    return true;
            }

            anchorSurfacePoint = default;
            return false;
        }

        bool TryGetAnchorPointOnBounds(Bounds groundBounds, Bounds playerBounds, out Vector2 anchorSurfacePoint)
        {
            float leftDistance = Mathf.Abs(playerBounds.center.x - groundBounds.min.x);
            float rightDistance = Mathf.Abs(playerBounds.center.x - groundBounds.max.x);
            float nearestEdgeDistance = Mathf.Min(leftDistance, rightDistance);
            bool nearHorizontalEdge = nearestEdgeDistance <= config.AnchorEdgeTolerance;
            bool overlapsVertically = playerBounds.max.y >= groundBounds.min.y &&
                playerBounds.min.y <= groundBounds.max.y + config.AnchorVerticalTolerance;

            if (!nearHorizontalEdge || !overlapsVertically)
            {
                anchorSurfacePoint = default;
                return false;
            }

            float edgeX = leftDistance <= rightDistance ? groundBounds.min.x : groundBounds.max.x;
            float surfaceY = Mathf.Clamp(playerBounds.center.y, groundBounds.min.y, groundBounds.max.y);
            anchorSurfacePoint = new Vector2(edgeX, surfaceY);
            return true;
        }
    }
}
