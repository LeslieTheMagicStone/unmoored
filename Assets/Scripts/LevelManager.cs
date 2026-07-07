using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace CGJ2026
{
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }

        [SerializeField] Transform playerA;
        [SerializeField] Transform playerB;
        [SerializeField] Transform winZone;
        [SerializeField] PlayerConfig config;
        [SerializeField] KeyCode respawnKey = KeyCode.R;

        [Header("Respawn Animation")]
        [SerializeField] bool useRespawnAnimation = true;
        [SerializeField] float respawnAnimDuration = 0.35f;

        Vector3 checkpoint;
        int lives;
        float deathCooldown;
        bool aReachedWin;
        bool bReachedWin;
        bool isRespawning;

        public int Lives => lives;

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (config == null)
                throw new System.Exception("LevelManager: config not assigned");

            lives = config.MaxLives;
        }

        void Start()
        {
            Vector3 mid = Vector3.Lerp(
                playerA != null ? playerA.position : Vector3.zero,
                playerB != null ? playerB.position : Vector3.zero,
                0.5f);
            checkpoint = mid;
        }

        void OnEnable()
        {
            EventBus.On(E.PlayerFell, OnPlayerFell);
        }

        void OnDisable()
        {
            EventBus.Off(E.PlayerFell, OnPlayerFell);
        }

        void Update()
        {
            if (Input.GetKeyDown(respawnKey) && !isRespawning)
                RespawnPlayers();
        }

        public void SetCheckpoint(Vector3 position)
        {
            checkpoint = position;
        }

        void OnPlayerFell(object _)
        {
            if (Time.time < deathCooldown) return;
            deathCooldown = Time.time + 0.5f;

            lives--;
            EventBus.Emit(E.LivesChanged, lives);
            Juice.ShakeHeavy();

            if (lives <= 0)
            {
                EventBus.Emit(E.GameOver);
                return;
            }

            RespawnPlayers();
        }

        public void PlayerReachedWin(int playerIndex)
        {
            if (playerIndex == 0) aReachedWin = true;
            if (playerIndex == 1) bReachedWin = true;

            if (aReachedWin && bReachedWin)
                EventBus.Emit(E.LevelComplete);
        }

        public void RespawnPlayers()
        {
            if (useRespawnAnimation)
                StartCoroutine(RespawnAnimation());
            else
                DoRespawn();
        }

        IEnumerator RespawnAnimation()
        {
            isRespawning = true;

            var pcA = playerA != null ? playerA.GetComponent<PlayerController>() : null;
            var pcB = playerB != null ? playerB.GetComponent<PlayerController>() : null;
            var rbA = playerA != null ? playerA.GetComponent<Rigidbody2D>() : null;
            var rbB = playerB != null ? playerB.GetComponent<Rigidbody2D>() : null;
            var animA = playerA != null ? playerA.GetComponent<PlayerAnimator>() : null;
            var animB = playerB != null ? playerB.GetComponent<PlayerAnimator>() : null;
            var scaleA = animA != null ? animA.ScaleRoot : null;
            var scaleB = animB != null ? animB.ScaleRoot : null;

            // Freeze: block input, disable animators so visual freezes
            var rope = FindObjectOfType<RopeSystem>();
            if (rope != null) rope.SetVisible(false);
            if (pcA != null) pcA.IsRespawning = true;
            if (pcB != null) pcB.IsRespawning = true;
            if (animA != null) animA.enabled = false;
            if (animB != null) animB.enabled = false;
            if (rbA != null) { rbA.velocity = Vector2.zero; rbA.bodyType = RigidbodyType2D.Kinematic; }
            if (rbB != null) { rbB.velocity = Vector2.zero; rbB.bodyType = RigidbodyType2D.Kinematic; }

            Vector3 scaleAOrig = scaleA != null ? scaleA.localScale : Vector3.one;
            Vector3 scaleBOrig = scaleB != null ? scaleB.localScale : Vector3.one;

            // Scale down visual layer at death point
            float shrinkDuration = respawnAnimDuration * 0.25f;
            var am = AudioManager.Instance;
            if (am != null) am.PlayDeathSound();
            if (scaleA != null) scaleA.DOScale(Vector3.zero, shrinkDuration).SetUpdate(true);
            if (scaleB != null) scaleB.DOScale(Vector3.zero, shrinkDuration).SetUpdate(true);
            yield return new WaitForSecondsRealtime(shrinkDuration);

            // Teleport to respawn
            Vector3 respawnPos = checkpoint + Vector3.up * config.RespawnHeightOffset;
            if (playerA != null) playerA.position = respawnPos;
            if (playerB != null) playerB.position = respawnPos + Vector3.right * 0.5f;

            if (pcA != null) pcA.ResetToHuman();
            if (pcB != null) pcB.ResetToHuman();
            // ResetToHuman re-enables Dynamic, keep Kinematic until grow finishes
            if (rbA != null) rbA.bodyType = RigidbodyType2D.Kinematic;
            if (rbB != null) rbB.bodyType = RigidbodyType2D.Kinematic;
            aReachedWin = false;
            bReachedWin = false;
            EventBus.Emit(E.PlayerRespawned);

            yield return new WaitForSecondsRealtime(0.5f);

            if (am != null) am.PlayRespawn();

            // Scale visual layer back up with overshoot bounce
            float growDuration = respawnAnimDuration * 0.75f;
            if (scaleA != null) scaleA.DOScale(scaleAOrig, growDuration).SetEase(Ease.OutBack, 2.5f).SetUpdate(true);
            if (scaleB != null) scaleB.DOScale(scaleBOrig, growDuration).SetEase(Ease.OutBack, 2.5f).SetUpdate(true);
            yield return new WaitForSecondsRealtime(growDuration);

            // Restore physics and re-enable animators
            if (rbA != null) rbA.bodyType = RigidbodyType2D.Dynamic;
            if (rbB != null) rbB.bodyType = RigidbodyType2D.Dynamic;
            if (animA != null) animA.enabled = true;
            if (animB != null) animB.enabled = true;

            rope = FindObjectOfType<RopeSystem>();
            if (rope != null) rope.SetVisible(true);
            if (pcA != null) pcA.IsRespawning = false;
            if (pcB != null) pcB.IsRespawning = false;

            isRespawning = false;
        }

        void DoRespawn()
        {
            Vector3 respawnPos = checkpoint + Vector3.up * config.RespawnHeightOffset;

            var pcA = playerA != null ? playerA.GetComponent<PlayerController>() : null;
            var pcB = playerB != null ? playerB.GetComponent<PlayerController>() : null;

            if (playerA != null) playerA.position = respawnPos;
            if (playerB != null) playerB.position = respawnPos + Vector3.right * 0.5f;

            if (pcA != null) pcA.ResetToHuman();
            if (pcB != null) pcB.ResetToHuman();

            aReachedWin = false;
            bReachedWin = false;

            EventBus.Emit(E.PlayerRespawned);
        }
    }
}
