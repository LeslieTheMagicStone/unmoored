using TMPro;
using UnityEngine;

namespace CGJ2026
{
    /// <summary>Shows the finished run's time on the GameOver screen.</summary>
    public class GameOverUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI timeText;

        void Start()
        {
            if (timeText == null)
                throw new System.Exception("GameOverUI: timeText not assigned");
            timeText.text = GameManager.FormatTime(GameManager.ElapsedTime);
        }
    }
}
