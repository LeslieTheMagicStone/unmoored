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
        [SerializeField] KeyCode reelKey = KeyCode.LeftShift;

        [Header("Physics")]
        [SerializeField] LayerMask groundLayer;
        [SerializeField] LayerMask anchorLayer;
        [SerializeField] float anchorProbeReach = 1.15f;

        CharacterForm form = CharacterForm.Human;
        Rigidbody2D rb;
        BoxCollider2D col;
        float moveInput;
        float jumpBufferTimer;
        bool jumpHeld;
        bool anchorTogglePressed;
        bool isGrounded;
        bool wasGrounded;
        float coyoteTimer;
        bool justJumped;
        bool jumpCutApplied;
        Vector2 snapNormal;
        bool snappedToAnchorPoint;
        RopeSystem ropeSystem;
        readonly RaycastHit2D[] anchorSnapHits = new RaycastHit2D[8];
        static readonly float[] FootRayOffsets = { 0f, -0.5f, 0.5f, -0.75f, 0.75f };
        const float FootRayDistance = 0.08f;
        const float LedgeHorizontalReach = 0.25f;

        public CharacterForm Form => form;
        public bool IsAnchored => form == CharacterForm.Anchor;
        public LayerMask GroundLayer => groundLayer;
        public bool IsGrounded => isGrounded;
        public bool JustJumped => justJumped;
        public float VerticalVelocity => rb != null ? rb.velocity.y : 0f;
        public int PlayerIndex => playerIndex;
        public PlayerConfig Config => config;
        public Rigidbody2D Body => rb;
        public float MoveInput => moveInput;
        public Vector2 SnapNormal => snapNormal;
        public bool SnappedToAnchorPoint => snappedToAnchorPoint;

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
            float footY = col.bounds.min.y + 0.02f;
            float x = transform.position.x;
            isGrounded = CastFootRays(footY, x, out _);

            if (form == CharacterForm.Human && !isGrounded && Mathf.Abs(moveInput) > 0.1f)
                TryLedgeAssist();

            ConveyorTerrain conveyor = isGrounded ? GetConveyorUnderfoot() : null;

            if (anchorTogglePressed)
            {
                anchorTogglePressed = false;
                ToggleAnchor();
            }

            if (form == CharacterForm.Anchor)
            {
                jumpBufferTimer = 0f;
                if (isGrounded && rb.bodyType != RigidbodyType2D.Kinematic)
                {
                    rb.bodyType = RigidbodyType2D.Kinematic;
                    rb.velocity = Vector2.zero;
                }
                return;
            }

            if (isGrounded)
            {
                if (!wasGrounded)
                {
                    Juice.DustPoof(col.bounds.min);
                    AudioManager.Instance?.PlayLand();
                }
                coyoteTimer = config.CoyoteTime;
                justJumped = false;
                jumpCutApplied = false;
            }
            else
                coyoteTimer -= Time.fixedDeltaTime;

            bool hasGroundControl = isGrounded || coyoteTimer > 0f;

            if (hasGroundControl)
            {
                Vector2 carry = conveyor != null ? conveyor.CurrentVelocity : Vector2.zero;
                float vx = moveInput * config.MoveSpeed + carry.x;
                float vy = isGrounded && conveyor != null ? carry.y : rb.velocity.y;
                rb.velocity = new Vector2(vx, vy);
                if (Mathf.Abs(moveInput) > 0.1f)
                    AudioManager.Instance?.PlayWalk();
            }
            else if (moveInput != 0f)
            {
                bool canSwing = ropeSystem != null && ropeSystem.TryApplySwingInput(this, moveInput);
                if (!canSwing)
                    ApplyAirControl();
            }
            if (jumpBufferTimer > 0f && coyoteTimer > 0f)
            {
                jumpBufferTimer = 0f;
                coyoteTimer = 0f;
                justJumped = true;
                jumpHeld = true;
                rb.AddForce(Vector2.up * config.JumpForce, ForceMode2D.Impulse);
                AudioManager.Instance?.PlayJump();
            }
            else if (isGrounded)
            {
                jumpBufferTimer -= Time.fixedDeltaTime;
            }

            if (!jumpHeld && !jumpCutApplied && rb.velocity.y > 0f && !isGrounded)
            {
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * config.JumpCutMultiplier);
                jumpCutApplied = true;
            }

            wasGrounded = isGrounded;
        }

        void ReadInput()
        {
            float h = 0f;
            if (Input.GetKey(moveLeftKey)) h -= 1f;
            if (Input.GetKey(moveRightKey)) h += 1f;
            moveInput = h;

            if (Input.GetKeyDown(jumpKey))
            {
                jumpBufferTimer = config.JumpBufferTime;
                jumpHeld = true;
            }
            if (Input.GetKeyUp(jumpKey))
                jumpHeld = false;
            if (Input.GetKeyDown(anchorKey)) anchorTogglePressed = true;

            if (Input.GetKey(reelKey))
                ropeSystem?.StartReel();
            else
                ropeSystem?.StopReel();
        }

        void ToggleAnchor()
        {
            if (form == CharacterForm.Anchor)
            {
                ExitAnchorForm(true);
                return;
            }

            TrySnapToSurface();
            EnterAnchorForm();
        }

        public void ResetToHuman()
        {
            bool wasAnchored = form == CharacterForm.Anchor;
            coyoteTimer = 0f;
            jumpBufferTimer = 0f;
            RestoreHumanState(wasAnchored);
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        void EnterAnchorForm()
        {
            form = CharacterForm.Anchor;
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
            rb.angularVelocity = 0f;
            rb.mass = config.AnchorMass;
            rb.drag = config.AnchorDrag;
            EventBus.Emit(E.PlayerAnchored, this);
        }

        void TrySnapToSurface()
        {
            Bounds b = col.bounds;
            Vector2 center = b.center;
            float halfH = b.extents.y;
            float reach = (b.extents.x + halfH) * anchorProbeReach;

            Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
            Vector2 bestPoint = Vector2.zero;
            Vector2 bestNormal = Vector2.zero;
            Collider2D bestCollider = null;
            float bestDist = float.MaxValue;

            for (int i = 0; i < 4; i++)
            {
                Vector2 dir = directions[i];
                int n = Physics2D.RaycastNonAlloc(center, dir, anchorSnapHits, reach, anchorLayer);
                for (int j = 0; j < n; j++)
                {
                    float d = anchorSnapHits[j].distance;
                    if (d > 0.001f && d < bestDist)
                    {
                        bestDist = d;
                        Vector2 surfacePoint = anchorSnapHits[j].point;
                        Vector2 normal = anchorSnapHits[j].normal;
                        bestPoint = surfacePoint + normal * halfH * 1.15f;
                        bestNormal = normal;
                        bestCollider = anchorSnapHits[j].collider;
                    }
                }
            }

            if (bestDist >= float.MaxValue) return;

            snapNormal = bestNormal;
            snappedToAnchorPoint = bestCollider != null && bestCollider.GetComponent<AnchorPoint>() != null;
            rb.position = bestPoint;
            rb.velocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
            AudioManager.Instance?.PlayAnchorSnap();
            Juice.ShakeLight();
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
            snapNormal = Vector2.zero;
            snappedToAnchorPoint = false;
            if (wasAnchored)
                EventBus.Emit(E.PlayerReleased, this);
        }

        void ApplyAirControl()
        {
            rb.AddForce(config.SwingForce * 0.4f * Mathf.Sign(moveInput) * Vector2.right, ForceMode2D.Force);
        }

        public ConveyorTerrain GetConveyorUnderfoot()
        {
            float footY = col.bounds.min.y + 0.02f;
            float x = transform.position.x;

            if (!CastFootRays(footY, x, out RaycastHit2D hit))
                return null;

            return hit.collider.GetComponentInParent<ConveyorTerrain>();
        }

        bool CastFootRays(float footY, float x, out RaycastHit2D hit)
        {
            for (int i = 0; i < FootRayOffsets.Length; i++)
            {
                hit = Physics2D.Raycast(
                    new Vector2(x + FootRayOffsets[i], footY),
                    Vector2.down,
                    FootRayDistance,
                    groundLayer);
                if (hit.collider != null)
                    return true;
            }

            hit = default;
            return false;
        }

        void TryLedgeAssist()
        {
            Bounds b = col.bounds;
            float dir = Mathf.Sign(moveInput);
            float extX = b.extents.x;

            Vector2[] origins =
            {
                new(b.center.x + dir * extX, b.center.y),
                new(b.center.x + dir * extX, b.max.y - 0.05f)
            };

            foreach (var origin in origins)
            {
                var hit = Physics2D.Raycast(origin, Vector2.right * dir, LedgeHorizontalReach, groundLayer);
                if (hit.collider == null) continue;

                float ledgeY = hit.collider.bounds.max.y;
                float footY = b.min.y;
                float gap = ledgeY - footY;

                if (gap < 0f || gap > config.LedgeAssistHeight) continue;
                if (ledgeY > b.max.y) continue;

                float checkX = hit.point.x + dir * 0.1f;
                var ceiling = Physics2D.Raycast(
                    new Vector2(checkX, ledgeY + 0.02f),
                    Vector2.up, b.size.y, groundLayer);
                if (ceiling.collider != null) continue;

                rb.position = new Vector2(rb.position.x, ledgeY + b.extents.y + 0.01f);
                if (rb.velocity.y < 0f)
                    rb.velocity = new Vector2(rb.velocity.x, 0f);
                break;
            }
        }
    }
}
