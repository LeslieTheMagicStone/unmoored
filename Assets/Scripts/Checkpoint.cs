using UnityEngine;

namespace CGJ2026
{
    [RequireComponent(typeof(Collider2D))]
    public class Checkpoint : MonoBehaviour
    {
        void Awake()
        {
            var col = GetComponent<Collider2D>();
            if (!col.isTrigger)
                throw new System.Exception("Checkpoint collider must be set to IsTrigger");
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<PlayerController>() == null) return;
            LevelManager.Instance?.SetCheckpoint(transform.position);
        }
    }
}
