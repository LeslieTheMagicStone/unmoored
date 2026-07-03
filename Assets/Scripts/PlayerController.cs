using UnityEngine;

namespace CGJ2026
{
    public enum CharacterForm { Human, Anchor }

    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] int playerIndex = 0;
        [SerializeField] PlayerConfig config;
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

        public CharacterForm Form => form;

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
                bool wallBlocked = moveInput != 0f && IsAgainstWall(Mathf.Sign(moveInput));
                float vx = wallBlocked ? 0f : moveInput * config.MoveSpeed;
                rb.velocity = new Vector2(vx, rb.velocity.y);

                if (jumpPressed && isGrounded)
                {
                    jumpPressed = false;
                    rb.AddForce(Vector2.up * config.JumpForce, ForceMode2D.Impulse);
                }
            }
        }

        void ReadInput()
        {
            if (playerIndex == 0)
            {
                float h = 0f;
                if (Input.GetKey(KeyCode.A)) h = -1f;
                if (Input.GetKey(KeyCode.D)) h = 1f;
                moveInput = h;

                if (Input.GetKeyDown(KeyCode.W)) jumpPressed = true;
                if (Input.GetKeyDown(KeyCode.E)) anchorTogglePressed = true;
            }
            else
            {
                float h = 0f;
                if (Input.GetKey(KeyCode.LeftArrow)) h = -1f;
                if (Input.GetKey(KeyCode.RightArrow)) h = 1f;
                moveInput = h;

                if (Input.GetKeyDown(KeyCode.UpArrow)) jumpPressed = true;
                if (Input.GetKeyDown(KeyCode.RightShift)) anchorTogglePressed = true;
            }
        }

        void ToggleForm()
        {
            if (form == CharacterForm.Human)
            {
                form = CharacterForm.Anchor;
                rb.velocity = Vector2.zero;
                rb.bodyType = RigidbodyType2D.Kinematic;
                spriteRenderer.color = anchorColor;
                transform.localScale = anchorScale;
                EventBus.Emit(E.PlayerAnchored, this);
            }
            else
            {
                form = CharacterForm.Human;
                rb.bodyType = RigidbodyType2D.Dynamic;
                spriteRenderer.color = humanColor;
                transform.localScale = humanScale;
                EventBus.Emit(E.PlayerReleased, this);
            }
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
