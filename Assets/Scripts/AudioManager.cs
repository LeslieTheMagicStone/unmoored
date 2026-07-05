using DG.Tweening;
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
        [SerializeField] AudioClip stompClip;

        [Header("UI / Events")]
        [SerializeField] AudioClip deathClip;
        [SerializeField] AudioClip checkpointClip;
        [SerializeField] AudioClip pauseClip;
        [SerializeField] AudioClip uiClickClip;

        [Header("Music")]
        [SerializeField] AudioClip musicTrack;
        [SerializeField, Range(0f, 1f)] float musicVolume = 0.5f;

        AudioSource source;
        AudioSource oneShotSource;
        AudioSource musicSource;
        AudioLowPassFilter musicLowPass;
        int walkIndex;

        const float LowPassCutoffMuffled = 300f;
        const float LowPassCutoffOpen = 22000f;
        const float LowPassResonance = 2f;
        const float LowPassTransitionDuration = 0.5f;

        public static AudioManager Instance { get; private set; }

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;

            oneShotSource = gameObject.AddComponent<AudioSource>();
            oneShotSource.playOnAwake = false;
            oneShotSource.spatialBlend = 0f;

            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.spatialBlend = 0f;
            musicSource.loop = true;
            musicSource.volume = 0f;

            musicLowPass = gameObject.AddComponent<AudioLowPassFilter>();
            musicLowPass.cutoffFrequency = LowPassCutoffOpen;
            musicLowPass.enabled = false;

            if (musicTrack != null)
                PlayMusic();
        }

        public void PlayMusic(float fadeDuration = 2f)
        {
            if (musicTrack == null) return;
            musicSource.clip = musicTrack;
            musicSource.Play();
            musicSource.DOFade(musicVolume, fadeDuration).SetUpdate(true);
        }

        public void StopMusic(float fadeDuration = 1f)
        {
            musicSource.DOFade(0f, fadeDuration).SetUpdate(true).OnComplete(() => musicSource.Stop());
        }

        public void SetMusicLowPass(bool enabled)
        {
            if (enabled)
            {
                musicLowPass.enabled = true;
                musicLowPass.lowpassResonanceQ = LowPassResonance;
                DOTween.To(() => musicLowPass.cutoffFrequency,
                    x => musicLowPass.cutoffFrequency = x,
                    LowPassCutoffMuffled, LowPassTransitionDuration).SetUpdate(true);
            }
            else
            {
                DOTween.To(() => musicLowPass.cutoffFrequency,
                    x => musicLowPass.cutoffFrequency = x,
                    LowPassCutoffOpen, LowPassTransitionDuration).SetUpdate(true)
                    .OnComplete(() =>
                    {
                        musicLowPass.enabled = false;
                        musicLowPass.lowpassResonanceQ = 1f;
                    });
            }
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
        public void PlayStomp() => oneShotSource.PlayOneShot(stompClip);
        public void PlayRopeTaut() => oneShotSource.PlayOneShot(ropeTautClip);
        public void PlayAnchorSnap() => oneShotSource.PlayOneShot(anchorSnapClip);
        public void PlayPause() => oneShotSource.PlayOneShot(pauseClip);
        public void PlayUIClick() => oneShotSource.PlayOneShot(uiClickClip);

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
