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
        [SerializeField] float anchorProbeReach = 1.15f;

        CharacterForm form = CharacterForm.Human;
        Rigidbody2D rb;
        BoxCollider2D col;
        float moveInput;
        bool jumpPressed;
        bool anchorTogglePressed;
        bool isGrounded;
        bool wasGrounded;
        float coyoteTimer;
        bool justJumped;
        Vector2 snapNormal;
        RopeSystem ropeSystem;
        readonly RaycastHit2D[] anchorSnapHits = new RaycastHit2D[8];
        static readonly float[] FootRayOffsets = { 0f, -0.5f, 0.5f, -0.75f, 0.75f };
        const float FootRayDistance = 0.08f;

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
            ConveyorTerrain conveyor = isGrounded ? GetConveyorUnderfoot() : null;

            if (anchorTogglePressed)
            {
                anchorTogglePressed = false;
                ToggleAnchor();
            }

            if (form == CharacterForm.Anchor)
            {
                jumpPressed = false;
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
            if (jumpPressed && coyoteTimer > 0f)
            {
                jumpPressed = false;
                coyoteTimer = 0f;
                justJumped = true;
                rb.AddForce(Vector2.up * config.JumpForce, ForceMode2D.Impulse);
                AudioManager.Instance?.PlayJump();
            }
            else if (jumpPressed)
            {
                jumpPressed = false;
            }

            wasGrounded = isGrounded;
        }

        void ReadInput()
        {
            float h = 0f;
            if (Input.GetKey(moveLeftKey)) h -= 1f;
            if (Input.GetKey(moveRightKey)) h += 1f;
            moveInput = h;

            if (Input.GetKeyDown(jumpKey)) jumpPressed = true;
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
            jumpPressed = false;
            RestoreHumanState(wasAnchored);
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        void EnterAnchorForm()
        {
            form = CharacterForm.Anchor;
            jumpPressed = false;
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
            float bestDist = float.MaxValue;

            for (int i = 0; i < 4; i++)
            {
                Vector2 dir = directions[i];
                int n = Physics2D.RaycastNonAlloc(center, dir, anchorSnapHits, reach, groundLayer);
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
                    }
                }
            }

            if (bestDist >= float.MaxValue) return;

            snapNormal = bestNormal;
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
    }
}
