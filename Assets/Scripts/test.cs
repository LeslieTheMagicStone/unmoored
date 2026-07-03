using DG.Tweening;
using UnityEngine;

namespace CGJ2026
{
    public class test : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] Vector3 target = new Vector3(5, 0, 0);
        [SerializeField] float moveDuration = 0.8f;

        [Header("Sound Effects")]
        [SerializeField] AudioClip whooshSFX;
        [SerializeField] AudioClip impactSFX;
        [SerializeField] AudioClip landSFX;

        AudioSource audioSource;
        Renderer rend;
        Color originalColor;

        void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        void Start()
        {
            rend = GetComponent<Renderer>();
            if (rend != null) originalColor = rend.material.color;

            var seq = DOTween.Sequence();

            seq.AppendCallback(() => Play(whooshSFX));
            seq.AppendCallback(() => SetColor(Color.cyan));
            seq.Append(transform.DOMove(target, moveDuration).SetEase(Ease.OutQuad));

            seq.AppendCallback(() => Play(impactSFX));
            seq.AppendCallback(() => SetColor(Color.red));
            seq.AppendCallback(() => Shake(0.4f, 0.5f));
            seq.Append(transform.DOPunchScale(Vector3.one * 1.2f, 0.35f, 5, 0.5f));

            seq.AppendCallback(() => SetColor(new Color(1f, 0.7f, 0f)));
            seq.Append(transform.DORotate(new Vector3(0, 360, 0), 0.6f, RotateMode.FastBeyond360)
                .SetEase(Ease.InOutSine));

            seq.AppendCallback(() => Play(landSFX));
            seq.AppendCallback(() => SetColor(Color.green));
            seq.AppendCallback(() => Shake(0.2f, 0.3f));
            seq.AppendCallback(Burst);

            seq.Append(transform.DOScale(1f, 0.3f));
            seq.AppendCallback(() => SetColor(originalColor));

            seq.SetLoops(-1, LoopType.Restart);
        }

        void Play(AudioClip clip)
        {
            if (clip != null) audioSource.PlayOneShot(clip);
        }

        void Shake(float amplitude, float duration)
        {
            var cam = Camera.main;
            if (cam != null)
                cam.transform.DOShakePosition(duration, amplitude, 15, 90, false, true);
        }

        void Burst()
        {
            var go = new GameObject("Burst");
            go.hideFlags = HideFlags.HideAndDontSave;
            var ps = go.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.duration = 2f;
            main.startLifetime = 0.6f;
            main.startSpeed = 4f;
            main.startSize = 0.4f;
            main.startColor = Color.white;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 25) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.2f;

            ps.Play();
            Destroy(go, 2f);
        }

        void SetColor(Color c)
        {
            if (rend != null) rend.material.color = c;
        }

        void OnDestroy()
        {
            DOTween.Kill(transform);
            DOTween.Kill(Camera.main?.transform);
        }
    }
}
