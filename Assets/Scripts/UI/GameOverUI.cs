using TMPro;
using UnityEngine;

namespace CGJ2026
{
    public class GameOverUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI timeText;

        void Start()
        {
            if (timeText != null)
                timeText.text = GameManager.FormatTime(GameManager.ElapsedTime);
        }
    }
}
