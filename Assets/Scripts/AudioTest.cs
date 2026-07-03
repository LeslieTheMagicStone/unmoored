using UnityEngine;

namespace CGJ2026
{
    public class AudioTest : MonoBehaviour
    {
        [SerializeField] AudioClip clip;

        void Start()
        {
            if (clip == null)
            {
                Debug.LogError("[AudioTest] No clip assigned — drag an .ogg from Assets/Audio/Kenney into the slot");
                return;
            }

            var src = GetComponent<AudioSource>();
            if (src == null) src = gameObject.AddComponent<AudioSource>();
            src.PlayOneShot(clip);
            Debug.Log($"[AudioTest] Played {clip.name} — you should have heard it");
        }
    }
}
