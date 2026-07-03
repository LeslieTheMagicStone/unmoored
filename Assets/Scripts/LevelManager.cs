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

        Vector3 checkpoint;
        int lives;
        bool aReachedWin;
        bool bReachedWin;

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
            EventBus.Off(E.PlayerFell);
        }

        public void SetCheckpoint(Vector3 position)
        {
            checkpoint = position;
        }

        void OnPlayerFell(object _)
        {
            lives--;
            EventBus.Emit(E.LivesChanged, lives);

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
            Vector3 respawnPos = checkpoint + Vector3.up * config.RespawnHeightOffset;

            var pcA = playerA != null ? playerA.GetComponent<PlayerController>() : null;
            var pcB = playerB != null ? playerB.GetComponent<PlayerController>() : null;

            if (playerA != null) playerA.position = respawnPos;
            if (playerB != null) playerB.position = respawnPos + Vector3.right * 0.5f; // slight offset so they don't stack

            pcA?.ResetToHuman();
            pcB?.ResetToHuman();

            aReachedWin = false;
            bReachedWin = false;
        }
    }
}
