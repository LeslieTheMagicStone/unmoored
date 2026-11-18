using UnityEngine;

namespace CGJ2026
{
    public enum CharacterForm { Human, Anchor }

    /// <summary>
    /// Reads one player's input and drives movement and the human/anchor form switch.
    ///
    /// Anchor flow: pressing the anchor key switches to <see cref="CharacterForm.Anchor"/>.
    /// If an anchorable surface is within reach the player snaps onto it at once;
    /// otherwise the player falls and attaches on landing. Only an attached player is
    /// <see cref="IsAnchored"/>: it turns kinematic, becomes the partner's swing pivot,
    /// and raises <see cref="E.PlayerAnchored"/>. Leaving that state raises
    /// <see cref="E.PlayerReleased"/>.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public class PlayerController : MonoBehaviour
    {
        const float AirControlFactor = 0.4f;

        [Header("Identity")]
        [SerializeField] int playerIndex = 0;

        [Header("Config")]
        [SerializeField] PlayerConfig config;

        [Header("Key Bindings")]
        [SerializeField] KeyCode moveLeftKey = KeyCode.A;
        [SerializeField] KeyCode moveRightKey = KeyCode.D;
        [SerializeField] KeyCode jumpKey = KeyCode.W;
        [SerializeField] KeyCode anchorKey = KeyCode.S;

        [Header("Physics")]
        [SerializeField] LayerMask groundLayer;
        [SerializeField] LayerMask anchorLayer;
        [SerializeField] float anchorProbeReach = 1.15f;

        Rigidbody2D rb;
        BoxCollider2D box;
        RopeSystem rope;
        GroundProbe groundProbe;
        AnchorSurfaceProbe anchorProbe;

        float moveInput;
        bool jumpPressed;
        bool anchorTogglePressed;

        CharacterForm form = CharacterForm.Human;
        bool attached;
        Collider2D anchoredCollider;
        Vector2 snapNormal;

        bool isGrounded;
        bool wasGrounded;
        bool justJumped;
        float coyoteTimer;

        public int PlayerIndex => playerIndex;
        public PlayerConfig Config => config;
        public Rigidbody2D Body => rb;
        public LayerMask GroundLayer => groundLayer;

        public CharacterForm Form => form;
        /// <summary>True only while attached to terrain. A player in anchor form that is still falling is not anchored.</summary>
        public bool IsAnchored => attached;
        public Collider2D AnchoredCollider => anchoredCollider;
        public Vector2 SnapNormal => snapNormal;

        public bool IsGrounded => isGrounded;
        public bool JustJumped => justJumped;
        public float MoveInput => moveInput;
        public float VerticalVelocity => rb.velocity.y;

        /// <summary>Set by <see cref="LevelManager"/> while the respawn animation runs; blocks input and physics logic.</summary>
        public bool IsRespawning { get; set; }

        /// <summary>Called by <see cref="RopeSystem"/> to connect this player to the rope.</summary>
        public void BindRope(RopeSystem ropeSystem)
        {
            if (rope != null && rope != ropeSystem)
                throw new System.InvalidOperationException($"PlayerController [{playerIndex}] is already bound to another rope");
            rope = ropeSystem;
        }

        /// <summary>Drops the anchor because the surface can no longer hold it. Only surfaces call this.</summary>
        public void DetachFromSurface()
        {
            if (IsAnchored)
                BecomeHuman();
        }

        /// <summary>Forces human form with zero velocity. Used when respawning.</summary>
        public void ResetToHuman()
        {
            BecomeHuman();
            coyoteTimer = 0f;
            ClearBufferedInput();
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        void Awake()
        {
            if (config == null)
                throw new System.Exception($"PlayerController [{playerIndex}]: config not assigned");

            rb = GetComponent<Rigidbody2D>();
            box = GetComponent<BoxCollider2D>();
            groundProbe = new GroundProbe(box, rb, groundLayer);
            anchorProbe = new AnchorSurfaceProbe(anchorLayer, anchorProbeReach);

            rb.gravityScale = config.GravityScale;
            rb.mass = config.HumanMass;
            rb.drag = config.HumanDrag;
        }

        void Start()
        {
            if (rope == null)
                throw new System.Exception($"PlayerController [{playerIndex}]: no RopeSystem is bound to this player");
        }

        void Update()
        {
            // Time.timeScale is 0 while paused. FixedUpdate does not run then, so a key
            // pressed during pause would otherwise fire the moment the game resumes.
            if (IsRespawning || Time.timeScale == 0f)
            {
                moveInput = 0f;
                return;
            }

            float h = 0f;
            if (Input.GetKey(moveLeftKey)) h -= 1f;
            if (Input.GetKey(moveRightKey)) h += 1f;
            moveInput = h;

            if (Input.GetKeyDown(jumpKey)) jumpPressed = true;
            if (Input.GetKeyDown(anchorKey)) anchorTogglePressed = true;
        }

        void FixedUpdate()
        {
            isGrounded = groundProbe.TryGetGround(out RaycastHit2D groundHit);

            if (IsRespawning)
            {
                ClearBufferedInput();
                wasGrounded = isGrounded;
                return;
            }

            if (anchorTogglePressed)
            {
                anchorTogglePressed = false;
                ToggleAnchor();
            }

            if (form == CharacterForm.Anchor)
                TickAnchorForm(groundHit);
            else
                TickHumanForm(groundHit);

            wasGrounded = isGrounded;
        }

        void ClearBufferedInput()
        {
            jumpPressed = false;
            anchorTogglePressed = false;
        }

        void ToggleAnchor()
        {
            if (form == CharacterForm.Anchor)
            {
                BecomeHuman();
                return;
            }

            EnterAnchorForm();

            if (anchorProbe.TryFind(box.bounds, out AnchorSurface surface))
            {
                rb.position = surface.SnapPosition;
                AttachTo(surface.Collider, surface.Normal);
                AudioManager.Instance.PlayAnchorSnap();
                Juice.ShakeLight();
            }
        }

        void TickAnchorForm(RaycastHit2D groundHit)
        {
            jumpPressed = false;

            if (IsAnchored)
            {
                // The surface vanished (e.g. a crumbling platform broke): lose the pivot.
                if (!IsUsableSurface(anchoredCollider))
                    BecomeHuman();
                return;
            }

            // Anchor form in the air just falls under gravity; it attaches on landing.
            if (isGrounded)
                AttachTo(groundHit.collider, groundHit.normal);
        }

        void TickHumanForm(RaycastHit2D groundHit)
        {
            if (isGrounded)
            {
                if (!wasGrounded)
                {
                    Juice.DustPoof(box.bounds.min);
                    AudioManager.Instance.PlayLand();
                }
                coyoteTimer = config.CoyoteTime;
                justJumped = false;
            }
            else
            {
                coyoteTimer -= Time.fixedDeltaTime;
            }

            bool hasGroundControl = isGrounded || coyoteTimer > 0f;
            if (hasGroundControl)
                ApplyGroundMovement(isGrounded ? groundHit.collider.GetComponentInParent<ConveyorTerrain>() : null);
            else if (moveInput != 0f && !rope.TryApplySwingInput(this, moveInput))
                ApplyAirControl();

            if (!jumpPressed)
                return;

            jumpPressed = false;
            if (coyoteTimer > 0f)
                Jump();
            else
                TryWallJump();
        }

        void ApplyGroundMovement(ConveyorTerrain conveyor)
        {
            Vector2 carry = conveyor != null ? conveyor.CurrentVelocity : Vector2.zero;
            float vx = moveInput * config.MoveSpeed + carry.x;
            float vy = conveyor != null ? carry.y : Mathf.Max(0f, rb.velocity.y);
            rb.velocity = new Vector2(vx, vy);

            if (isGrounded && moveInput != 0f)
                AudioManager.Instance.PlayWalk();
        }

        void ApplyAirControl()
        {
            rb.AddForce(config.SwingForce * AirControlFactor * Mathf.Sign(moveInput) * Vector2.right, ForceMode2D.Force);
        }

        void Jump()
        {
            coyoteTimer = 0f;
            justJumped = true;
            rb.AddForce(Vector2.up * config.JumpForce, ForceMode2D.Impulse);
            AudioManager.Instance.PlayJump();
        }

        void TryWallJump()
        {
            int wallDir = groundProbe.FindAdjacentWall();
            if (wallDir == 0)
                return;

            rb.velocity = new Vector2(rb.velocity.x, 0f);
            rb.AddForce(new Vector2(-wallDir * config.WallJumpHorizontalForce, config.JumpForce), ForceMode2D.Impulse);
            justJumped = true;
            AudioManager.Instance.PlayJump();
        }

        void EnterAnchorForm()
        {
            form = CharacterForm.Anchor;
            ClearBufferedInput();
            coyoteTimer = 0f;
            rb.angularVelocity = 0f;
            rb.mass = config.AnchorMass;
            rb.drag = config.AnchorDrag;
            // Feedback on the press itself, even when nothing is in reach and the player just falls.
            AudioManager.Instance.PlayAnchorToggle(true);
        }

        void AttachTo(Collider2D surface, Vector2 normal)
        {
            attached = true;
            anchoredCollider = surface;
            snapNormal = normal;
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
            EventBus.Emit(E.PlayerAnchored, this);
        }

        void BecomeHuman()
        {
            bool wasAnchored = IsAnchored;
            if (form == CharacterForm.Anchor)
                AudioManager.Instance.PlayAnchorToggle(false);

            form = CharacterForm.Human;
            attached = false;
            anchoredCollider = null;
            snapNormal = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.mass = config.HumanMass;
            rb.drag = config.HumanDrag;
            rb.gravityScale = config.GravityScale;

            if (wasAnchored)
                EventBus.Emit(E.PlayerReleased, this);
        }

        static bool IsUsableSurface(Collider2D surface)
        {
            return surface != null && surface.enabled && surface.gameObject.activeInHierarchy;
        }
    }
}
