using UnityEngine;

namespace CGJ2026
{
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }

        [SerializeField] Transform playerA;
        [SerializeField] Transform playerB;
        [SerializeField] Transform winZone;

        Vector3 spawnA;
        Vector3 spawnB;
        bool aReachedWin;
        bool bReachedWin;

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void Start()
        {
            spawnA = playerA != null ? playerA.position : Vector3.zero;
            spawnB = playerB != null ? playerB.position : Vector3.zero;
        }

        void OnEnable()
        {
            EventBus.On(E.PlayerFell, OnPlayerFell);
        }

        void OnDisable()
        {
            EventBus.Off(E.PlayerFell);
        }

        void OnPlayerFell(object data)
        {
            ResetPlayers();
        }

        public void PlayerReachedWin(int playerIndex)
        {
            if (playerIndex == 0) aReachedWin = true;
            if (playerIndex == 1) bReachedWin = true;

            if (aReachedWin && bReachedWin)
                EventBus.Emit(E.LevelComplete);
        }

        public void ResetPlayers()
        {
            var pcA = playerA != null ? playerA.GetComponent<PlayerController>() : null;
            var pcB = playerB != null ? playerB.GetComponent<PlayerController>() : null;

            if (playerA != null) playerA.position = spawnA;
            if (playerB != null) playerB.position = spawnB;

            pcA?.ResetToHuman();
            pcB?.ResetToHuman();

            aReachedWin = false;
            bReachedWin = false;
        }
    }
}
