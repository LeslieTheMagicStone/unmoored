using UnityEngine;

namespace CGJ2026
{
    public class AudioManager : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] AudioClip[] walkClips;
        [SerializeField] AudioClip[] landClips;

        [Header("Actions")]
        [SerializeField] AudioClip jumpClip;
        [SerializeField] AudioClip anchorClip;
        [SerializeField] AudioClip anchorSnapClip;
        [SerializeField] AudioClip releaseClip;

        [Header("Rope")]
        [SerializeField] AudioClip ropeTautClip;

        [Header("Environment")]
        [SerializeField] AudioClip crumbleClip;

        [Header("UI / Events")]
        [SerializeField] AudioClip deathClip;
        [SerializeField] AudioClip checkpointClip;
        [SerializeField] AudioClip pauseClip;

        AudioSource source;
        AudioSource oneShotSource;
        int walkIndex;

        public static AudioManager Instance { get; private set; }

        void Awake()
        {
            Instance = this;
            source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;

            oneShotSource = gameObject.AddComponent<AudioSource>();
            oneShotSource.playOnAwake = false;
            oneShotSource.spatialBlend = 0f;
        }

        void OnEnable()
        {
            EventBus.On(E.PlayerAnchored, OnAnchor);
            EventBus.On(E.PlayerReleased, OnRelease);
            EventBus.On(E.PlayerFell, OnDeath);
            EventBus.On(E.CheckpointReached, OnCheckpoint);
        }

        void OnDisable()
        {
            EventBus.Off(E.PlayerAnchored, OnAnchor);
            EventBus.Off(E.PlayerReleased, OnRelease);
            EventBus.Off(E.PlayerFell, OnDeath);
            EventBus.Off(E.CheckpointReached, OnCheckpoint);
        }

        public void PlayWalk()
        {
            if (walkClips == null || walkClips.Length == 0) return;
            if (source.isPlaying) return;
            walkIndex = (walkIndex + 1) % walkClips.Length;
            source.pitch = Random.Range(0.95f, 1.05f);
            source.PlayOneShot(walkClips[walkIndex]);
        }

        public void PlayJump() => oneShotSource.PlayOneShot(jumpClip);

        public void PlayLand()
        {
            if (landClips == null || landClips.Length == 0) return;
            var c = landClips[Random.Range(0, landClips.Length)];
            oneShotSource.pitch = Random.Range(0.9f, 1.1f);
            oneShotSource.PlayOneShot(c);
        }

        public void PlayCrumble() => oneShotSource.PlayOneShot(crumbleClip);
        public void PlayRopeTaut() => oneShotSource.PlayOneShot(ropeTautClip);
        public void PlayAnchorSnap() => oneShotSource.PlayOneShot(anchorSnapClip);
        public void PlayPause() => oneShotSource.PlayOneShot(pauseClip);

        void OnAnchor(object _)
        {
            oneShotSource.pitch = 1f;
            oneShotSource.PlayOneShot(anchorClip);
        }

        void OnRelease(object _)
        {
            oneShotSource.pitch = 1f;
            oneShotSource.PlayOneShot(releaseClip);
        }

        void OnDeath(object _) => oneShotSource.PlayOneShot(deathClip);

        void OnCheckpoint(object _)
        {
            oneShotSource.pitch = 1f;
            oneShotSource.PlayOneShot(checkpointClip);
        }
    }
}
