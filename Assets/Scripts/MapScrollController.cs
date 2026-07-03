using UnityEngine;

namespace CGJ2026
{
    public class MapScrollController : MonoBehaviour
    {
        [SerializeField] Transform worldRoot;
        [SerializeField] float scrollSpeed = 1f;

        int anchoredPlayerCount;

        void Awake()
        {
            if (worldRoot == null)
                throw new System.Exception("MapScrollController: worldRoot not assigned");
        }

        void OnEnable()
        {
            EventBus.On(E.PlayerAnchored, OnPlayerAnchored);
            EventBus.On(E.PlayerReleased, OnPlayerReleased);
        }

        void OnDisable()
        {
            EventBus.Off(E.PlayerAnchored, OnPlayerAnchored);
            EventBus.Off(E.PlayerReleased, OnPlayerReleased);
        }

        void Update()
        {
            if (anchoredPlayerCount > 0) return;
            worldRoot.position += Vector3.down * scrollSpeed * Time.deltaTime;
        }

        void OnPlayerAnchored(object _)
        {
            anchoredPlayerCount++;
        }

        void OnPlayerReleased(object _)
        {
            anchoredPlayerCount = Mathf.Max(0, anchoredPlayerCount - 1);
        }
    }
}
