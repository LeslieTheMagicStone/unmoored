using DG.Tweening;
using UnityEngine;

namespace CGJ2026
{
    /// <summary>
    /// Persistent sound player. Gameplay code calls the Play methods directly;
    /// death and checkpoint sounds come from EventBus events.
    /// </summary>
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
        [SerializeField] AudioClip gateOpenClip;
        [SerializeField] AudioClip gateCloseClip;
        [SerializeField] AudioClip stompClip;
        [SerializeField] AudioClip[] bounceClips;

        [Header("UI / Events")]
        [SerializeField] AudioClip deathClip;
        [SerializeField] AudioClip respawnClip;
        [SerializeField] AudioClip checkpointClip;
        [SerializeField] AudioClip pauseClip;
        [SerializeField] AudioClip uiClickClip;
        [SerializeField] AudioClip uiHoverClip;

        [Header("Music")]
        [SerializeField] AudioClip musicTrack;
        [SerializeField, Range(0f, 1f)] float musicVolume = 0.5f;

        AudioSource walkSource;
        AudioSource oneShotSource;
        AudioSource loopSource;
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

            ValidateClips();

            walkSource = CreateSource(gameObject);
            oneShotSource = CreateSource(gameObject);
            loopSource = CreateSource(gameObject);
            loopSource.loop = true;

            var musicGo = new GameObject("MusicSource");
            musicGo.transform.SetParent(transform);
            musicSource = CreateSource(musicGo);
            musicSource.loop = true;
            musicSource.volume = 0f;

            musicLowPass = musicGo.AddComponent<AudioLowPassFilter>();
            musicLowPass.cutoffFrequency = LowPassCutoffOpen;
            musicLowPass.enabled = false;

            if (musicTrack != null)
                PlayMusic();
        }

        static AudioSource CreateSource(GameObject host)
        {
            var src = host.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.spatialBlend = 0f;
            return src;
        }

        public void PlayMusic(float fadeDuration = 2f)
        {
            if (musicTrack == null)
                throw new System.InvalidOperationException("AudioManager: no music track assigned");
            musicSource.DOKill();
            musicSource.clip = musicTrack;
            musicSource.Play();
            musicSource.DOFade(musicVolume, fadeDuration).SetUpdate(true);
        }

        public void StopMusic(float fadeDuration = 1f)
        {
            musicSource.DOKill();
            musicSource.DOFade(0f, fadeDuration).SetUpdate(true).OnComplete(() => musicSource.Stop());
        }

        /// <summary>Muffles the music (used by the pause menu) or opens it back up.</summary>
        public void SetMusicLowPass(bool enabled)
        {
            DOTween.Kill(musicLowPass);
            if (enabled)
            {
                musicLowPass.enabled = true;
                musicLowPass.lowpassResonanceQ = LowPassResonance;
                DOTween.To(() => musicLowPass.cutoffFrequency,
                    x => musicLowPass.cutoffFrequency = x,
                    LowPassCutoffMuffled, LowPassTransitionDuration).SetTarget(musicLowPass).SetUpdate(true);
            }
            else
            {
                DOTween.To(() => musicLowPass.cutoffFrequency,
                    x => musicLowPass.cutoffFrequency = x,
                    LowPassCutoffOpen, LowPassTransitionDuration).SetTarget(musicLowPass).SetUpdate(true)
                    .OnComplete(() =>
                    {
                        musicLowPass.enabled = false;
                        musicLowPass.lowpassResonanceQ = 1f;
                    });
            }
        }

        void OnEnable()
        {
            // A duplicate copy is already scheduled for destruction in Awake; keep it off the bus.
            if (Instance != this)
                return;
            EventBus.On(E.PlayerFell, OnDeath);
            EventBus.On(E.CheckpointReached, OnCheckpoint);
        }

        void OnDisable()
        {
            EventBus.Off(E.PlayerFell, OnDeath);
            EventBus.Off(E.CheckpointReached, OnCheckpoint);
        }

        public void PlayWalk()
        {
            if (walkSource.isPlaying)
                return;
            walkIndex = (walkIndex + 1) % walkClips.Length;
            walkSource.pitch = Random.Range(0.95f, 1.05f);
            walkSource.PlayOneShot(walkClips[walkIndex]);
        }

        public void PlayLand() => PlayVaried(landClips[Random.Range(0, landClips.Length)]);
        public void PlayBounce() => PlayVaried(bounceClips[Random.Range(0, bounceClips.Length)]);
        public void PlayJump() => Play(jumpClip);
        public void PlayCrumble() => Play(crumbleClip);
        public void PlayStomp() => Play(stompClip);
        public void PlayRopeTaut() => Play(ropeTautClip);
        public void PlayAnchorSnap() => Play(anchorSnapClip);
        public void PlayAnchorToggle(bool anchored) => Play(anchored ? anchorClip : releaseClip);
        public void PlayPause() => Play(pauseClip);
        public void PlayUIClick() => Play(uiClickClip);
        public void PlayUIHover() => Play(uiHoverClip);
        public void PlayRespawn() => Play(respawnClip);

        public void StartGateOpenLoop() => StartLoop(gateOpenClip);
        public void StartGateCloseLoop() => StartLoop(gateCloseClip);
        public void StopGateLoop() => loopSource.Stop();

        void OnDeath(object _) => Play(deathClip);
        void OnCheckpoint(object _) => Play(checkpointClip);

        void Play(AudioClip clip)
        {
            oneShotSource.pitch = 1f;
            oneShotSource.PlayOneShot(clip);
        }

        void PlayVaried(AudioClip clip)
        {
            oneShotSource.pitch = Random.Range(0.9f, 1.1f);
            oneShotSource.PlayOneShot(clip);
        }

        void StartLoop(AudioClip clip)
        {
            loopSource.clip = clip;
            loopSource.Play();
        }

        void ValidateClips()
        {
            AudioClip[] singles =
            {
                jumpClip, anchorClip, anchorSnapClip, releaseClip, ropeTautClip, crumbleClip,
                gateOpenClip, gateCloseClip, stompClip, deathClip, respawnClip, checkpointClip,
                pauseClip, uiClickClip, uiHoverClip
            };
            foreach (var clip in singles)
            {
                if (clip == null)
                    throw new System.Exception("AudioManager: a sound effect clip is not assigned");
            }

            RequireClips(walkClips, nameof(walkClips));
            RequireClips(landClips, nameof(landClips));
            RequireClips(bounceClips, nameof(bounceClips));
        }

        static void RequireClips(AudioClip[] clips, string field)
        {
            if (clips == null || clips.Length == 0)
                throw new System.Exception($"AudioManager: {field} is empty");
            foreach (var clip in clips)
            {
                if (clip == null)
                    throw new System.Exception($"AudioManager: {field} has an empty slot");
            }
        }
    }
}
